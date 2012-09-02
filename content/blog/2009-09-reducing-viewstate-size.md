I dislike ViewState as much as the next guy, but when you're working with ASP.NET WebForms, you just can't avoid it.  In some cases, the size of the ViewState can become so big that it significantly increases the load time of pages due to the extra bandwidth consumption.  The correct solution would obviously be to reduce the size of the ViewState in those pages as much as you can, but it's not always feasible to do so.  So we wanted a more general 'solution', and i found <a href="http://aspadvice.com/blogs/mamanzes_blog/archive/2006/08/27/Save-some-space_2C00_-compress-that-ViewState.aspx">this post</a> which discusses compressing the ViewState before you send it to the client and decompressing it when the client sends it back.  We used pretty much the same approach, but with some differences.

First of all, ViewState is persisted in the resulting HTML page through an IStateFormatter object.  We'll provide our own CompressedStateFormatter which implements the IStateFormatter interface, and uses the standard IStateFormatter that ASP.NET uses:

<div>
[csharp]
    public class CompressedStateFormatter : IStateFormatter
    {
        private readonly IStateFormatter actualFormatter;
 
        public CompressedStateFormatter(IStateFormatter actualFormatter)
        {
            this.actualFormatter = actualFormatter;
        }
 
        public string Serialize(object state)
        {
            string decompressedState = actualFormatter.Serialize(state);
 
            using (MemoryStream memoryStream = new MemoryStream())
            {
                using (Stream zipStream = new GZipStream(memoryStream, CompressionMode.Compress))
                using (StreamWriter writer = new StreamWriter(zipStream))
                {
                    writer.Write(decompressedState);
                }
 
                return Convert.ToBase64String(memoryStream.ToArray());
            }
        }
 
        public object Deserialize(string serializedState)
        {
            byte[] data = Convert.FromBase64String(serializedState);
 
            using (MemoryStream memoryStream = new MemoryStream(data))
            using (Stream zippedStream = new GZipStream(memoryStream, CompressionMode.Decompress))
            using (StreamReader reader = new StreamReader(zippedStream))
            {
                return actualFormatter.Deserialize(reader.ReadToEnd());
            }
        }
    }
[/csharp]
</div>

The idea is very simple: when the Serialize method is called, we first call the real formatter's Serialize method, compress its return value and then return the Base64-encoded string of the compressed serialized state.  And in the Deserialize method, we do the exact opposite: we first decompress the Base64-encoded string and then we use the real formatter to deserialize the actual ViewState.

In Mamanze's example, he checks to see if the compressed version is actually smaller than the decompressed version and if so, uses the decompressed version instead of the compressed one.  And when decompressing he first checks to see if it's a compressed or decompressed version and obviously only decompresses in case of a compressed version.  The only page where i found the compressed version of the ViewState to be larger than the decompressed version was in our log in page, so i just got rid of that piece of the code.

Now we still have to plug this into ASP.NET's behavior somehow... first we add a pagestate.browser file to the App_Browsers folder of your web application (if it doesn't exist, just create it) with the following content:

<div>
[xml]
&lt;browsers&gt;
  &lt;browser refID=&quot;Default&quot;&gt;
    &lt;controlAdapters&gt;
      &lt;adapter controlType=&quot;System.Web.UI.Page&quot; adapterType=&quot;Our.Application.CompressedPageStateAdapter&quot; /&gt;
    &lt;/controlAdapters&gt;
  &lt;/browser&gt;
&lt;/browsers&gt;
[/xml]
</div>

The CompressedPageStateAdapter looks like this:

<div>
[csharp]
    public class CompressedPageStateAdapter : PageAdapter
    {
        public override PageStatePersister GetStatePersister()
        {
            return new CompressedHiddenFieldPageStatePersister(Page);
        }
    }
[/csharp]
</div>

And the CompressedHiddenFieldPageStatePersister class looks like this:

<div>
[csharp]
    public class CompressedHiddenFieldPageStatePersister : HiddenFieldPageStatePersister
    {
        public CompressedHiddenFieldPageStatePersister(Page page) : base(page)
        {
            FieldInfo field = typeof(PageStatePersister).GetField(&quot;_stateFormatter&quot;, BindingFlags.NonPublic | BindingFlags.Instance);
            // retrieving this property instantiates the default IStateFormatter
            var defaultFormatter = base.StateFormatter;
            var formatter = new CompressedStateFormatter(defaultFormatter);
            field.SetValue(this, formatter);
        }
    }
[/csharp]
</div>

The HiddenFieldPageStatePersister is the class that ASP.NET WebForms will use by default to store your ViewState into a hidden field in the resulting HTML.  By default, the HiddenFieldPageStatePersister uses the default IStateFormatter type that ASP.NET uses, which only uses Base64 encoding but no compression.  Unfortunately, there is no clean way to instruct ASP.NET to use a different implementation for IStateFormatter, so we need to use a bit of reflection to overwrite the value of HiddenFieldPageStatePersister's _stateFormatter field.  Luckily, this also enables us to first get the value of the StateFormatter property so we can pass this reference (which is the 'real' formatter) to our CompressedStateFormatter.

And that is all there is to it... all of your pages will now use this CompressedHiddenFieldPageStatePersister so you get the benefit of ViewState compression in each of your pages.  You can also do this selectively if you want, by not using the pagestate.browser file and overriding the PageStatePersister property of your ASPX page:

<div>
[csharp]
        private CompressedHiddenFieldPageStatePersister persister;
 
        protected override PageStatePersister PageStatePersister
        {
            get
            {
                if (persister == null)
                {
                    persister = new CompressedHiddenFieldPageStatePersister(this);
                }
 
                return persister;
            }
        }
[/csharp]
</div>

This way, only the pages that contain this code will use the CompressedHiddenFieldPageStatePersister.

Instead of inheriting from HiddenFieldPageStatePersister, you could also inherit from SessionPageStatePersister.  SessionPageStatePersister will store your ViewState in the HttpSessionState, and will only include a little bit of ViewState in your HTML page instead of everything.  But you do need to be aware of the fact that using the CompressedStateFormatter when inheriting from SessionPageStatePersister will only result in compressing the little bit of ViewState that is included in the HTML, and <strong>not</strong> the ViewState that is stored in the HttpSessionState. 

In case you're wondering: why should i use this instead of using typical HTTP compression on the IIS level?  I believe it has a couple of advantages to HTTP compression.  First of all, AFAIK, HTTP compression does not have any benefit on postbacks.  And since ViewState is always posted back to the server, this can make a pretty big difference.  Also, with this approach, the client will not have to decompress the entire ViewState (which isn't used client-side anyway) and the browser doesn't have to waste time on it in general.

I haven't used this in production yet, but i will very soon... unless someone knows of a good reason why i shouldn't ;)