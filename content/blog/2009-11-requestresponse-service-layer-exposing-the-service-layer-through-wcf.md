Note: This post is part of a series. Be sure to read the introduction <a href="/blog/2009/11/requestresponse-service-layer-series/">here</a>.

The previous posts in the series already covered everything there is to know about implementing the Request/Response Service Layer (RRSL).  In this post, we'll focus on hosting the RRSL through WCF and the next posts will show you how you can actually use the RRSL from your clients (both synchronously and asynchronously).  First, let's take another look at what our service actually exposes:

<script src="https://gist.github.com/3685498.js?file=s1.cs"></script>

This interface doesn't contain the necessary WCF attributes, but we'll get to that in a minute.  The first thing that we need to take care of is to make sure that WCF's DataContractSerializer can handle the derived Request and Response types without us having to declare the KnownType attribute on each derived class.  Luckily for us, WCF has a ServiceKnownType attribute where you can define a class that will provide the KnownTypes to WCF so it can properly serialize and deserialize them.  But first, we'll need a KnownTypeProvider class:

<script src="https://gist.github.com/3685498.js?file=s2.cs"></script>

Now we just have to make sure that all of our Request and Response types are registered before the application is initialized.  You basically need to make sure that the following code is called before the first request is sent to the service:

<script src="https://gist.github.com/3685498.js?file=s3.cs"></script>

Now we can actually define our Service Contract.  Instead of putting the WCF attributes on the IRequestProcessor interface, I decided to put a different interface next to it.  The reason I chose to go that route is merely to make sure that the actual IRequestProcessor interface and it's implementation isn't tied to WCF.  And that's why we have the IWcfRequestProcessor interface:

<script src="https://gist.github.com/3685498.js?file=s4.cs"></script>

This is a 'properly' defined WCF Service Contract, and we also make sure that WCF knows where to get the list of known types by using the ServiceKnownType attribute and hooking it to our KnownTypeProvider.  The implementation of this service contract looks like this:

<script src="https://gist.github.com/3685498.js?file=s5.cs"></script>

As you can see, there's nothing to it... The implementation of the Process method simply resolves the real IRequestProcessor implementation, calls the Process method and returns the Responses.  That's it.

Now we still need to host the service somewhere.  I always host it through IIS but you can use any of the other WCF hosting options as well.  In case you're hosting it in a Web Application, add a RequestProcessorService.svc file to your Web Application with the following content:

<script src="https://gist.github.com/3685498.js?file=s6.xml"></script>

In your web.config (or app.config if you're hosting it yourself or through a windows service), add the following block:

<script src="https://gist.github.com/3685498.js?file=s7.xml"></script>

I included two binding configurations in this example, but you obviously only need one.  In this case, the service endpoint is configured to use the custom binaryHttpBinding.  This uses binary XML which (in most cases) is more compact and thus, has less bandwidth overhead.  If you want to use a regular basicHttpBinding, just modify the service endpoint configuration to point to the other binding.  Obviously, you can use wsHttpBinding or any other binding as well if you want to.

And that is it.  That's really all you need to do to host the RRSL through WCF.  You'll never have to edit or add anything to the xml configuration as you add more functionality to your service layer.  It works now, and it will keep working.

Just keep in mind that you need to register the Request and Response types with the KnownTypeProvider <em>before</em> the service receives the first request ;)