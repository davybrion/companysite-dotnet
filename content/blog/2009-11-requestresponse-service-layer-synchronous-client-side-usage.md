Note: This post is part of a series. Be sure to read the introduction <a href="/blog/2009/11/requestresponse-service-layer-series/">here</a>.

One of my biggest issues with using typical WCF Services is that you always need to update your client-side proxies whenever a new operation is added to your service layer.  I definitely wanted to avoid that with the Request/Response Service Layer, and that was easy to do because this is our only Service Contract:

<script src="https://gist.github.com/3685524.js?file=s1.cs"></script>

As you can see there's only one Operation on this service, and because each 'operation' of our service layer is exposed as a Request/Response combination, we'll never have to add anything else to this Service Contract and thus, we only need to create our proxy once and we'll never have to update it.

Instead of generating a service proxy with SvcUtil, i chose to just inherit from WCF's ClientBase class:

<script src="https://gist.github.com/3685524.js?file=s2.cs"></script>

And that is it as far as the proxy is concerned.  The implementation of the Dispose method isn't very clean, but that is to work around an issue that WCF has where calling the Close method can throw an Exception (and as you know, your Dispose methods should <strong>never, ever</strong> throw an Exception) and when it does, you absolutely need to call the Abort method of the proxy.  If you don't, the Channel will remain open and once you hit the limit of concurrent open channels on your service, no requests will be able to be handled by the service.  Why yes, we definitely learned that one the hard way ;)

Now, this proxy is already enough to be able to make synchronous calls to the RRSL from your client code, as long as you put the following WCF configuration in your web.config or app.config file:

<script src="https://gist.github.com/3685524.js?file=s3.xml"></script>

In this example we're connecting to a RRSL that is exposed through the wsHttpBinding over HTTPS.  Obviously, you can use any of the other WCF bindings and configurations as well.  

Also, you'll need to register your Request and Response types with the KnownTypeProvider (just as you had to do in your service host) before you start sending requests to the service.  You can do that with something like this:

<script src="https://gist.github.com/3685524.js?file=s4.cs"></script>

So, that is all you need to use the RRSL synchronously.  But using the Process method of the RequestProcessorProxy is clumsy and even prone to errors because you'd have to keep track of the indexes of the requests and the responses.  It certainly isn't a nice API.  So to fix that, we also have the IRequestDispatcher interface which offers a much cleaner and less error prone API:

<script src="https://gist.github.com/3685524.js?file=s5.cs"></script>

The idea is that you can add requests to an IRequestDispatcher in a clean manner, and then simply retrieve the responses you want based on the type of the response.  If you need to send multiple requests of the same type in a single batch, you need to provide a string key to identify its response because another response will be of the same type.  You can add as many requests as you want, and the requests won't be sent to the service until you actually try to retrieve one of the responses.  Once you try to retrieve a response, all of the batched requests will be sent to the service in a single roundtrip and the responses are stored in the IRequestDispatcher until you retrieve them or until the IRequestDispatcher is cleared (or garbage collected).  

The implementation of the IRequestDispatcher will use an IRequestProcessor instance to send the requests to the Request Processor (covered in an earlier post in this series).  If you're using the RRSL over WCF, then you better configure your IOC container to create instances of the RequestProcessorProxy class whenever an instance of IRequestProcessor is requested by your code.  This also means that you can just as well run the RRSL within the same process as your client code if you don't actually need it to run in a separate process (or service) and just want the architectural benefits of this approach.  You could do this by simply configuring your IOC container to return the actual RequestProcessor implementation instead of the WCF proxy. 

The implementation of the RequestDispatcher class looks like this:

<script src="https://gist.github.com/3685524.js?file=s6.cs"></script>

There's nothing special about this class, except for 3 things maybe.  First of all, it's an abstract class with 2 abstract methods.  So obviously, your application needs to inherit from this RequestDispatcher implementation (or can provide its own implementation as long as it implements the IRequestDispatcher interface) and implement the abstract DealWithSecurityException and DealWithUnknownException methods.  We found this to be useful because most web applications will return you to a general "Oops, something went wrong" page or "You don't have enough clout in this world to perform the task you were trying to do" page when unknown exceptions or security exceptions occur.   If you don't want any of that to happen and simply deal with the exception in the calling code, the implementations of these methods can just do nothing, or only do something in certain conditions or whatever else that fits your purpose or intentions.

There's also a virtual BeforeSendingRequests methods which you can implement.  We typically override that method to add authentication data (such as the current user's username and password hash) to each request.

That's pretty much all there is to using the RRSL from your client code in a synchronous manner with a pretty clean API.