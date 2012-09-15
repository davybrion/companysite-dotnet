Note: This post is part of a series. Be sure to read the introduction <a href="/blog/2009/11/requestresponse-service-layer-series/">here</a>.

The Request/Response Service Layer (RRSL) only defines one service operation, or entry point if you will:

<script src="https://gist.github.com/3685427.js?file=s1.cs"></script>

The topic of this post is how we go from that interface, to some kind of mechanism that can actually process the incoming requests and send the appropriate responses back to the client.  Note that i make a difference between <em>processing</em> requests and <em>handling</em> them.  Processing the requests is done by the Request Processor, and each request will be handled by its very own Request Handler.  The Request Processor accepts all incoming requests, delegates the actual handling of each request to the correct Request Handler and will then send back all of the responses to the client.  Handling requests and how Request Handlers work is the topic of a different post, but you do already need to know that each Request Handler will implement the following interface:

<script src="https://gist.github.com/3685427.js?file=s2.cs"></script>

Unfortunately, when trying to work with Generic types in .NET in a generic manner you typically need to resort to a non-generic version to be able to handle your generic usage (read that sentence a few more times if you didn't get it):

<script src="https://gist.github.com/3685427.js?file=s3.cs"></script>

The Request Processor will make use of the non-generic IRequestHandler interface, while each Request Handler will simply implement the generic version. 

We'll begin with a relatively simple implementation of the Request Processor, which is the bare minimum that you'll need to get this working.  We'll then work our way up to include exception handling and logging.

This is the simplest version of the Request Processor:

<script src="https://gist.github.com/3685427.js?file=s4.cs"></script>

First of all, you'll notice that the RequestProcessor has an empty Dispose method.  The only reason it's there is because the IRequestProcessor interface will also be used client-side, and it's <strong>very</strong> important that client-side implementations of the IRequestProcessor interface are disposable.  We'll cover this in detail in a future post of this series though so let's just focus on how the requests are processed.

As you can see, it's pretty simple and straightforward.  For each request that needs to be processed, we ask our IOC container to resolve the actual handler for it, based on the convention that each Request Handler must implement the generic IRequestHandler interface with the type of the request as the generic type argument.  Once we have a reference to the correct handler, we can call the handler's Handle method, but before we do that we call the protected virtual BeforeHandle method of the Request Processor.  Then we call the actual Handle method on the handler, and after that we call the protected virtual AfterHandle method.  Those 2 virtual methods are only there to make it easy to inherit from the Request Processor in your application to add something extra before and/or after the handler is called.  After that, the responses are sent back to the client and that's all there is to it.

Again, this is the simplest implementation.  Its biggest limitation right now is that it doesn't properly deal with failed requests.  In its current form, once a request handler throws an exception, the exception will not be caught by the Request Processor and the call to the Request Processor will simply fail.  And if you're using this with WCF, the client's channel will be faulted as well.  The biggest question is of course: what should happen if one of the requests of the batch fails?  Should the entire batch be considered a failure? Should default responses be returned for failed requests?  If the second request of a batch fails, should we still attempt to handle the ones that follow the failed request in the batch?

Many people will have their own preference on how to deal with this, but it's important to realize that each approach has it pro's and con's and i don't consider any of them to be perfect.  For instance, most of our applications use NHibernate.  Each Request Handler will set up and manage its own Unit Of Work.  If there is batch of 3 requests, and the second request fails, we have no way of rolling back the transaction of the first request because its Unit Of Work has already been completed (and thus, its transaction is already committed).  That is a deliberate choice that we made.  You could also implement your Request Processor in a way that a Unit Of Work is set up, and that each Request Handler shares the same Unit Of Work.  This would give you the ability to roll back the transaction of the first request if the second one failed.  It also means that each request could potentially be impacted by previous requests in the batch, depending on what a previous request left in the Session Cache of NHibernate and the thought of that doesn't really sit well with me either.

I've chosen the following approach to dealing with exceptions, and after using this for over a year, i'm still happy with it:
<ul>
	<li>The Request Processor has no knowledge of a Request Handler's Unit Of Work.</li>
	<li>The Request Processor must always return a response for each request it received.</li>
	<li>If the handling of a request failed, the Request Processor must return a response object with the details of the actual exception.</li>
	<li>If a previous request in a batch failed, the Request Processor must return a default response instance for the subsequent requests indicating that they couldn't be handled because an earlier request in the batch already failed.</li>
</ul>

Our previously simple Process method now looks like this:

<script src="https://gist.github.com/3685427.js?file=s5.cs"></script>

It now uses a boolean to keep track of whether the batch can still be considered successful.  Once a request failed, it will start adding default responses which indicate that a previous request in the batch failed.  We also have to modify the GetResponseFromHandler method so that it can add the actual exception to the response in case of failure:

<script src="https://gist.github.com/3685427.js?file=s6.cs"></script>

I agree that it's not pretty, but it works :)

Alright, we now have a Request Processor that can reroute each request to the appropriate Request Handler and can deal with exceptions in a manner that is sufficient for me.  Again, if you want to use a different approach with regards to dealing with failed requests in a batch, you are obviously free to do whatever you prefer :)

I'd also like to point out that the exception handling that you just saw is the <em>only</em> exception handling that we need to implement in our service layer.  We don't need any repetitive error handling code and we don't need any Aspect Oriented Programming tricks to deal with it all over the place either.  We also don't need to implement a clumsy interface which then has to be registered with WCF ;)

This version of the Request Processor is still not complete though.  We still need proper logging.  Every exception that occurs obviously has to be logged properly.  I also want to log each handled request that took too long to process, or each batch of requests that took too long in its entirety.   And as you can probably guess by now, the logging code will only need to be written once and it will work for the entire service layer :)

So to wrap up this post, here's the final version of the Request Processor that we use for all our projects:

<script src="https://gist.github.com/3685427.js?file=s7.cs"></script>

Yes, the logging and exception handling code looks ugly.  I could clean it up and extract it to more methods, but since it only occurs in this single class i don't really see the point and it might actually reduce readability in this particular case.