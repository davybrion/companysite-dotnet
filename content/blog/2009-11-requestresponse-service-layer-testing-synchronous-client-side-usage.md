Note: This post is part of a series. Be sure to read the introduction <a href="/blog/2009/11/requestresponse-service-layer-series/">here</a>.

If you want to write automated tests for your client-side code, it's often useful to replace the service proxy with a mock instance.  With typical WCF services and their proxies, that's pretty easy to do.  With the Request/Response Service Layer (RRSL) and its IRequestDispatcher, it's a bit more tricky.  While you could provide a mock instance of IRequestDispatcher to your classes under test, we've learned that it's easier to use a prepared stub class which inherits from the RequestDispatcher class and adds some extra methods to inspect the requests that were supposed to be sent, and to return response objects that you can easily prepare yourself.

If we go back to our implementation of the IRequestDispatcher interface, you'll notice that the RequestDispatcher class has the following constructor:

<script src="https://gist.github.com/3685540.js?file=s1.cs"></script>

As well as the following virtual method which is the only place where we actually use the IRequestProcessor to send requests to the Request Processor:

<script src="https://gist.github.com/3685540.js?file=s2.cs"></script>

You'll also notice that most of the public methods of the RequestDispatcher class are virtual and that many of its protected methods are virtual as well.  While we don't need to override all of them, there's plenty of flexibility to do whatever you want to do. We'll basically pass a null reference to the RequestDispatcher's constructor (which takes an IRequestProcessor instance) and override the protected GetResponses method to simply return our prepared responses instead of actually sending them to the IRequestProcessor.  We'll also add a few methods so you can add prepared responses in your tests, as well as some methods which allow you to easily inspect whether certain requests were added by the code under test, and to retrieve the actual requests so you can verify that they contain the expected data.

There is no interface to define the added functionality of the stub, but this fictional interface might make it clearer what you'll be able to do with the stub in your tests:

<script src="https://gist.github.com/3685540.js?file=s3.cs"></script>

Those added methods make it very clear to verify that your code under test is communicating with the RRSL in the way you intended it to.

So finally, this is the code of the RequestDispatcherStub class:

<script src="https://gist.github.com/3685540.js?file=s4.cs"></script>

With this in place, you can very easily test whether the correct requests have been sent, whether they contain the expected data, and how your code reacts to the data in your prepared responses.