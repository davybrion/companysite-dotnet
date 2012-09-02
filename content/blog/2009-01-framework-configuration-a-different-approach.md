Last week i spent some time at work moving some common infrastructure classes which were used in multiple projects into its own little 'framework' assembly. This framework basically just provides everything we need to use the <a href="http://davybrion.com/blog/2008/07/the-request-response-service-layer/">Request/Response Service Layer</a>, the <a href="http://davybrion.com/blog/2008/07/the-known-type-provider/">KnownTypeProvider</a>, some stuff built on top of NHibernate (the <a href="http://davybrion.com/blog/2008/06/the-query-batcher/">QueryBatcher</a>, <a href="http://davybrion.com/blog/2008/06/data-access-with-nhibernate/">Repository</a> and <a href="http://davybrion.com/blog/2008/06/managing-your-nhibernate-sessions/">UnitOfWork</a> implementations), our own MVP implementation for <a href="http://davybrion.com/blog/2008/07/how-to-write-testable-aspnet-webforms/">WebForms</a> and <a href="http://davybrion.com/blog/2008/10/how-to-write-testable-aspnet-usercontrols/">UserControls</a>, the Dispatcher (for easy communication with the Request/Response service layer, see that post for more details), and some Silverlight equivalents of some of these classes that were written by a coworker of mine.  

This little framework uses the Windsor IoC container quite heavily so naturally, everything needs to be registered properly.  You also need to register your known Request/Response types with the KnownTypeProvider and you have to configure the NHibernate components to use the correct mapping files as well.  The framework provides default implementations for pretty much all of the components, except for some that you need to provide implementations for yourself, although abstract base classes are available in the framework assembly which you can inherit from.  So i needed something which would allow me to easily configure the framework in every project that uses it, making it possible to override the implementation of certain components or use the default implementations when these are sufficient.  I really wanted to avoid the typical XM-Hell that this usually leads to, so i wanted to go for a code-only approach.  

First of all, the business side behind the Request/Response WCF service.  There's two ways to use this, with or without NHibernate support (in case we have to work for... umm... conservative clients).  So we start off with the following class:

<div>
[csharp]
namespace Trinity.Business
{
    public class Registration
    {
        public Registration(Assembly commonAssembly, Assembly businessAssembly)
        {
            CommonAssembly = commonAssembly;
            BusinessAssembly = businessAssembly;
            SetDefaultImplementationTypes();
        }
 
        public Assembly BusinessAssembly { get; private set; }
        public Assembly CommonAssembly { get; private set; }
        public Type IRequestProcessorImplementation { get; set; }
 
        private void SetDefaultImplementationTypes()
        {
            IRequestProcessorImplementation = typeof(RequestProcessor);
        }
    }
}
[/csharp]
</div>

Nothing spectacular... This Registration class can be instantiated with two assembly references.  The first is a Common assembly, which contains the known Request/Response types that will be registered with the KnownTypeProvider.  The second is the business assembly which contains all of the IRequestHandler implementations to handle each Request type so we can return a Response.   Finally, you can set the implementation type of the IRequestProcessor interface that the framework should use at runtime.  Notice how this is optional and that a default implementation type is set.

We also have another Registration class which contains some more configuration possibilities for NHibernate usage:

<div>
[csharp]
    public class RegistrationWithNHibernateSupport : Registration
    {
        public RegistrationWithNHibernateSupport(Assembly commonAssembly, Assembly businessAssembly,
            string mappingAssemblyName) : base(commonAssembly, businessAssembly)
        {
            MappingAssemblyName = mappingAssemblyName;
            SetDefaultImplementations();
        }
 
        public string MappingAssemblyName { get; private set; }
        public Type ISessionProviderImplementation { get; set; }
        public Type IActiveSessionManagerImplementation { get; set; }
        public Type IUnitOfWorkImplementation { get; set; }
 
        private void SetDefaultImplementations()
        {
            ISessionProviderImplementation = typeof(SessionProvider);
            IActiveSessionManagerImplementation = typeof(ActiveSessionManager);
            IUnitOfWorkImplementation = typeof(UnitOfWork);
        }
    }
[/csharp]
</div>

Here we have an extra required constructor parameter to provide the name of the assembly which contains the embedded NHibernate mapping files.  We also have 3 properties which enable you to provide implementation types for the ISessionProvider, IActiveSessionManager and IUnitOfWork interfaces.  Again, the default implementation types are set automatically.

We can now configure the business side of the framework with the following two static methods in the Register class:

<div>
[csharp]
    public static class Register
    {
        public static void WithNHibernateSupport(RegistrationWithNHibernateSupport registration)
        {
            Normal(registration);
            RegisterNHibernateComponents(registration);
            RegisterAllRepositories(registration.BusinessAssembly);
        }
 
        public static void Normal(Registration registration)
        {
            RegisterRequestProcessor(registration);
            RegisterRequestAndResponseTypes(registration.CommonAssembly);
            RegisterRequestHandlers(registration.BusinessAssembly);
        }
    }
[/csharp]
</div>

I left out the implementations of the private methods for brevity.  Although i will show you what the implementation of the NHibernate components looks like:

<div>
[csharp]
        private static void RegisterNHibernateComponents(RegistrationWithNHibernateSupport registration)
        {
            IoC.Container.Register(Component.For&lt;ISessionProvider&gt;()
                .ImplementedBy(registration.ISessionProviderImplementation)
                .Parameters(Parameter.ForKey(&quot;mappingAssemblyName&quot;).Eq(registration.MappingAssemblyName))
                .LifeStyle.Singleton);
 
            IoC.Register(typeof(IActiveSessionManager), registration.IActiveSessionManagerImplementation, IoC.LifeStyle.Singleton);
            IoC.Register(typeof(IUnitOfWork), registration.IUnitOfWorkImplementation);
 
            // these 2 component implementations can not be overridden by consumers (why yes, that _is_ on purpose)
            IoC.Register&lt;IQueryBatcherFactory, QueryBatcherFactory&gt;(IoC.LifeStyle.Singleton);
            IoC.Register&lt;IQueryBatcher, QueryBatcher&gt;();
        }
[/csharp]
</div>

As you can see, it always uses the implementation types that are set in the Registration instance, which refer to the default implementations if you didn't overwrite the property values.

So how do we configure this to be usable in one of our projects? It's pretty easy:

<div>
[csharp]
            Assembly businessAssembly = Assembly.GetExecutingAssembly();
            Assembly commonAssembly = typeof(EmsRequest).Assembly;
 
            var registration = new RegistrationWithNHibernateSupport(commonAssembly, businessAssembly, &quot;EMS.Mappings&quot;)
                {
                    IRequestProcessorImplementation = typeof(EmsRequestProcessor),
                    ISessionProviderImplementation = typeof(Infrastructure.NHibernate.SessionProvider)
                };
 
            Trinity.Business.Register.WithNHibernateSupport(registration);
[/csharp]
</div>

As you can see i am providing my assembly containing my common Request/Response types, and my business assembly.  I also provide the name of the assembly containing my mapping files, and i overwrite the implementation of 2 components in the framework.  And that's about it.

It works the same way in the Web layer... for that i have the following Registration class:

<div>
[csharp]
namespace Trinity.WebForms
{
    public class Registration
    {
        public Registration(Assembly commonAssembly, Assembly webAssembly,
            Type resourceManagerFactoryImplementation)
        {
            WebAssembly = webAssembly;
            CommonAssembly = commonAssembly;
            IResourceManagerFactoryImplementation = resourceManagerFactoryImplementation;
            SetDefaultImplementationTypes();
        }
 
        public Assembly WebAssembly { get; private set; }
        public Assembly CommonAssembly { get; private set; }
 
        public Type IResourceManagerFactoryImplementation { get; set; }
        public Type ITextRepositoryManagerImplementation { get; set; }
        public Type ISessionStateImplementation { get; set; }
        public Type IRequestProcessorImplementation { get; set; }
        public Type IDispatcherImplementation { get; set; }
 
        private void SetDefaultImplementationTypes()
        {
            ITextRepositoryManagerImplementation = typeof(TextRepositoryManager);
            ISessionStateImplementation = typeof(WebSessionState);
            IRequestProcessorImplementation = typeof(RequestProcessorProxy);
            IDispatcherImplementation = typeof(Dispatcher);
        }
    }
}
[/csharp]
</div>

And well, i guess you can imagine what the rest looks like :)