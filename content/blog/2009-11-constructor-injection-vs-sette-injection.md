For those of you who've used Dependency Injection, you know that the two most common ways of injecting a dependency into a class are constructor injection and setter injection.  For those of you who haven't used Dependency Injection yet, here's a simple example which shows both techniques:

<div>
[csharp]
    public class MyService : IMyService
    {
        private IRequiredDependency requiredDependency;
 
        public IOptionalDependency OptionalDependency { get; set; }
 
        public MyService(IRequiredDependency requiredDependency)
        {
            this.requiredDependency = requiredDependency;
        }
 
        public void DoSomething()
        {
            // do something cool and/or important
            // ...
        }
    }
[/csharp]
</div>

This example is very abstract, but it should be pretty clear.  Constructor injection is used to inject the required dependency whenever an instance of MyService is created, whereas setter injection is used to inject the optional dependency <strong>after</strong> the instance is created.  I obviously can't speak for everyone who uses Dependency Injection, but generally speaking most people use constructor injection for required dependencies and setter injection for optional dependencies.

There is however one situation in which i prefer setter injection for required dependencies over constructor injection: dependencies of abstract classes or base classes.  For instance, in our service layer each incoming request is handled by a specific RequestHandler.  Most of our RequestHandlers need our NHibernate infrastructure to be set up, which is automatically taken care of by our UnitOfWork implementation.  So we have the following NhRequestHandler class (simplified for the purpose of this blog post):

<div>
[csharp]
    public abstract class NhRequestHandler&lt;TRequest, TResponse&gt; : RequestHandler&lt;TRequest, TResponse&gt;
        where TRequest : Request
        where TResponse : Response
    {
        public IUnitOfWork UnitOfWork { get; set; }
 
        public override Response Handle(Request request)
        {
            using (ITransaction transaction = UnitOfWork.CreateTransaction())
            {
                Response response;
 
                try
                {
                    response = base.Handle(request); // calls the specific Handle(TRequest) method of the derived handler
                    transaction.Commit();
                }
                catch (Exception e)
                {
                    transaction.Rollback();
                    throw;
                }
 
                return response;
            }
        }
    }
[/csharp]
</div>

As you can see, the IUnitOfWork dependency is a required dependency because you would get a NullReferenceException when trying to handle a request without having a IUnitOfWork instance present.  Yet, i really don't want to put it in the constructor because then each and every RequestHandler that derives from this will also have to put it in the constructor, even though most of them won't access the IUnitOfWork directly.

Actually, most of our applications inherit from the NhRequestHandler and then add some more dependencies that some kind of base BusinessRequestHandler will need.  These are dependencies to deal with authentication, authorization, user context, application context, etc... Some of these dependencies will be used by the derived RequestHandlers, some won't.  All of them however will indeed be used by the BusinessRequestHandler so they are definitely required dependencies.  Using constructor injection for these dependencies would lead to 'noise' in every derived RequestHandler's constructor.

Instead, we use setter injection for all of a base-type's dependencies, and use constructor injection only for the dependencies of the derived types.  It keeps the constructors as clean as they can be and avoids unnecessary noise.  We know that our IOC container will fulfill all the constructor dependencies as well as each property dependency in the inheritance hierarchy so there's no chance of anything going wrong there.  Unless of course somebody seriously breaks the IOC configuration but in that case, our applications won't even make it through the simplest of requests so that's not something that will ever happen unnoticed.

And for our tests, we always inherit our fixtures from things like HandlerTest or ControllerTest or whatever where all of those property dependencies are automatically set up with mocks or stubs, so it doesn't really cause problems there either.