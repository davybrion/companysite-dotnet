<a href="http://danthar.tweakblogs.net/" target="_blank">Arjen Smits</a> recently submitted a patch for <a href="http://code.google.com/p/agatha-rrsl/" target="_blank">Agatha</a> which makes it possible to send One-Way (aka Fire And Forget) requests to an Agatha service layer. As you know, Agatha is a Request/Response framework where each Request sent to the service layer will receive a corresponding Response. A One-Way request doesn’t have a response however. It can be very useful when you just need the service layer to do <em>something</em> where the client that sent the request doesn’t need a response, and thus, shouldn’t have to wait for it.

The biggest benefit here is that client can simply send a One-Way request to the service layer, and instead of waiting for the service layer to process it, the call to the service layer will return <em>immediately. </em>You could achieve similar behavior by using the asynchronous service proxy and supplying an empty delegate to be called after the response was received. The downside of that is obviously that the communication channel between the client and the server would remain in use throughout the handling of the request, only to return a response later on that will be ignored anyway. Sending a One-Way request avoids this kind of waste.

Let’s see how we can use One-Way requests with Agatha. First of all, you will need to create a Request class which inherits from Agatha’s OneWayRequest class:  

<script src="https://gist.github.com/3685652.js?file=s1.cs"></script>

This is very similar to a regular RequestHandler, except that there’s no way for you to return a response. Simply inherit from this class, implement the Handle(TRequest) method to perform whatever it is that you need to do and that’s it as far as the service layer is concerned. If you want to, you can (as always) override the BeforeHandle and AfterHandle methods to add some extra stuff. If you don’t want to inherit from this OneWayRequestHandler class, you can just as well create a class which implements Agatha’s IOneWayRequestHandler&lt;TRequest&gt; interface. Your One Way Request Handlers will be picked up and registered by Agatha automatically, just as it does with regular Request Handlers.

As for sending One Way Requests to your service layer, you can do that with both the synchronous IRequestDispatcher and the IAsyncRequestDispatcher. The IRequestDispatcher interface now looks like this:

<script src="https://gist.github.com/3685652.js?file=s2.cs"></script>

Sending OneWayRequests can be done through the Send method, which will immediately send the given OneWayRequests to the service layer and the call will return as soon as the requests have been transmitted. This means that this call will block for a short time until the requests have actually been <em>sent</em>. It will return before the requests are actually processed by the service layer however.

The IAsyncRequestDispatcher now looks like this:

<script src="https://gist.github.com/3685652.js?file=s3.cs"></script>

You can add your OneWayRequest through the correct Add method, and then have them processed by the server by calling the ProcessOneWayRequests method. This call will not block and will return <em>immediately</em>. That means that it won’t wait until the requests have actually been sent to the server like it does with the IRequestDispatcher.