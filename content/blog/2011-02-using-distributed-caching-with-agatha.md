As you may or may not know, Agatha has supported <a href="http://davybrion.com/blog/2010/06/using-agathas-server-side-caching/">server-side caching of responses</a> for a while now (it also sports built-in <a href="http://davybrion.com/blog/2010/08/using-agathas-client-side-caching/">client-side caching</a> actually). But it only came with one in-memory implementation of that cache. And while that implementation works well, it's still just an in-process cache which just isn't sufficient for some scenarios.

This week i was introduced to <a href="http://www.membase.org/">Membase</a>, a great distributed caching solution which is very easy to set up. I wanted to see what it would take to make Agatha's server-side caching work with Membase. With a little help from the <a href="http://memcached.enyim.com/">Enyim Membase client</a>, it turned out to be very easy.  If you want to change the actual caching implementation that Agatha uses, you have to implement 2 interfaces. First, you'll need a custom implementation of the ICache interface:

<div>
[csharp]
    public class MembaseCache : ICache
    {
        private readonly MembaseClient membaseClient;

        public MembaseCache(string region = null)
        {
            // this implementation assumes password-less buckets
            membaseClient = new MembaseClient(region, null);
        }

        public Response GetCachedResponseFor(Request request)
        {
            return membaseClient.Get&lt;Response&gt;(GetKey(request));
        }

        public void Store(Request request, Response response, TimeSpan expiration)
        {
            membaseClient.Store(StoreMode.Set, GetKey(request), response, expiration);
        }

        public void Clear()
        {
            membaseClient.FlushAll();
        }

        private static string GetKey(Request request)
        {
            return string.Format(&quot;{0}_{1}&quot;, request.GetType().FullName, request.GetHashCode());
        }
    }
[/csharp]
</div>

With Agatha's caching, you can use regions in your caching configuration. A region corresponds with a bucket in Membase.  If you don't specify a region name when configuring caching for a response, Agatha will use the default region which is named _defaultRegion.  You will need to create at least the _defaultRegion bucket in your Membase cluster, and you'll also need to create a bucket for each other region you use in your caching configuration.  When your service layer is initialized, Agatha will create an ICache instance for each known region to be used. 

Then you'll need an ICacheProvider implementation:

<div>
[csharp]
    public class MembaseCacheProvider : ICacheProvider
    {
        public ICache BuildCache(string region)
        {
            return new MembaseCache(region);
        }
    }
[/csharp]
</div>

Now, because the Enyim Membase client uses binary serialization of cached objects by default, we're going to provide our own ITranscoder (defined in the Enyim assembly) implementation which uses the DataContractSerializer:

<div>
[csharp]
    public class MembaseTranscoder : ITranscoder
    {
        public CacheItem Serialize(object o)
        {
            using (var stream = new MemoryStream())
            {
                var serializer = new DataContractSerializer(typeof(Response), KnownTypeProvider.GetKnownTypes(null));
                serializer.WriteObject(stream, o);
                var data = stream.ToArray();

                return new CacheItem(((ushort)(((ushort)Type.GetTypeCode(o.GetType())) | 0x100)),
                    new ArraySegment&lt;byte&gt;(data, 0, data.Length));
            }
        }

        public object Deserialize(CacheItem item)
        {
            using (var stream = new MemoryStream(item.Data.Array.Skip(item.Data.Offset).ToArray()))
            {
                var serializer = new DataContractSerializer(typeof(Response), KnownTypeProvider.GetKnownTypes(null));
                return serializer.ReadObject(stream);
            }
        }
    }
[/csharp]
</div>

That's actually all we need to support distributed response caching with Membase.  To use this, you'd need to add this to the configuration file of your service host:

<div>
[xml]
  &lt;membase&gt;
    &lt;servers bucket=&quot;_defaultRegion&quot;&gt;
      &lt;add uri=&quot;http://localhost:8091/pools/default&quot; /&gt;
    &lt;/servers&gt;
    &lt;transcoder type=&quot;Agatha.Common.Caching.MembaseTranscoder, Agatha.Common&quot; /&gt;
  &lt;/membase&gt;
[/xml]
</div>

Obviously, you'd need a bucket definition for every region that you'd use and you'll probably need a different bucket URI as well ;)

You'd also need to configure Agatha to use the MembaseCacheProvider implemention:

<div>
[csharp]
            var config = new ServiceLayerConfiguration(Assembly.GetExecutingAssembly(), 
                typeof(HelloWorldRequest).Assembly, typeof(Agatha.Castle.Container))
                            {
                                CacheProviderImplementation = typeof(MembaseCacheProvider)
                            };
            config.Initialize();
[/csharp]
</div>

And that's it... distributed caching of service-layer responses has never been this easy ;)

Note that i haven't committed this implementation to Agatha's Subversion repository... the plan is to add it in the 2.0 version, which will have many more changes (more on that in a future post).  But if you need it already, or you need inspiration for an implementation that targets a different distributed caching server, the information in this post should get you going.