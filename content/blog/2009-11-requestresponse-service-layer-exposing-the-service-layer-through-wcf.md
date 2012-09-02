Note: This post is part of a series. Be sure to read the introduction <a href="http://davybrion.com/blog/2009/11/requestresponse-service-layer-series/">here</a>.

The previous posts in the series already covered everything there is to know about implementing the Request/Response Service Layer (RRSL).  In this post, we'll focus on hosting the RRSL through WCF and the next posts will show you how you can actually use the RRSL from your clients (both synchronously and asynchronously).  First, let's take another look at what our service actually exposes:

<div>
[csharp]
    public interface IRequestProcessor : IDisposable
    {
        Response[] Process(params Request[] requests);
    }
[/csharp]
</div>

This interface doesn't contain the necessary WCF attributes, but we'll get to that in a minute.  The first thing that we need to take care of is to make sure that WCF's DataContractSerializer can handle the derived Request and Response types without us having to declare the KnownType attribute on each derived class.  Luckily for us, WCF has a ServiceKnownType attribute where you can define a class that will provide the KnownTypes to WCF so it can properly serialize and deserialize them.  But first, we'll need a KnownTypeProvider class:

<div>
[csharp]
    public static class KnownTypeProvider
    {
        private static HashSet&lt;Type&gt; knownTypes = new HashSet&lt;Type&gt;();
 
        public static void ClearAllKnownTypes()
        {
            knownTypes = new HashSet&lt;Type&gt;();
        }
 
        public static void Register&lt;T&gt;()
        {
            Register(typeof(T));
        }
 
        public static void Register(Type type)
        {
            knownTypes.Add(type);
        }
 
        public static void RegisterDerivedTypesOf&lt;T&gt;(Assembly assembly)
        {
            RegisterDerivedTypesOf(typeof(T), assembly);
        }
 
        public static void RegisterDerivedTypesOf&lt;T&gt;(IEnumerable&lt;Type&gt; types)
        {
            RegisterDerivedTypesOf(typeof(T), types);
        }
 
        public static void RegisterDerivedTypesOf(Type type, Assembly assembly)
        {
            RegisterDerivedTypesOf(type, assembly.GetTypes());
        }
 
        public static void RegisterDerivedTypesOf(Type type, IEnumerable&lt;Type&gt; types)
        {
            knownTypes.UnionWith(GetDerivedTypesOf(type, types));
        }
 
        public static IEnumerable&lt;Type&gt; GetKnownTypes(ICustomAttributeProvider provider)
        {
            return knownTypes;
        }
 
        private static List&lt;Type&gt; GetDerivedTypesOf(Type baseType, IEnumerable&lt;Type&gt; types)
        {
            return types.Where(t =&gt; !t.IsAbstract &amp;&amp; t.IsSubclassOf(baseType)).ToList();
        }
    }
[/csharp]
</div>

Now we just have to make sure that all of our Request and Response types are registered before the application is initialized.  You basically need to make sure that the following code is called before the first request is sent to the service:

<div>
[csharp]
        private static void RegisterRequestAndResponseTypes(Assembly assembly)
        {
            KnownTypeProvider.RegisterDerivedTypesOf&lt;Request&gt;(assembly);
            KnownTypeProvider.RegisterDerivedTypesOf&lt;Response&gt;(assembly);
        }
[/csharp]
</div>

Now we can actually define our Service Contract.  Instead of putting the WCF attributes on the IRequestProcessor interface, i decided to put a different interface next to it.  The reason i chose to go that route is merely to make sure that the actual IRequestProcessor interface and it's implementation isn't tied to WCF.  And that's why we have the IWcfRequestProcessor interface:

<div>
[csharp]
    [ServiceContract]
    public interface IWcfRequestProcessor
    {
        [OperationContract(Name = &quot;ProcessRequests&quot;)]
        [ServiceKnownType(&quot;GetKnownTypes&quot;, typeof(KnownTypeProvider))]
        Response[] Process(params Request[] requests);
    }
[/csharp]
</div>

This is a 'properly' defined WCF Service Contract, and we also make sure that WCF knows where to get the list of known types by using the ServiceKnownType attribute and hooking it to our KnownTypeProvider.  The implementation of this service contract looks like this:

<div>
[csharp]
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.PerCall)]
    public class WcfRequestProcessor : IWcfRequestProcessor
    {
        public Response[] Process(params Request[] requests)
        {
            return IoC.Container.Resolve&lt;IRequestProcessor&gt;().Process(requests);
        }
    }
[/csharp]
</div>

As you can see, there's nothing to it... The implementation of the Process method simply resolves the real IRequestProcessor implementation, calls the Process method and returns the Responses.  That's it.

Now we still need to host the service somewhere.  I always host it through IIS but you can use any of the other WCF hosting options as well.  In case you're hosting it in a Web Application, add a RequestProcessorService.svc file to your Web Application with the following content:

<div>
[xml]
&lt;%@ ServiceHost Language=&quot;C#&quot; Debug=&quot;true&quot; Service=&quot;The.Namespace.Of.Your.WcfRequestProcessor&quot; %&gt;
[/xml]
</div>

In your web.config (or app.config if you're hosting it yourself or through a windows service), add the following block:

<div>
[xml]
  &lt;system.serviceModel&gt;
 
    &lt;services&gt;
      &lt;service name=&quot;The.Namespace.Of.Your.WcfRequestProcessor&quot; behaviorConfiguration=&quot;RequestProcessorBehavior&quot;&gt;
        &lt;endpoint address=&quot;&quot; contract=&quot;The.Namespace.Of.Your.IWcfRequestProcessor&quot;
        binding=&quot;customBinding&quot; bindingConfiguration=&quot;binaryHttpBinding&quot;/&gt;
      &lt;/service&gt;
    &lt;/services&gt;
 
    &lt;bindings&gt;
      &lt;customBinding&gt;
        &lt;binding name=&quot;binaryHttpBinding&quot; receiveTimeout=&quot;00:30:00&quot; sendTimeout=&quot;00:30:00&quot; &gt;
          &lt;binaryMessageEncoding&gt;
            &lt;readerQuotas maxStringContentLength=&quot;2147483647&quot; maxArrayLength=&quot;2147483647&quot; /&gt;
          &lt;/binaryMessageEncoding&gt;
          &lt;httpTransport maxReceivedMessageSize=&quot;2147483647&quot; maxBufferSize=&quot;2147483647&quot; /&gt;
        &lt;/binding&gt;
      &lt;/customBinding&gt;
 
       &lt;basicHttpBinding&gt;
          &lt;binding name=&quot;basicHttpBinding&quot; receiveTimeout=&quot;00:30:00&quot;
             sendTimeout=&quot;00:30:00&quot; maxReceivedMessageSize=&quot;2147483647&quot;&gt;
             &lt;readerQuotas maxStringContentLength=&quot;2147483647&quot; maxArrayLength=&quot;2147483647&quot; /&gt;
             &lt;security mode=&quot;None&quot; /&gt;
          &lt;/binding&gt;
       &lt;/basicHttpBinding&gt;
    &lt;/bindings&gt;
 
    &lt;behaviors&gt;
      &lt;serviceBehaviors&gt;
        &lt;behavior name=&quot;RequestProcessorBehavior&quot;&gt;
          &lt;serviceMetadata httpGetEnabled=&quot;true&quot; /&gt;
          &lt;serviceDebug includeExceptionDetailInFaults=&quot;true&quot;/&gt;
          &lt;dataContractSerializer maxItemsInObjectGraph=&quot;2147483647&quot;/&gt;
          &lt;serviceThrottling maxConcurrentCalls=&quot;100&quot; maxConcurrentInstances=&quot;100&quot; /&gt;
        &lt;/behavior&gt;
      &lt;/serviceBehaviors&gt;
    &lt;/behaviors&gt;
 
  &lt;/system.serviceModel&gt;
[/xml]
</div>

I included two binding configurations in this example, but you obviously only need one.  In this case, the service endpoint is configured to use the custom binaryHttpBinding.  This uses binary XML which (in most cases) is more compact and thus, has less bandwidth overhead.  If you want to use a regular basicHttpBinding, just modify the service endpoint configuration to point to the other binding.  Obviously, you can use wsHttpBinding or any other binding as well if you want to.

And that is it.  That's really all you need to do to host the RRSL through WCF.  You'll never have to edit or add anything to the xml configuration as you add more functionality to your service layer.  It works now, and it will keep working.

Just keep in mind that you need to register the Request and Response types with the KnownTypeProvider <em>before</em> the service receives the first request ;)