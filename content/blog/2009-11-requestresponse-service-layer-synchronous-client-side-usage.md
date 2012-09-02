Note: This post is part of a series. Be sure to read the introduction <a href="http://davybrion.com/blog/2009/11/requestresponse-service-layer-series/">here</a>.

One of my biggest issues with using typical WCF Services is that you always need to update your client-side proxies whenever a new operation is added to your service layer.  I definitely wanted to avoid that with the Request/Response Service Layer, and that was easy to do because this is our only Service Contract:

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

As you can see there's only one Operation on this service, and because each 'operation' of our service layer is exposed as a Request/Response combination, we'll never have to add anything else to this Service Contract and thus, we only need to create our proxy once and we'll never have to update it.

Instead of generating a service proxy with SvcUtil, i chose to just inherit from WCF's ClientBase class:

<div>
[csharp]
    public class RequestProcessorProxy : ClientBase&lt;IWcfRequestProcessor&gt;, IRequestProcessor
    {
        public RequestProcessorProxy() {}
 
        public RequestProcessorProxy(string endpointConfigurationName)
            : base(endpointConfigurationName) {}
 
        public RequestProcessorProxy(string endpointConfigurationName, string remoteAddress)
            : base(endpointConfigurationName, remoteAddress) {}
 
        public RequestProcessorProxy(string endpointConfigurationName, EndpointAddress remoteAddress)
            : base(endpointConfigurationName, remoteAddress) {}
 
        public RequestProcessorProxy(Binding binding, EndpointAddress remoteAddress)
            : base(binding, remoteAddress) {}
 
        public RequestProcessorProxy(InstanceContext callbackInstance)
            : base(callbackInstance) {}
 
        public RequestProcessorProxy(InstanceContext callbackInstance, string endpointConfigurationName)
            : base(callbackInstance, endpointConfigurationName) {}
 
        public RequestProcessorProxy(InstanceContext callbackInstance, string endpointConfigurationName, string remoteAddress)
            : base(callbackInstance, endpointConfigurationName, remoteAddress) {}
 
        public RequestProcessorProxy(InstanceContext callbackInstance, string endpointConfigurationName, EndpointAddress remoteAddress)
            : base(callbackInstance, endpointConfigurationName, remoteAddress) {}
 
        public RequestProcessorProxy(InstanceContext callbackInstance, Binding binding, EndpointAddress remoteAddress)
            : base(callbackInstance, binding, remoteAddress) {}
 
        public Response[] Process(params Request[] requests)
        {
            return Channel.Process(requests);
        }
 
        public void Dispose()
        {
            try
            {
                Close();
            }
            catch (Exception)
            {
                Abort();
            }
        }
    }
[/csharp]
</div>

And that is it as far as the proxy is concerned.  The implementation of the Dispose method isn't very clean, but that is to work around an issue that WCF has where calling the Close method can throw an Exception (and as you know, your Dispose methods should <strong>never, ever</strong> throw an Exception) and when it does, you absolutely need to call the Abort method of the proxy.  If you don't, the Channel will remain open and once you hit the limit of concurrent open channels on your service, no requests will be able to be handled by the service.  Why yes, we definitely learned that one the hard way ;)

Now, this proxy is already enough to be able to make synchronous calls to the RRSL from your client code, as long as you put the following WCF configuration in your web.config or app.config file:

<div>
[xml]
  &lt;system.serviceModel&gt;
    &lt;bindings&gt;
      &lt;wsHttpBinding&gt;
        &lt;binding name=&quot;RequestProcessorBindingConfiguration_HTTPS&quot; maxReceivedMessageSize=&quot;2147483647&quot; sendTimeout=&quot;00:30&quot;&gt;
          &lt;readerQuotas maxStringContentLength=&quot;2147483647&quot; maxArrayLength=&quot;2147483647&quot; /&gt;
          &lt;security mode=&quot;Transport&quot;&gt;
            &lt;transport clientCredentialType=&quot;None&quot;/&gt;
          &lt;/security&gt;
        &lt;/binding&gt;
      &lt;/wsHttpBinding&gt;
    &lt;/bindings&gt;
    &lt;behaviors&gt;
      &lt;serviceBehaviors&gt;
        &lt;behavior name=&quot;RequestProcessorBehavior&quot;&gt;
          &lt;dataContractSerializer maxItemsInObjectGraph=&quot;2147483647&quot;/&gt;
        &lt;/behavior&gt;
      &lt;/serviceBehaviors&gt;
    &lt;/behaviors&gt;
    &lt;client&gt;
      &lt;endpoint binding=&quot;wsHttpBinding&quot; name=&quot;IRequestProcessor&quot; bindingConfiguration=&quot;RequestProcessorBindingConfiguration_HTTPS&quot;
                behaviorConfiguration=&quot;RequestProcessorServiceBehavior&quot; contract=&quot;Namespace.Of.Your.IWcfRequestProcessor&quot; /&gt;
    &lt;/client&gt;
  &lt;/system.serviceModel&gt;
[/xml]
</div>

In this example we're connecting to a RRSL that is exposed through the wsHttpBinding over HTTPS.  Obviously, you can use any of the other WCF bindings and configurations as well.  

Also, you'll need to register your Request and Response types with the KnownTypeProvider (just as you had to do in your service host) before you start sending requests to the service.  You can do that with something like this:

<div>
[csharp]
        private static void RegisterRequestAndResponseTypes(Assembly assembly)
        {
            KnownTypeProvider.RegisterDerivedTypesOf&lt;Request&gt;(assembly);
            KnownTypeProvider.RegisterDerivedTypesOf&lt;Response&gt;(assembly);
        }
[/csharp]
</div>

So, that is all you need to use the RRSL synchronously.  But using the Process method of the RequestProcessorProxy is clumsy and even prone to errors because you'd have to keep track of the indexes of the requests and the responses.  It certainly isn't a nice API.  So to fix that, we also have the IRequestDispatcher interface which offers a much cleaner and less error prone API:

<div>
[csharp]
    public interface IRequestDispatcher : IDisposable
    {
        IEnumerable&lt;Response&gt; Responses { get; }
 
        void Add(Request request);
        void Add(params Request[] requestsToAdd);
        void Add(string key, Request request);
        bool HasResponse&lt;TResponse&gt;() where TResponse : Response;
        TResponse Get&lt;TResponse&gt;() where TResponse : Response;
        TResponse Get&lt;TResponse&gt;(string key) where TResponse : Response;
        TResponse Get&lt;TResponse&gt;(Request request) where TResponse : Response;
        void Clear();
    }
[/csharp]
</div>

The idea is that you can add requests to an IRequestDispatcher in a clean manner, and then simply retrieve the responses you want based on the type of the response.  If you need to send multiple requests of the same type in a single batch, you need to provide a string key to identify its response because another response will be of the same type.  You can add as many requests as you want, and the requests won't be sent to the service until you actually try to retrieve one of the responses.  Once you try to retrieve a response, all of the batched requests will be sent to the service in a single roundtrip and the responses are stored in the IRequestDispatcher until you retrieve them or until the IRequestDispatcher is cleared (or garbage collected).  

The implementation of the IRequestDispatcher will use an IRequestProcessor instance to send the requests to the Request Processor (covered in an earlier post in this series).  If you're using the RRSL over WCF, then you better configure your IOC container to create instances of the RequestProcessorProxy class whenever an instance of IRequestProcessor is requested by your code.  This also means that you can just as well run the RRSL within the same process as your client code if you don't actually need it to run in a separate process (or service) and just want the architectural benefits of this approach.  You could do this by simply configuring your IOC container to return the actual RequestProcessor implementation instead of the WCF proxy. 

The implementation of the RequestDispatcher class looks like this:

<div>
[csharp]
    public abstract class RequestDispatcher : Disposable, IRequestDispatcher
    {
        private readonly IRequestProcessor requestProcessor;
 
        private Dictionary&lt;string, Type&gt; keyToTypes;
        protected Dictionary&lt;string, int&gt; keyToResultPositions;
        private List&lt;Request&gt; requests;
        private Response[] responses;
 
        protected RequestDispatcher(IRequestProcessor requestProcessor)
        {
            this.requestProcessor = requestProcessor;
            InitializeState();
        }
 
        private void InitializeState()
        {
            requests = new List&lt;Request&gt;();
            responses = null;
            keyToTypes = new Dictionary&lt;string, Type&gt;();
            keyToResultPositions = new Dictionary&lt;string, int&gt;();
        }
 
        public IEnumerable&lt;Request&gt; SentRequests
        {
            get { return requests; }
        }
 
        public IEnumerable&lt;Response&gt; Responses
        {
            get
            {
                SendRequestsIfNecessary();
                return responses;
            }
        }
 
        public virtual void Add(params Request[] requestsToAdd)
        {
            foreach (var request in requestsToAdd)
            {
                Add(request);
            }
        }
 
        public virtual void Add(Request request)
        {
            AddRequest(request, false);
        }
 
        public virtual void Add(string key, Request request)
        {
            AddRequest(request, true);
            keyToTypes[key] = request.GetType();
            keyToResultPositions[key] = requests.Count - 1;
        }
 
        public virtual bool HasResponse&lt;TResponse&gt;() where TResponse : Response
        {
            SendRequestsIfNecessary();
            return responses.OfType&lt;TResponse&gt;().Count() &gt; 0;
        }
 
        public virtual TResponse Get&lt;TResponse&gt;() where TResponse : Response
        {
            SendRequestsIfNecessary();
            return responses.OfType&lt;TResponse&gt;().Single();
        }
 
        public virtual TResponse Get&lt;TResponse&gt;(string key) where TResponse : Response
        {
            SendRequestsIfNecessary();
            return (TResponse)responses[keyToResultPositions[key]];
        }
 
        public virtual TResponse Get&lt;TResponse&gt;(Request request) where TResponse : Response
        {
            Add(request);
            return Get&lt;TResponse&gt;();
        }
 
        public virtual void Clear()
        {
            InitializeState();
        }
 
        protected override void DisposeManagedResources()
        {
            if (requestProcessor != null) requestProcessor.Dispose();
        }
 
        protected virtual Response[] GetResponses(params Request[] requestsToProcess)
        {
            BeforeSendingRequests(requestsToProcess);
            return requestProcessor.Process(requestsToProcess);
        }
 
        protected virtual void BeforeSendingRequests(IEnumerable&lt;Request&gt; requestsToProcess) { }
 
        private void SendRequestsIfNecessary()
        {
            if (responses == null)
            {
                responses = GetResponses(requests.ToArray());
                DealWithPossibleExceptions(responses);
            }
        }
 
        private void DealWithPossibleExceptions(IEnumerable&lt;Response&gt; responsesToCheck)
        {
            foreach (var response in responsesToCheck)
            {
                if (response.ExceptionType == ExceptionType.Security)
                {
                    DealWithSecurityException(response.Exception);
                }
 
                if (response.ExceptionType == ExceptionType.Unknown)
                {
                    DealWithUnknownException(response.Exception);
                }
            }
        }
 
        protected abstract void DealWithUnknownException(ExceptionInfo exception);
        protected abstract void DealWithSecurityException(ExceptionInfo exceptionDetail);
 
        private void AddRequest(Request request, bool wasAddedWithKey)
        {
            Type requestType = request.GetType();
 
            if (RequestTypeIsAlreadyPresent(requestType) &amp;&amp;
                (RequestTypeIsNotAssociatedWithKey(requestType) || !wasAddedWithKey))
            {
                throw new InvalidOperationException(String.Format(&quot;A request of type {0} has already been added. &quot;
                                                                  + &quot;Please add requests of the same type with a different key.&quot;, requestType.FullName));
            }
 
            requests.Add(request);
        }
 
        private bool RequestTypeIsNotAssociatedWithKey(Type requestType)
        {
            return !keyToTypes.Values.Contains(requestType);
        }
 
        private bool RequestTypeIsAlreadyPresent(Type requestType)
        {
            return requests.Count(r =&gt; r.GetType().Equals(requestType)) &gt; 0;
        }
    }
[/csharp]
</div>

There's nothing special about this class, except for 3 things maybe.  First of all, it's an abstract class with 2 abstract methods.  So obviously, your application needs to inherit from this RequestDispatcher implementation (or can provide its own implementation as long as it implements the IRequestDispatcher interface) and implement the abstract DealWithSecurityException and DealWithUnknownException methods.  We found this to be useful because most web applications will return you to a general "Oops, something went wrong" page or "You don't have enough clout in this world to perform the task you were trying to do" page when unknown exceptions or security exceptions occur.   If you don't want any of that to happen and simply deal with the exception in the calling code, the implementations of these methods can just do nothing, or only do something in certain conditions or whatever else that fits your purpose or intentions.

There's also a virtual BeforeSendingRequests methods which you can implement.  We typically override that method to add authentication data (such as the current user's username and password hash) to each request.

That's pretty much all there is to using the RRSL from your client code in a synchronous manner with a pretty clean API.