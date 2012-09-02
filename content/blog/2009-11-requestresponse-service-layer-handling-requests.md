Note: This post is part of a series. Be sure to read the introduction <a href="http://davybrion.com/blog/2009/11/requestresponse-service-layer-series/">here</a>.

We already covered how you can define your request/response types, and how they can be processed.  Now it's time to deal with how they can be handled by their respective Request Handlers.  As shown in the previous post, we have the following 2 interfaces:

<div>
[csharp]
    public interface IRequestHandler : IDisposable
    {
        Response Handle(Request request);
        Response CreateDefaultResponse();
    }
 
    public interface IRequestHandler&lt;TRequest&gt; : IRequestHandler where TRequest : Request
    {
        Response Handle(TRequest request);
    }
[/csharp]
</div>

Alright, let's get started with their implementations.  First we have the regular RequestHandler class:

<div>
[csharp]
    public abstract class RequestHandler : Disposable, IRequestHandler
    {
        public abstract Response Handle(Request request);
        public abstract Response CreateDefaultResponse();
 
        /// &lt;summary&gt;
        /// Default implementation is empty
        /// &lt;/summary&gt;
        protected override void DisposeManagedResources() { }
    }
[/csharp]
</div>

And then a generic RequestHandler class which inherits from the previous class:

<div>
[csharp]
    public abstract class RequestHandler&lt;TRequest, TResponse&gt; : RequestHandler, IRequestHandler&lt;TRequest&gt;
        where TRequest : Request
        where TResponse : Response
    {
        public override Response Handle(Request request)
        {
            var typedRequest = (TRequest)request;
            BeforeHandle(typedRequest);
            var response = Handle(typedRequest);
            AfterHandle(typedRequest);
            return response;
        }
 
        public virtual void BeforeHandle(TRequest request) {}
        public virtual void AfterHandle(TRequest request) {}
 
        public abstract Response Handle(TRequest request);
 
        public override Response CreateDefaultResponse()
        {
            return CreateTypedResponse();
        }
 
        public TResponse CreateTypedResponse()
        {
            return (TResponse)Activator.CreateInstance(typeof(TResponse));
        }
    }
[/csharp]
</div>

When the Request Processor delegates the handling of a request to its Request Handler, it calls the Handle method which accepts a parameter of type Request.  The implementation of that method will first call a virtual BeforeHandle method, then the typed Handle method which accepts a parameter of type TRequest, and then the virtual AfterHandle method.  This gives each derived Request Handler the ability to put some custom logic before and after the actual handling of a request.

You'll hardly ever inherit directly from the generic RequestHandler class.  In most cases, you'll put another class in between your actual handlers and this one.  For our NHibernate applications, we have the following custom NhRequestHandler class that each application bases its handlers on:

<div>
[csharp]
    public abstract class NhRequestHandler&lt;TRequest, TResponse&gt; : RequestHandler&lt;TRequest, TResponse&gt;
        where TRequest : Request
        where TResponse : Response
    {
        private readonly ILog logger = LogManager.GetLogger(typeof(NhRequestHandler&lt;TRequest, TResponse&gt;));
 
        public IUnitOfWork UnitOfWork { get; set; }
 
        protected override void DisposeManagedResources()
        {
            if (UnitOfWork != null) UnitOfWork.Dispose();
        }
 
        public override Response Handle(Request request)
        {
            using (ITransaction transaction = UnitOfWork.CreateTransaction())
            {
                Response response;
 
                try
                {
                    response = base.Handle(request);
                    transaction.Commit();
                }
                catch (Exception handlerException)
                {
                    try
                    {
                        transaction.Rollback();
                    }
                    catch (Exception)
                    {
                        logger.Error(&quot;NhRequestHandler: Rollback after exception failed! Original exception (the one that caused the rollback) was: &quot;, handlerException);
                        throw;
                    }
 
                    // note: there's no need to log the exception here, it will be logged by the RequestProcessor
                    throw;
                }
 
                return response;
            }
        }
    }
[/csharp]
</div>

This class requires an IUnitOfWork instance which is injected automatically by the IOC container.  I prefer to use Setter Injection for dependencies in base classes instead of Constructor Injection but you could obviously just as easily have the IUnitOfWork injected through the constructor if you'd prefer so.

The interesting part is that this class overrides the Handle(Request) method instead of the Handle(TRequest) method.  It first creates a transaction through the UnitOfWork and will then call the base class' Handle(Request) method.  The 'tricky' part here is that the base implementation of Handle(Request) will in turn call the typed BeforeHandle(TRequest), Handle(TRequest) and AfterHandle(TRequest) methods in the derived class, all of which will be executed within the scope of the UnitOfWork's transaction.  

Should an exception occur, the transaction is automatically rolled back and otherwise it's automatically committed.  Each derived Request Handler can simply declare its dependencies in its constructor (or through property setters if that's what you want) and only needs to implement the Handle(TRequest) method.  It can of course also implement the BeforeHandle(TRequest) and AfterHandle(TRequest) methods when it makes sense. 

This is a simple example of an NHibernate-enabled base class for Request Handlers.  We also have one of these for projects that don't use NHibernate but use <a href="http://davybrion.com/blog/2009/08/build-your-own-data-access-layer-series/">our custom DAL</a> instead.  It's virtually identical to this one though.  The important part to remember is that these classes are very simple, yet still give you a lot of flexibility and power.

In fact, most of our applications' Request Handlers don't inherit directly from the NhRequestHandler (or the AdoNetRequestHandler).  In most cases, we have another class in between the actual Request Handlers and the NhRequestHandler which adds custom logic for authentication, provides hooks (virtual methods) for authorization and whatever else that might make sense (like setting up the user's context for the duration of the request).

In fact, here's a simple, real world example:

<div>
[csharp]
    public abstract class BusinessRequestHandler&lt;TRequest, TResponse&gt; : NhRequestHandler&lt;TRequest, TResponse&gt;
        where TRequest : MyApplicationRequest
        where TResponse : Response
    {
        public IAuthenticator Authenticator { get; set; }
        public IUserCredentialRepository UserCredentialRepository { get; set; }
        public IUserContext UserContext { get; set; }
 
        public override void BeforeHandle(TRequest request)
        {
            var authenticatedRequest = request as AuthenticatedRequest;
 
            if (authenticatedRequest != null)
            {
                var userCredential = UserCredentialRepository.FindByLoginName(authenticatedRequest.LoginName);
 
                if (!Authenticator.Authenticate(authenticatedRequest.LoginName, authenticatedRequest.PasswordHash,
                    userCredential))
                {
                    throw new SecurityException(MessageKeys.RequestCouldNotBeAuthenticated);
                }
 
                UserContext.CurrentLanguage = authenticatedRequest.CurrentLanguage;
                UserContext.UserId = userCredential.Id;
                UserContext.LoginName = authenticatedRequest.LoginName;
            }
        }
    }
[/csharp]
</div>

For this particular application, each Request Handler inherits from this BusinessRequestHandler and before the actual Handle(TRequest) method is called, the implementation of BeforeHandle(TRequest) will make sure that the current request is properly authenticated and if so, will store some user data in the UserContext instance so that each Request Handler can make use of it.  This means that each single request gets authenticated (we obviously make use of caching here).  If however you only want to authenticate say, the first request of each batch instead of all of them then you can inherit from the default RequestProcessor class (covered in the previous post in this series) and plug in the authentication logic at that point.  The point is that this entire approach is extremely flexible.

Here's another real world example:

<div>
[csharp]
    public abstract class BusinessRequestHandler&lt;TRequest, TResponse&gt; : NhRequestHandler&lt;TRequest, TResponse&gt;
        where TRequest : Request
        where TResponse : Response
    {
        public IAuthenticator Authenticator { get; set; }
        public IAuthenticationContext AuthenticationContext { get; set; }
        public IAuthorizationProvider AuthorizationProvider { get; set; }
        public IConfigurationProvider ConfigurationProvider { get; set; }
 
        protected override void DisposeManagedResources()
        {
            AuthorizationProvider = null;
            AuthenticationContext = null;
            ConfigurationProvider = null;
            base.DisposeManagedResources();
        }
 
        public override void BeforeHandle(TRequest request)
        {
            log4net.MDC.Set(&quot;Tenant&quot;, ConfigurationProvider.TenantName);
            base.BeforeHandle(request);
            Authenticate(request);
            Authorize(request);
        }
 
        public virtual void Authorize(TRequest request) {}
 
        private void Authenticate(TRequest request)
        {
            var emsRequest = request as EmsRequest;
 
            if (emsRequest != null)
            {
                if (Authenticator.AreValidCredentials(emsRequest.ApplicationUserId, emsRequest.ApplicationUserName, emsRequest.PasswordHash))
                {
                    AuthenticationContext.SetContextData(emsRequest.ApplicationUserId, emsRequest.ApplicationUserName);
                }
                else
                {
                    throw new SecurityException(&quot;request could not be authenticated!&quot;);
                }
            }
        }
    }
[/csharp]
</div>

Very similar to the previous one, but this version includes a virtual Authorize(TRequest) method which each Request Handler can implement to perform Authorization in a uniform manner.

As you can see, we can deal with a lot of cross-cutting concerns in a single place without having to resort to Aspect Oriented Programming.  That's not to say that Aspect Oriented Programming is bad (in fact, i quite like it), but in a lot of cases you can achieve the same result with simple Object Oriented Programming.

Anyway, once you have this in place you can create very simple Request Handlers which simply inherit from your custom base handler class, declare their dependencies as either constructor arguments or as public properties so the IOC container can inject them, and can then focus on simply implementing whatever logic is required to handle a specific request.  They can be sure that everything is properly set up for them to function in the environment that they are ment to function in, and that all of this set up only occurs in one place in the code.