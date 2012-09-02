Note: This post is part of a series. Be sure to read the introduction <a href="http://davybrion.com/blog/2009/11/requestresponse-service-layer-series/">here</a>.

First of all, i would like to mention that i am by no means an expert on WCF and asynchronous operations, so it is quite possible that some of the things in this post could be done easier by someone who knows more about it.  Most of the code in this post wasn't written by me either, but by my co-worker Tom Ceulemans (who unfortunately doesn't have a blog that i can link to).  What you'll see in this post does work and it actually works very well.  But as i said, there very well might be room for some nice improvements here.  Anyways, let's get to it.

As you know by now, our Service Contract for the Request/Response Service Layer (RRSL) looks like this:

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

As you can see, this Service Contract doesn't define any asynchronous operations.  We don't need to define them in the original contract, but we do have to use an asynchronous version of this Service Contract in the client from which we want to make asynchronous calls to our RRSL.  So client-side, we use the following version of the IWcfRequestProcessor Service Contract:

<div>
[csharp]
    [ServiceContract(ConfigurationName = &quot;Namespace.Of.Your.IWcfRequestProcessor&quot;)]
    public interface IWcfRequestProcessor : IDisposable
    {
        [OperationContract(AsyncPattern = true, Name = &quot;ProcessRequests&quot;)]
        [ServiceKnownType(&quot;GetAllKnownTypes&quot;, typeof(KnownTypeProvider))]
        IAsyncResult BeginProcessRequests(Request[] requests, AsyncCallback callback, object asyncState);
 
        Response[] EndProcessRequests(IAsyncResult result);
 
        void ProcessRequestsAsync(Request[] requests, Action&lt;ProcessRequestsAsyncCompletedArgs&gt; processCompleted);
    }
[/csharp]
</div>

This one only defines the asynchronous version of the ProcessRequests operation, which in our case is all we need since we use this in our Silverlight applications where you never make synchronous remote calls.

The ProcessRequestsAsyncCompletedArgs class looks like this:

<div>
[csharp]
    public class ProcessRequestsAsyncCompletedArgs : System.ComponentModel.AsyncCompletedEventArgs
    {
        private readonly object[] results;
 
        public ProcessRequestsAsyncCompletedArgs(object[] results, Exception exception, bool cancelled, object userState) :
            base(exception, cancelled, userState)
        {
            this.results = results;
        }
 
        public Response[] Result
        {
            get
            {
                RaiseExceptionIfNecessary();
                return ((Response[])(results[0]));
            }
        }
    }
[/csharp]
</div>

Now we need a proxy class to implement the asynchronous version of the IWcfRequestProcessor interface.  The code of the AsyncWcfRequestProcessorProxy code is somewhat low-level because it's (obviously) dealing with all of the async stuff, so there's not really a lot of need to get into the details of this piece of code.  If you're implementing your own RRSL, just copy this code and be glad that you didn't have to write it ;)

<div>
[csharp]
    public class AsyncWcfRequestProcessorProxy : ClientBase&lt;IWcfRequestProcessor&gt;, IWcfRequestProcessor
    {
        public event EventHandler&lt;AsyncCompletedEventArgs&gt; OpenCompleted;
 
        public AsyncWcfRequestProcessorProxy() {}
 
        public AsyncWcfRequestProcessorProxy(string endpointConfigurationName, string remoteAddress)
            : base(endpointConfigurationName, remoteAddress) {}
 
        IAsyncResult IWcfRequestProcessor.BeginProcessRequests(Request[] requests, AsyncCallback callback, object asyncState)
        {
            return Channel.BeginProcessRequests(requests, callback, asyncState);
        }
 
        [System.Diagnostics.DebuggerHidden]
        Response[] IWcfRequestProcessor.EndProcessRequests(IAsyncResult result)
        {
            return Channel.EndProcessRequests(result);
        }
 
        private IAsyncResult OnBeginProcessRequests(object[] inValues, AsyncCallback callback, object asyncState)
        {
            var requests = ((Request[])(inValues[0]));
            return ((IWcfRequestProcessor)(this)).BeginProcessRequests(requests, callback, asyncState);
        }
 
        [System.Diagnostics.DebuggerHidden]
        private object[] OnEndProcessRequests(IAsyncResult result)
        {
            Response[] retVal = ((IWcfRequestProcessor)(this)).EndProcessRequests(result);
            return new object[] { retVal };
        }
 
        [System.Diagnostics.DebuggerHidden]
        private void OnProcessRequestsCompleted(object state)
        {
            var e = ((InvokeAsyncCompletedEventArgs)(state));
            ((Action&lt;ProcessRequestsAsyncCompletedArgs&gt;)(e.UserState)).Invoke(new ProcessRequestsAsyncCompletedArgs(e.Results, e.Error, e.Cancelled, e.UserState));
        }
 
        public void ProcessRequestsAsync(Request[] requests, Action&lt;ProcessRequestsAsyncCompletedArgs&gt; processCompleted)
        {
            InvokeAsync(OnBeginProcessRequests, new object[] { requests },
                        OnEndProcessRequests, OnProcessRequestsCompleted, processCompleted);
        }
 
        private IAsyncResult OnBeginOpen(object[] inValues, AsyncCallback callback, object asyncState)
        {
            return ((ICommunicationObject)(this)).BeginOpen(callback, asyncState);
        }
 
        private object[] OnEndOpen(IAsyncResult result)
        {
            ((ICommunicationObject)(this)).EndOpen(result);
            return null;
        }
 
        private void OnOpenCompleted(object state)
        {
            if ((OpenCompleted != null))
            {
                var e = ((InvokeAsyncCompletedEventArgs)(state));
                OpenCompleted(this, new AsyncCompletedEventArgs(e.Error, e.Cancelled, e.UserState));
            }
        }
 
        public void OpenAsync()
        {
            OpenAsync(null);
        }
 
        public void OpenAsync(object userState)
        {
            InvokeAsync(OnBeginOpen, null, OnEndOpen, OnOpenCompleted, userState);
        }
 
        private IAsyncResult OnBeginClose(object[] inValues, AsyncCallback callback, object asyncState)
        {
            return ((ICommunicationObject)(this)).BeginClose(callback, asyncState);
        }
 
        private object[] OnEndClose(IAsyncResult result)
        {
            ((ICommunicationObject)(this)).EndClose(result);
            return null;
        }
 
        private void OnCloseCompleted(object state)
        {
            var e = ((InvokeAsyncCompletedEventArgs)(state));
            CloseCompleted(new AsyncCompletedEventArgs(e.Error, e.Cancelled, e.UserState));
        }
 
        public void CloseCompleted(AsyncCompletedEventArgs args)
        {
            if (args.Error != null)
            {
                Abort();
            }
        }
 
        public void CloseAsync()
        {
            CloseAsync(null);
        }
 
        public void CloseAsync(object userState)
        {
            InvokeAsync(OnBeginClose, null, OnEndClose, OnCloseCompleted, userState);
        }
 
        protected override IWcfRequestProcessor CreateChannel()
        {
            return new WcfRequestProcessorClientChannel(this);
        }
 
        private class WcfRequestProcessorClientChannel : ChannelBase&lt;IWcfRequestProcessor&gt;, IWcfRequestProcessor
        {
            public WcfRequestProcessorClientChannel(ClientBase&lt;IWcfRequestProcessor&gt; client) :
                base(client)
            {
            }
 
            public IAsyncResult BeginProcessRequests(Request[] requests, AsyncCallback callback, object asyncState)
            {
                var _args = new object[1];
                _args[0] = requests;
                IAsyncResult _result = BeginInvoke(&quot;ProcessRequests&quot;, _args, callback, asyncState);
                return _result;
            }
 
            [System.Diagnostics.DebuggerHidden]
            public Response[] EndProcessRequests(IAsyncResult result)
            {
                var _args = new object[0];
                var _result = ((Response[])(EndInvoke(&quot;ProcessRequests&quot;, _args, result)));
 
                result.AsyncWaitHandle.Close();
 
                return _result;
            }
 
            public void ProcessRequestsAsync(Request[] requests, Action&lt;ProcessRequestsAsyncCompletedArgs&gt; processCompleted)
            {
                throw new NotImplementedException();
            }
        }
 
        public void Dispose()
        {
            CloseAsync();
        }
    }
[/csharp]
</div>

All you would need to be able to make asynchronous calls to the RRSL right now, is the following client-side WCF configuration:

<div>
[xml]
  &lt;system.serviceModel&gt;
    &lt;bindings&gt;
      &lt;basicHttpBinding&gt;
        &lt;binding name=&quot;BasicHttpBinding_IWcfRequestProcessor_HTTP&quot; maxBufferSize=&quot;2147483647&quot;
          maxReceivedMessageSize=&quot;2147483647&quot;&gt;
          &lt;security mode=&quot;None&quot;/&gt;
        &lt;/binding&gt;
      &lt;/basicHttpBinding&gt;
    &lt;/bindings&gt;
    &lt;client&gt;
      &lt;endpoint binding=&quot;basicHttpBinding&quot; bindingConfiguration=&quot;BasicHttpBinding_IWcfRequestProcessor_HTTP&quot;
        contract=&quot;Namespace.Of.Your.IWcfRequestProcessor&quot; name=&quot;BasicHttpBinding_IWcfRequestProcessor_HTTP&quot; /&gt;
    &lt;/client&gt;
  &lt;/system.serviceModel&gt;
[/xml]
</div>

Note, this is an example taken from a Silverlight client... in case of a regular .NET client the WCF configuration will be slightly bigger (and pretty much similar to the one in the example of synchronous RRSL usage).

That's all you really need to be able to use the RRSL asynchronously from a client.  Of course, using the AsyncWcfRequestProcessorProxy class would be even more clumsy and error prone than using the WcfRequestProcessorProxy (from the synchronous usage post) class directly.  Ideally, we should be able to use something similar to the IRequestDispatcher, only asynchronously.  And thus, the IAsyncRequestDispatcher interface was born:

<div>
[csharp]
    public interface IAsyncRequestDispatcher : IDisposable
    {
        void Add(Request request);
        void Add(params Request[] requestsToAdd);
        void Add(string key, Request request);
        void ProcessRequests(Action&lt;ReceivedResponses&gt; receivedResponsesDelegate, Action&lt;ExceptionInfo&gt; exceptionOccurredDelegate);
        void ProcessRequests(Action&lt;ReceivedResponses&gt; receivedResponsesDelegate, Action&lt;ExceptionInfo, ExceptionType&gt; exceptionAndTypeOccurredDelegate);
    }
[/csharp]
</div>

Its usage is pretty similar to that of the IRequestDispatcher, except that you don't access the received responses directly.  Instead, you tell the IAsyncRequestDispatcher to process the requests and you can provide some callbacks.  The first callback needs to be a method which accepts a ReceivedResponses instance as a parameter (we'll get to that class later on in the post).  The second callback is a method which either receives an ExceptionInfo object as a parameter, or both an ExceptionInfo and ExceptionType parameter.  The last callback will obviously only be called if something went wrong.  

Another big difference between the IAsyncRequestDispatcher and the IRequestDispatcher is that the IAsyncRequestDispatcher is not ment to be reused for multiple service calls.  That is, you can obviously add as many requests as you like but you can only call the ProcessRequests method once, at which point all of the added requests will be sent to the RRSL through the AsyncWcfRequestProcessorProxy class.  The reason why we chose to go the "You can only use it once"-route is to guarantee that the IAsyncRequestDispatcher and especially its AsyncWcfRequestProcessorProxy instance are always guaranteed to be disposed properly no matter when the responses are returned, which might be after the view-component has already been closed by the user, for instance.

Now, our implementation of the IAsyncRequestDispatcher interface is dependent upon 3 other classes that we wrote.  The first is the AsyncWcfRequestProcessoryProxy class which we already covered.  The other two are the ReceivedResponses class, and the ResponseReceiver class.  I'll show the implementations of those classes after i show the code of the AsyncRequestDispatcher so you might have to scroll back and forth between the code of these classes in order to grasp the code.  

First of all, the AsyncRequestDispatcher:

<div>
[csharp]
    public class AsyncRequestDispatcher : Disposable, IAsyncRequestDispatcher
    {
        private readonly IWcfRequestProcessor requestProcessor;
        protected Dictionary&lt;string, int&gt; keyToResultPositions;
        private Dictionary&lt;string, Type&gt; keyToTypes;
 
        private List&lt;Request&gt; queuedRequests;
 
        public AsyncRequestDispatcher(IWcfRequestProcessor requestProcessor)
        {
            this.requestProcessor = requestProcessor;
            InitializeState();
        }
 
        public virtual Request[] QueuedRequests
        {
            get { return queuedRequests.ToArray(); }
        }
 
        public virtual void Add(params Request[] requestsToAdd)
        {
            foreach (var request in requestsToAdd)
            {
                Add(request);
            }
        }
 
        public virtual void Add(string key, Request request)
        {
            AddRequest(request, true);
            keyToTypes[key] = request.GetType();
            keyToResultPositions[key] = queuedRequests.Count - 1;
        }
 
        public virtual void Add(Request request)
        {
            AddRequest(request, false);
        }
 
        public virtual void ProcessRequests(Action&lt;ReceivedResponses&gt; receivedResponsesDelegate, Action&lt;ExceptionInfo&gt; exceptionOccurredDelegate)
        {
            ProcessRequests(new ResponseReceiver(receivedResponsesDelegate, exceptionOccurredDelegate, keyToResultPositions));
        }
 
        public virtual void ProcessRequests(Action&lt;ReceivedResponses&gt; receivedResponsesDelegate, Action&lt;ExceptionInfo, ExceptionType&gt; exceptionAndTypeOccurredDelegate)
        {
            ProcessRequests(new ResponseReceiver(receivedResponsesDelegate, exceptionAndTypeOccurredDelegate, keyToResultPositions));
        }
 
        private void ProcessRequests(ResponseReceiver responseReciever)
        {
            var requests = queuedRequests.ToArray();
            BeforeSendingRequests(requests);
            requestProcessor.ProcessRequestsAsync(requests, a =&gt; OnProcessRequestsCompleted(a, responseReciever));
        }
 
        protected virtual void BeforeSendingRequests(IEnumerable&lt;Request&gt; requestsToProcess) { }
 
        public virtual void OnProcessRequestsCompleted(ProcessRequestsAsyncCompletedArgs args, ResponseReceiver responseReciever)
        {
            Dispose();
            responseReciever.ReceiveResponses(args);
        }
 
        protected override void DisposeManagedResources()
        {
            if (requestProcessor != null) requestProcessor.Dispose();
        }
 
        private void AddRequest(Request request, bool wasAddedWithKey)
        {
            Type requestType = request.GetType();
 
            if (RequestTypeIsAlreadyPresent(requestType) &amp;&amp;
                (RequestTypeIsNotAssociatedWithKey(requestType) || !wasAddedWithKey))
            {
                throw new InvalidOperationException(String.Format(&quot;A request of type {0} has already been added. &quot;
                                                                  + &quot;Please add requests of the same type with a different key.&quot;, requestType.FullName));
            }
 
            queuedRequests.Add(request);
        }
 
        private bool RequestTypeIsAlreadyPresent(Type requestType)
        {
            return QueuedRequests.Count(r =&gt; r.GetType().Equals(requestType)) &gt; 0;
        }
 
        private bool RequestTypeIsNotAssociatedWithKey(Type requestType)
        {
            return !keyToTypes.Values.Contains(requestType);
        }
 
        private void InitializeState()
        {
            queuedRequests = new List&lt;Request&gt;();
            keyToTypes = new Dictionary&lt;string, Type&gt;();
            keyToResultPositions = new Dictionary&lt;string, int&gt;();
        }
    }
[/csharp]
</div>

There's nothing complex or difficult about this class.  You can basically add requests just as you could do with the synchronous RequestDispatcher, and when you call the ProcessRequests method, we create a ResponseReceiver which will also be passed into the method that will be called once the responses have returned from the asynchronous proxy.  When those responses are returned, we dispose our own instance of the AsyncRequestDispatcher (which in turn disposes the AsyncWcfRequestProcessorProxy) and then we ask the ResponseReceiver to handle the received responses.  Nothing complicated, but you might have to take a second look if you didn't get it the first time (and you certainly wouldn't be the first).

The implementation of the ResponseReceiver class looks like this:

<div>
[csharp]
    public class ResponseReceiver
    {
        private readonly Action&lt;ReceivedResponses&gt; responseReceivedCallback;
        private readonly Action&lt;ExceptionInfo, ExceptionType&gt; exceptionAndTypeOccuredCallback;
        private readonly Action&lt;ExceptionInfo&gt; exceptionOccurredCallback;
        private readonly Dictionary&lt;string, int&gt; keyToResultPositions;
 
        public ResponseReceiver(Action&lt;ReceivedResponses&gt; responseReceivedCallback, Action&lt;ExceptionInfo&gt; exceptionOccurredCallback,
            Dictionary&lt;string, int&gt; keyToResultPositions)
        {
            if (responseReceivedCallback == null) throw new ArgumentNullException(&quot;responseReceivedCallback&quot;);
            if (exceptionOccurredCallback == null) throw new ArgumentNullException(&quot;exceptionOccurredCallback&quot;);
 
            this.responseReceivedCallback = responseReceivedCallback;
            this.exceptionOccurredCallback = exceptionOccurredCallback;
            this.keyToResultPositions = keyToResultPositions;
        }
 
        public ResponseReceiver(Action&lt;ReceivedResponses&gt; responseReceivedCallback, Action&lt;ExceptionInfo, ExceptionType&gt; exceptionAndTypeOccuredCallback,
            Dictionary&lt;string, int&gt; keyToResultPositions)
        {
            if (responseReceivedCallback == null) throw new ArgumentNullException(&quot;responseReceivedCallback&quot;);
            if (exceptionAndTypeOccuredCallback == null) throw new ArgumentNullException(&quot;exceptionAndTypeOccuredCallback&quot;);
 
            this.responseReceivedCallback = responseReceivedCallback;
            this.exceptionAndTypeOccuredCallback = exceptionAndTypeOccuredCallback;
            this.keyToResultPositions = keyToResultPositions;
        }
 
        public void ReceiveResponses(ProcessRequestsAsyncCompletedArgs args)
        {
            if (HasException(args))
            {
                HandleException(args);
            }
            else
            {
                var disposable = responseReceivedCallback.Target as Disposable;
 
                if (disposable == null || !disposable.IsDisposed)
                {
                    responseReceivedCallback(new ReceivedResponses(args.Result, keyToResultPositions));
                }
            }
        }
 
        private void HandleException(ProcessRequestsAsyncCompletedArgs args)
        {
            var disposable = responseReceivedCallback.Target as Disposable;
 
            if (disposable == null || !disposable.IsDisposed)
            {
                var exception = GetException(args);
 
                if (exceptionOccurredCallback != null)
                {
                    exceptionOccurredCallback(exception);
                }
                else if (exceptionAndTypeOccuredCallback != null)
                {
                    var exceptionType = GetExceptionType(args);
 
                    exceptionAndTypeOccuredCallback(exception, exceptionType);
                }
                else
                {
                    responseReceivedCallback(new ReceivedResponses(args.Result, keyToResultPositions));
                }
            }
        }
 
        private static bool HasException(ProcessRequestsAsyncCompletedArgs args)
        {
            if (args.Error == null)
            {
                return args.Result.Any(r =&gt; r.Exception != null);
            }
 
            return true;
        }
 
        private static ExceptionInfo GetException(ProcessRequestsAsyncCompletedArgs args)
        {
            if (args.Error == null)
            {
                var responseWithException = GetFirstException(args.Result);
                if (responseWithException != null)
                {
                    return responseWithException.Exception;
                }
 
                return null;
            }
 
            return new ExceptionInfo(args.Error);
        }
 
        private static ExceptionType GetExceptionType(ProcessRequestsAsyncCompletedArgs args)
        {
            if (args.Error == null)
            {
                var responseWithException = GetFirstException(args.Result);
 
                if (responseWithException != null)
                {
                    return responseWithException.ExceptionType;
                }
            }
 
            return ExceptionType.Unknown;
        }
 
        private static Response GetFirstException(IEnumerable&lt;Response&gt; responsesToCheck)
        {
            return responsesToCheck.FirstOrDefault(r =&gt; r.Exception != null);
        }
    }
[/csharp]
</div>

Pretty straightforward... it basically makes sure that either the callback from the original caller is called to handle the received responses, or that the callback is called to deal with exceptions.  The callback to handle the received responses receives a ReceivedResponses instance, which again makes it possible to easily retrieve the response you need:

<div>
[csharp]
    public class ReceivedResponses
    {
        private readonly Response[] responses;
        private readonly Dictionary&lt;string, int&gt; keyToResultPositions;
 
        public ReceivedResponses(Response[] responses)
            : this(responses, new Dictionary&lt;string, int&gt;()) {}
 
        public ReceivedResponses(Response[] responses, Dictionary&lt;string, int&gt; keyToResultPositions)
        {
            this.responses = responses;
            this.keyToResultPositions = keyToResultPositions;
        }
 
        public virtual TResponse Get&lt;TResponse&gt;() where TResponse : Response
        {
            var responseType = typeof(TResponse);
            return (TResponse)responses.Single(r =&gt; r.GetType().Equals(responseType));
        }
 
        public virtual TResponse Get&lt;TResponse&gt;(string key) where TResponse : Response
        {
            return (TResponse)responses[keyToResultPositions[key]];
        }
 
        public virtual bool HasResponse&lt;TResponse&gt;() where TResponse : Response
        {
            return responses.OfType&lt;TResponse&gt;().Any();
        }
    }
[/csharp]
</div>

Now, some of you will probably be thinking "isn't all this more complex than it needs to be?".  Apart from the asynchronous proxy, i truly doubt it.  The only thing that your code needs to know of is the API of the IAsyncRequestDispatcher interface and of the ReceivedResponses class which are both pretty clean, very easy to use and easy to grasp. 

One final word about the fact that the IAsyncRequestDispatcher is only meant to be used once.  Obviously, we don't create each IAsyncRequestDispatcher instance manually.  We can't have the IOC container inject it whenever we want either, because then we'd only have one instance for the lifetime of the class that had the IAsyncRequestDispatcher injected.  We inject the following factory instead:

<div>
[csharp]
    public interface IAsyncRequestDispatcherFactory
    {
        IAsyncRequestDispatcher CreateAsyncRequestDispatcher();
    }
[/csharp]
</div>

The implementation of which looks like this:

<div>
[csharp]
    public class AsyncRequestDispatcherFactory : IAsyncRequestDispatcherFactory
    {
        public IAsyncRequestDispatcher CreateAsyncRequestDispatcher()
        {
            return IoC.Container.Resolve&lt;IAsyncRequestDispatcher&gt;();
        }
    }
[/csharp]
</div>

One thing that you need to be very careful of: if your IAsyncRequestDispatcherFactory implementation happens to use Castle Windsor's container (ours doesn't because we have our own custom little container for our Silverlight apps) then you absolutely have to make sure that the Dispose method of the IAsyncRequestDispatcher implementation calls the Release method of the Windsor container.  More information on why you'd need to do that can be found <a href="http://davybrion.com/blog/2008/12/the-importance-of-releasing-your-components-through-windsor/">here</a> and <a href="http://davybrion.com/blog/2008/12/the-component-burden/">here</a>.

That's it for this post, which is probably the most difficult one to comprehend but again, the most important facts to remember are the ease of use of IAsyncRequestDispatcher and ReceivedResponses.  Also, keep in mind that even though we use this primarily for Silverlight clients, you can just as well do this from WPF applications or any other .NET application for that matter.

Finally, i'd like to thank Tom Ceulemans for the implementation shown in this post.  He happened to be the first one who needed to use the RRSL from a silverlight application and he did a great job with getting it to work :)