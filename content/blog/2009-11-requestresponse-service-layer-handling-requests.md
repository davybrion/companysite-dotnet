Note: This post is part of a series. Be sure to read the introduction <a href="http://davybrion.com/blog/2009/11/requestresponse-service-layer-series/">here</a>.

We already covered how you can define your request/response types, and how they can be processed.  Now it's time to deal with how they can be handled by their respective Request Handlers.  As shown in the previous post, we have the following 2 interfaces:

<script src="https://gist.github.com/3685457.js?file=s1.cs"></script>

Alright, let's get started with their implementations.  First we have the regular RequestHandler class:

<script src="https://gist.github.com/3685457.js?file=s2.cs"></script>

And then a generic RequestHandler class which inherits from the previous class:

<script src="https://gist.github.com/3685457.js?file=s3.cs"></script>

When the Request Processor delegates the handling of a request to its Request Handler, it calls the Handle method which accepts a parameter of type Request.  The implementation of that method will first call a virtual BeforeHandle method, then the typed Handle method which accepts a parameter of type TRequest, and then the virtual AfterHandle method.  This gives each derived Request Handler the ability to put some custom logic before and after the actual handling of a request.

You'll hardly ever inherit directly from the generic RequestHandler class.  In most cases, you'll put another class in between your actual handlers and this one.  For our NHibernate applications, we have the following custom NhRequestHandler class that each application bases its handlers on:

<script src="https://gist.github.com/3685457.js?file=s4.cs"></script>

This class requires an IUnitOfWork instance which is injected automatically by the IOC container.  I prefer to use Setter Injection for dependencies in base classes instead of Constructor Injection but you could obviously just as easily have the IUnitOfWork injected through the constructor if you'd prefer so.

The interesting part is that this class overrides the Handle(Request) method instead of the Handle(TRequest) method.  It first creates a transaction through the UnitOfWork and will then call the base class' Handle(Request) method.  The 'tricky' part here is that the base implementation of Handle(Request) will in turn call the typed BeforeHandle(TRequest), Handle(TRequest) and AfterHandle(TRequest) methods in the derived class, all of which will be executed within the scope of the UnitOfWork's transaction.  

Should an exception occur, the transaction is automatically rolled back and otherwise it's automatically committed.  Each derived Request Handler can simply declare its dependencies in its constructor (or through property setters if that's what you want) and only needs to implement the Handle(TRequest) method.  It can of course also implement the BeforeHandle(TRequest) and AfterHandle(TRequest) methods when it makes sense. 

This is a simple example of an NHibernate-enabled base class for Request Handlers.  We also have one of these for projects that don't use NHibernate but use <a href="http://davybrion.com/blog/2009/08/build-your-own-data-access-layer-series/">our custom DAL</a> instead.  It's virtually identical to this one though.  The important part to remember is that these classes are very simple, yet still give you a lot of flexibility and power.

In fact, most of our applications' Request Handlers don't inherit directly from the NhRequestHandler (or the AdoNetRequestHandler).  In most cases, we have another class in between the actual Request Handlers and the NhRequestHandler which adds custom logic for authentication, provides hooks (virtual methods) for authorization and whatever else that might make sense (like setting up the user's context for the duration of the request).

In fact, here's a simple, real world example:

<script src="https://gist.github.com/3685457.js?file=s5.cs"></script>

For this particular application, each Request Handler inherits from this BusinessRequestHandler and before the actual Handle(TRequest) method is called, the implementation of BeforeHandle(TRequest) will make sure that the current request is properly authenticated and if so, will store some user data in the UserContext instance so that each Request Handler can make use of it.  This means that each single request gets authenticated (we obviously make use of caching here).  If however you only want to authenticate say, the first request of each batch instead of all of them then you can inherit from the default RequestProcessor class (covered in the previous post in this series) and plug in the authentication logic at that point.  The point is that this entire approach is extremely flexible.

Here's another real world example:

<script src="https://gist.github.com/3685457.js?file=s6.cs"></script>

Very similar to the previous one, but this version includes a virtual Authorize(TRequest) method which each Request Handler can implement to perform Authorization in a uniform manner.

As you can see, we can deal with a lot of cross-cutting concerns in a single place without having to resort to Aspect Oriented Programming.  That's not to say that Aspect Oriented Programming is bad (in fact, i quite like it), but in a lot of cases you can achieve the same result with simple Object Oriented Programming.

Anyway, once you have this in place you can create very simple Request Handlers which simply inherit from your custom base handler class, declare their dependencies as either constructor arguments or as public properties so the IOC container can inject them, and can then focus on simply implementing whatever logic is required to handle a specific request.  They can be sure that everything is properly set up for them to function in the environment that they are ment to function in, and that all of this set up only occurs in one place in the code.