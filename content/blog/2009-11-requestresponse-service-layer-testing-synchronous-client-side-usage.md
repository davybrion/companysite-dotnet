Note: This post is part of a series. Be sure to read the introduction <a href="http://davybrion.com/blog/2009/11/requestresponse-service-layer-series/">here</a>.

If you want to write automated tests for your client-side code, it's often useful to replace the service proxy with a mock instance.  With typical WCF services and their proxies, that's pretty easy to do.  With the Request/Response Service Layer (RRSL) and its IRequestDispatcher, it's a bit more tricky.  While you could provide a mock instance of IRequestDispatcher to your classes under test, we've learned that it's easier to use a prepared stub class which inherits from the RequestDispatcher class and adds some extra methods to inspect the requests that were supposed to be sent, and to return response objects that you can easily prepare yourself.

If we go back to our implementation of the IRequestDispatcher interface, you'll notice that the RequestDispatcher class has the following constructor:

<div>
[csharp]
        protected RequestDispatcher(IRequestProcessor requestProcessor)
        {
            this.requestProcessor = requestProcessor;
            InitializeState();
        }
[/csharp]
</div>

As well as the following virtual method which is the only place where we actually use the IRequestProcessor to send requests to the Request Processor:

<div>
[csharp]
        protected virtual Response[] GetResponses(params Request[] requestsToProcess)
        {
            BeforeSendingRequests(requestsToProcess);
            return requestProcessor.Process(requestsToProcess);
        }
[/csharp]
</div>

You'll also notice that most of the public methods of the RequestDispatcher class are virtual and that many of its protected methods are virtual as well.  While we don't need to override all of them, there's plenty of flexibility to do whatever you want to do. We'll basically pass a null reference to the RequestDispatcher's constructor (which takes an IRequestProcessor instance) and override the protected GetResponses method to simply return our prepared responses instead of actually sending them to the IRequestProcessor.  We'll also add a few methods so you can add prepared responses in your tests, as well as some methods which allow you to easily inspect whether certain requests were added by the code under test, and to retrieve the actual requests so you can verify that they contain the expected data.

There is no interface to define the added functionality of the stub, but this fictional interface might make it clearer what you'll be able to do with the stub in your tests:

<div>
[csharp]
    public interface IRequestDispatcherStub : IRequestDispatcher
    {
        void AddResponsesToReturn(params Response[] responses);
        void AddResponsesToReturn(Dictionary&lt;string, Response&gt; keyedResponses);
        void AddResponseToReturn(Response response);
        void AddResponseToReturn(string key, Response response);
        TRequest GetRequest&lt;TRequest&gt;() where TRequest : Request;
        TRequest GetRequest&lt;TRequest&gt;(string key) where TRequest : Request;
        bool HasRequest&lt;TRequest&gt;() where TRequest : Request;
    }
[/csharp]
</div>

Those added methods make it very clear to verify that your code under test is communicating with the RRSL in the way you intended it to.

So finally, this is the code of the RequestDispatcherStub class:

<div>
[csharp]
    public class RequestDispatcherStub : RequestDispatcher
    {
        private readonly List&lt;Response&gt; responsesToReturn = new List&lt;Response&gt;();
        private readonly Dictionary&lt;string, Request&gt; keyToRequest = new Dictionary&lt;string, Request&gt;();
 
        public RequestDispatcherStub() : base(null) { }
 
        public void AddResponsesToReturn(params Response[] responses)
        {
            responsesToReturn.AddRange(responses);
        }
 
        public void AddResponsesToReturn(Dictionary&lt;string, Response&gt; keyedResponses)
        {
            responsesToReturn.AddRange(keyedResponses.Values);
 
            for (int i = 0; i &lt; keyedResponses.Keys.Count; i++)
            {
                var key = keyedResponses.Keys.ElementAt(i);
 
                if (key != null)
                {
                    keyToResultPositions.Add(key, i);
                }
            }
        }
 
        public void AddResponseToReturn(Response response)
        {
            responsesToReturn.Add(response);
        }
 
        public void AddResponseToReturn(string key, Response response)
        {
            responsesToReturn.Add(response);
            keyToResultPositions.Add(key, responsesToReturn.Count - 1);
        }
 
        public override void Clear()
        {
            // this Stub can't clear the state because we have to be able to inspect the sent requests
            // during our tests
        }
 
        public override void Add(string key, Request request)
        {
            base.Add(key, request);
            keyToRequest[key] = request;
        }
 
        public TRequest GetRequest&lt;TRequest&gt;() where TRequest : Request
        {
            return (TRequest)SentRequests.First(r =&gt; r.GetType().Equals(typeof(TRequest)));
        }
 
        public TRequest GetRequest&lt;TRequest&gt;(string key) where TRequest : Request
        {
            return (TRequest)keyToRequest[key];
        }
 
        public bool HasRequest&lt;TRequest&gt;() where TRequest : Request
        {
            return SentRequests.Count(r =&gt; r.GetType().Equals(typeof(TRequest))) &gt; 0;
        }
 
        protected override Response[] GetResponses(params Request[] requestsToProcess)
        {
            return responsesToReturn.ToArray();
        }
 
        protected override void DealWithSecurityException(ExceptionInfo exceptionInfo)
        {
            throw new SecurityException(&quot;a security exception was thrown: &quot; + exceptionInfo);
        }
 
        protected override void DealWithUnknownException(ExceptionInfo exceptionInfo)
        {
            throw new Exception(&quot;an unknown exception was thrown: &quot; + exceptionInfo);
        }
    }
[/csharp]
</div>

With this in place, you can very easily test whether the correct requests have been sent, whether they contain the expected data, and how your code reacts to the data in your prepared responses.