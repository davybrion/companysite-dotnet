Note: This post is part of a series. Be sure to read the introduction <a href="http://davybrion.com/blog/2009/11/requestresponse-service-layer-series/">here</a>.

First of all, i would like to mention that i am by no means an expert on WCF and asynchronous operations, so it is quite possible that some of the things in this post could be done easier by someone who knows more about it.  Most of the code in this post wasn't written by me either, but by my co-worker Tom Ceulemans (who unfortunately doesn't have a blog that i can link to).  What you'll see in this post does work and it actually works very well.  But as i said, there very well might be room for some nice improvements here.  Anyways, let's get to it.

As you know by now, our Service Contract for the Request/Response Service Layer (RRSL) looks like this:

<script src="https://gist.github.com/3685562.js?file=s1.cs"></script>

As you can see, this Service Contract doesn't define any asynchronous operations.  We don't need to define them in the original contract, but we do have to use an asynchronous version of this Service Contract in the client from which we want to make asynchronous calls to our RRSL.  So client-side, we use the following version of the IWcfRequestProcessor Service Contract:

<script src="https://gist.github.com/3685562.js?file=s2.cs"></script>

This one only defines the asynchronous version of the ProcessRequests operation, which in our case is all we need since we use this in our Silverlight applications where you never make synchronous remote calls.

The ProcessRequestsAsyncCompletedArgs class looks like this:

<script src="https://gist.github.com/3685562.js?file=s3.cs"></script>

Now we need a proxy class to implement the asynchronous version of the IWcfRequestProcessor interface.  The code of the AsyncWcfRequestProcessorProxy code is somewhat low-level because it's (obviously) dealing with all of the async stuff, so there's not really a lot of need to get into the details of this piece of code.  If you're implementing your own RRSL, just copy this code and be glad that you didn't have to write it ;)

<script src="https://gist.github.com/3685562.js?file=s4.cs"></script>

All you would need to be able to make asynchronous calls to the RRSL right now, is the following client-side WCF configuration:

<script src="https://gist.github.com/3685562.js?file=s5.xml"></script>

Note, this is an example taken from a Silverlight client... in case of a regular .NET client the WCF configuration will be slightly bigger (and pretty much similar to the one in the example of synchronous RRSL usage).

That's all you really need to be able to use the RRSL asynchronously from a client.  Of course, using the AsyncWcfRequestProcessorProxy class would be even more clumsy and error prone than using the WcfRequestProcessorProxy (from the synchronous usage post) class directly.  Ideally, we should be able to use something similar to the IRequestDispatcher, only asynchronously.  And thus, the IAsyncRequestDispatcher interface was born:

<script src="https://gist.github.com/3685562.js?file=s6.cs"></script>

Its usage is pretty similar to that of the IRequestDispatcher, except that you don't access the received responses directly.  Instead, you tell the IAsyncRequestDispatcher to process the requests and you can provide some callbacks.  The first callback needs to be a method which accepts a ReceivedResponses instance as a parameter (we'll get to that class later on in the post).  The second callback is a method which either receives an ExceptionInfo object as a parameter, or both an ExceptionInfo and ExceptionType parameter.  The last callback will obviously only be called if something went wrong.  

Another big difference between the IAsyncRequestDispatcher and the IRequestDispatcher is that the IAsyncRequestDispatcher is not ment to be reused for multiple service calls.  That is, you can obviously add as many requests as you like but you can only call the ProcessRequests method once, at which point all of the added requests will be sent to the RRSL through the AsyncWcfRequestProcessorProxy class.  The reason why we chose to go the "You can only use it once"-route is to guarantee that the IAsyncRequestDispatcher and especially its AsyncWcfRequestProcessorProxy instance are always guaranteed to be disposed properly no matter when the responses are returned, which might be after the view-component has already been closed by the user, for instance.

Now, our implementation of the IAsyncRequestDispatcher interface is dependent upon 3 other classes that we wrote.  The first is the AsyncWcfRequestProcessoryProxy class which we already covered.  The other two are the ReceivedResponses class, and the ResponseReceiver class.  I'll show the implementations of those classes after i show the code of the AsyncRequestDispatcher so you might have to scroll back and forth between the code of these classes in order to grasp the code.  

First of all, the AsyncRequestDispatcher:

<script src="https://gist.github.com/3685562.js?file=s7.cs"></script>

There's nothing complex or difficult about this class.  You can basically add requests just as you could do with the synchronous RequestDispatcher, and when you call the ProcessRequests method, we create a ResponseReceiver which will also be passed into the method that will be called once the responses have returned from the asynchronous proxy.  When those responses are returned, we dispose our own instance of the AsyncRequestDispatcher (which in turn disposes the AsyncWcfRequestProcessorProxy) and then we ask the ResponseReceiver to handle the received responses.  Nothing complicated, but you might have to take a second look if you didn't get it the first time (and you certainly wouldn't be the first).

The implementation of the ResponseReceiver class looks like this:

<script src="https://gist.github.com/3685562.js?file=s8.cs"></script>

Pretty straightforward... it basically makes sure that either the callback from the original caller is called to handle the received responses, or that the callback is called to deal with exceptions.  The callback to handle the received responses receives a ReceivedResponses instance, which again makes it possible to easily retrieve the response you need:

<script src="https://gist.github.com/3685562.js?file=s9.cs"></script>

Now, some of you will probably be thinking "isn't all this more complex than it needs to be?".  Apart from the asynchronous proxy, i truly doubt it.  The only thing that your code needs to know of is the API of the IAsyncRequestDispatcher interface and of the ReceivedResponses class which are both pretty clean, very easy to use and easy to grasp. 

One final word about the fact that the IAsyncRequestDispatcher is only meant to be used once.  Obviously, we don't create each IAsyncRequestDispatcher instance manually.  We can't have the IOC container inject it whenever we want either, because then we'd only have one instance for the lifetime of the class that had the IAsyncRequestDispatcher injected.  We inject the following factory instead:

<script src="https://gist.github.com/3685577.js?file=s1.cs"></script>

The implementation of which looks like this:

<script src="https://gist.github.com/3685577.js?file=s1.cs"></script>

One thing that you need to be very careful of: if your IAsyncRequestDispatcherFactory implementation happens to use Castle Windsor's container (ours doesn't because we have our own custom little container for our Silverlight apps) then you absolutely have to make sure that the Dispose method of the IAsyncRequestDispatcher implementation calls the Release method of the Windsor container.  More information on why you'd need to do that can be found <a href="http://davybrion.com/blog/2008/12/the-importance-of-releasing-your-components-through-windsor/">here</a> and <a href="http://davybrion.com/blog/2008/12/the-component-burden/">here</a>.

That's it for this post, which is probably the most difficult one to comprehend but again, the most important facts to remember are the ease of use of IAsyncRequestDispatcher and ReceivedResponses.  Also, keep in mind that even though we use this primarily for Silverlight clients, you can just as well do this from WPF applications or any other .NET application for that matter.

Finally, i'd like to thank Tom Ceulemans for the implementation shown in this post.  He happened to be the first one who needed to use the RRSL from a silverlight application and he did a great job with getting it to work :)