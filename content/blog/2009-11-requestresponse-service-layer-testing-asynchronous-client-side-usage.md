Note: This post is part of a series. Be sure to read the introduction <a href="/blog/2009/11/requestresponse-service-layer-series/">here</a>.

Writing automated tests for asynchronous operations in general can be pretty cumbersome.  In the case of the Request/Response Service Layer (RRSL), we basically need to be able to verify that our client code is sending the correct requests, and see how it deals with prepared responses that we send back to the client code through the provided callback.  This actually makes it very easy to test the asynchronous usage of the RRSL.  We basically just need a different implementation of the IAsyncRequestDispatcher interface, which stores the added requests so we can inspect them later on, and which simply holds a reference to the ResponseReceiver and gives us a specific way to trigger the execution of the ResponseReceiver's logic to call the correct callback from the client code.  I'll show the AsyncRequestDispatcherStub class later on, but first we'll take a look at its interface to see which extra methods it provides:

<script src="https://gist.github.com/3685599.js?file=s1.cs"></script>

Note that this interface doesn't really exist... it's just shown here to give you a clear view on what specific testing-related functionality the AsyncRequestDispatcherStub offers on top of the regular AsyncRequestDispatcher.

As you can see, we have two methods to add some prepared responses which will be returned to the client code once we call the ReturnResponses method in our test.  We also have some methods to inspect the requests that were added by the client code.

And here's the actual code of the AsyncRequestDispatcherStub class:

<script src="https://gist.github.com/3685599.js?file=s2.cs"></script>

All of this is (once again) very straightforward and we can now very easily verify that our client code is using the RRSL correctly.

Since our client code always receives an IAsyncRequestDispatcher instance through an IAsyncRequestDispatcherFactory, we'll need a different implementation of that factory to be used during our tests:

<script src="https://gist.github.com/3685599.js?file=s3.cs"></script>

And that's all folks.