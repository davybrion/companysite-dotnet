Note: This post is part of a series. Be sure to read the introduction <a href="http://davybrion.com/blog/2009/11/requestresponse-service-layer-series/">here</a>.

Writing automated tests for asynchronous operations in general can be pretty cumbersome.  In the case of the Request/Response Service Layer (RRSL), we basically need to be able to verify that our client code is sending the correct requests, and see how it deals with prepared responses that we send back to the client code through the provided callback.  This actually makes it very easy to test the asynchronous usage of the RRSL.  We basically just need a different implementation of the IAsyncRequestDispatcher interface, which stores the added requests so we can inspect them later on, and which simply holds a reference to the ResponseReceiver and gives us a specific way to trigger the execution of the ResponseReceiver's logic to call the correct callback from the client code.  I'll show the AsyncRequestDispatcherStub class later on, but first we'll take a look at its interface to see which extra methods it provides:

<div>
[csharp]
    public interface IAsyncRequestDispatcherStub : IAsyncRequestDispatcher
    {
        void SetResponsesToReturn(params Response[] responses);
        void AddResponseToReturn(Response response, string key);
        bool HasRequest&lt;TRequest&gt;() where TRequest : Request;
        bool HasRequest&lt;TRequest&gt;(string key) where TRequest : Request;
        TRequest GetRequest&lt;TRequest&gt;() where TRequest : Request;
        TRequest GetRequest&lt;TRequest&gt;(string key) where TRequest : Request;
        void ClearRequests();
        void ReturnResponses();
    }
[/csharp]
</div>

Note that this interface doesn't really exist... it's just shown here to give you a clear view on what specific testing-related functionality the AsyncRequestDispatcherStub offers on top of the regular AsyncRequestDispatcher.

As you can see, we have two methods to add some prepared responses which will be returned to the client code once we call the ReturnResponses method in our test.  We also have some methods to inspect the requests that were added by the client code.

And here's the actual code of the AsyncRequestDispatcherStub class:

<div>
[csharp]
    public class AsyncRequestDispatcherStub : Disposable, IAsyncRequestDispatcher
    {
        private readonly Dictionary&lt;Type, string&gt; unkeyedTypesToAutoKey;
        private readonly Dictionary&lt;string, Request&gt; requests;
        private readonly Dictionary&lt;string, int&gt; responseKeyToIndexPosition;
        private readonly List&lt;Response&gt; responsesToReturn;
        private ResponseReceiver responseReceiver;
 
        public AsyncRequestDispatcherStub()
        {
            unkeyedTypesToAutoKey = new Dictionary&lt;Type, string&gt;();
            requests = new Dictionary&lt;string, Request&gt;();
            responseKeyToIndexPosition = new Dictionary&lt;string, int&gt;();
            responsesToReturn = new List&lt;Response&gt;();
        }
 
        public void SetResponsesToReturn(params Response[] responses)
        {
            responsesToReturn.Clear();
            responsesToReturn.AddRange(responses);
        }
 
        public void AddResponseToReturn(Response response, string key)
        {
            responsesToReturn.Add(response);
            responseKeyToIndexPosition.Add(key, responsesToReturn.Count - 1);
        }
 
        public bool HasRequest&lt;TRequest&gt;() where TRequest : Request
        {
            return unkeyedTypesToAutoKey.ContainsKey(typeof(TRequest));
        }
 
        public bool HasRequest&lt;TRequest&gt;(string key) where TRequest : Request
        {
            return requests.ContainsKey(key) &amp;&amp; (requests[key] is TRequest);
        }
 
        public TRequest GetRequest&lt;TRequest&gt;() where TRequest : Request
        {
            var autoKey = unkeyedTypesToAutoKey[typeof(TRequest)];
            return (TRequest)requests[autoKey];
        }
 
        public TRequest GetRequest&lt;TRequest&gt;(string key) where TRequest : Request
        {
            return (TRequest)requests[key];
        }
 
        public void ClearRequests()
        {
            unkeyedTypesToAutoKey.Clear();
            requests.Clear();
        }
 
        public void Add(Request request)
        {
            var autoKey = Guid.NewGuid().ToString();
            unkeyedTypesToAutoKey.Add(request.GetType(), autoKey);
            requests.Add(autoKey, request);
        }
 
        public void Add(params Request[] requestsToAdd)
        {
            if (requestsToAdd != null)
            {
                foreach (var request in requestsToAdd)
                {
                    Add(request);
                }
            }
        }
 
        public void Add(string key, Request request)
        {
            requests.Add(key, request);
        }
 
        public void ProcessRequests(Action&lt;ReceivedResponses&gt; receivedResponsesDelegate, Action&lt;ExceptionInfo&gt; exceptionOccurredDelegate)
        {
            ProcessRequests(new ResponseReceiver(receivedResponsesDelegate, exceptionOccurredDelegate, responseKeyToIndexPosition));
        }
 
        public void ProcessRequests(Action&lt;ReceivedResponses&gt; receivedResponsesDelegate, Action&lt;ExceptionInfo, ExceptionType&gt; exceptionAndTypeOccurredDelegate)
        {
            ProcessRequests(new ResponseReceiver(receivedResponsesDelegate, exceptionAndTypeOccurredDelegate, responseKeyToIndexPosition));
        }
 
        private void ProcessRequests(ResponseReceiver responseReceiver)
        {
            this.responseReceiver = responseReceiver;
        }
 
        public void ReturnResponses()
        {
            responseReceiver.ReceiveResponses(new ProcessRequestsAsyncCompletedArgs(new[] { responsesToReturn.ToArray() }, null, false, null));
        }
 
        public void Clear()
        {
            // has to be an empty implementation to be able to inspect the added requests
        }
 
        protected override void DisposeManagedResources()
        {
        }
    }
[/csharp]
</div>

All of this is (once again) very straightforward and we can now very easily verify that our client code is using the RRSL correctly.

Since our client code always receives an IAsyncRequestDispatcher instance through an IAsyncRequestDispatcherFactory, we'll need a different implementation of that factory to be used during our tests:

<div>
[csharp]
    public class AsyncRequestDispatcherFactoryStub : IAsyncRequestDispatcherFactory
    {
        private readonly AsyncRequestDispatcherStub asyncRequestDispatcherStub;
 
        public AsyncRequestDispatcherFactoryStub(AsyncRequestDispatcherStub asyncRequestDispatcherStub)
        {
            this.asyncRequestDispatcherStub = asyncRequestDispatcherStub;
        }
 
        public IAsyncRequestDispatcher CreateAsyncRequestDispatcher()
        {
            return asyncRequestDispatcherStub;
        }
    }
[/csharp]
</div>

And that's all folks.