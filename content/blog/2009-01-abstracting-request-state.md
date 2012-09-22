Sometimes it's useful to be able to store an object somewhere so you can easily access it for the duration of the current request, instead of having to pass it around with every method call that you make.  That request could be an ASP.NET request, or a request in your WCF service layer.  I used to resort to storing these objects in ThreadStatic fields (which is basically a static reference for each thread), thinking that it would be safe because only one thread handles a complete request.  Last week I read that some requests can <a href="http://piers7.blogspot.com/2005/11/threadstatic-callcontext-and_02.html">be paused and resumed by another thread</a>.  If you're using ThreadStatic fields, this could lead so major issues which would be a royal pain in the ass to debug.  In order to prevent this possible problem, I wanted to have a safe way to keep state that should be available for the duration of a single request.  

If your code executes in an ASP.NET environment, you can safely use the HttpContext.Current.Items dictionary for this.  If your code executes in a WCF environment, you can store these things in the OperationContext.  I don't want my code to be tightly coupled to either ASP.NET or WCF, so I wanted some kind of abstraction.  This is the approach that I came up with.

First, we have the IRequestState interface:

<script src="https://gist.github.com/3684186.js?file=s1.cs"></script>

This just offers a way to store objects and retrieve them.  That's pretty much al we need, right?

Then we have the ASP.NET implementation:

<script src="https://gist.github.com/3684186.js?file=s2.cs"></script>

Very simple stuff... the AspNetRequestState implementation simply uses the HttpContext.Current.Items dictionary underneath to store and retrieve the objects.

For WCF, it is slightly more complicated.  Every WCF call is an operation and it has a context as well, which is provided through the OperationContext class.  The OperationContext class doesn't have an Items dictionary like HttpContext does, but it does have a way to add extensions to the context.  We can use this extensions mechanism to store state which should be kept around for the duration of the current WCF operation.  First, we need to define our Extension:

<script src="https://gist.github.com/3684186.js?file=s3.cs"></script>

The IExtension interface that we must implement defines the Attach and Detach methods but we don't really need them for what we're trying to do.  This extension simply initializes a Dictionary instance and exposes it with a public getter.  Now we can easily create our WcfRequestState implementation:

<script src="https://gist.github.com/3684186.js?file=s4.cs"></script>

Pretty simple as well, and pretty similar to the AspNetRequestState implementation.  The AspNetRequestState implementation is able to simply use the HttpContext.Current.Items dictionary, which we can't use here.  So when we want to access the 'State' dictionary in this implementation, we look it up in the current OperationContext's Extensions collection.  If it's not there yet, we add a new instance of our MyExtension class to the OperationContext's Extensions collection.

Now we can use this wherever we need to store something for the duration of the current request, regardless of whether we're executing in an ASP.NET or WCF context.  Just configure your IoC container to create instances of AspNetRequestState whenever an IRequestState instance is needed in your WebApplication, or configure it to return WcfRequestState instances in your WCF service.  The code that needs to store some request state will no longer have to resort to using ThreadStatic fields, and it doesn't need to know about it's runtime environment either.  It merely needs an instance of IRequestState.
