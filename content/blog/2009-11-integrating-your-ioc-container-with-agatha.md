<p>Agatha relies on the presence of an IOC container, both client-side as well as server-side.&#160; When it was still a closed-source library used by my company, we could get away with using our preferred IOC container (Castle Windsor) directly.&#160; Obviously, when making an open-source library available you want your users to have the ability to not only use their preferred container, but to integrate with their container as well.&#160; With ‘integrate’ i mean that it should be possible to register Agatha’s components in your container, so that you can easily resolve Agatha components (such as the IRequestDispatcher or the IAsyncRequestDispatcher), or to have the ability to have the container automatically inject your dependencies (which are registered in your container) in your Request Handlers (which are resolved and used by Agatha).</p>  <p>I originally planned to use the <a href="http://www.codeplex.com/CommonServiceLocator" target="_blank">Common Service Locator</a> project for this, because the project description certainly seemed to fit my need:</p>  <blockquote>   <p>The Common Service Locator library contains a shared interface for service location which application and framework developers can reference. The library provides an abstraction over IoC containers and service locators. Using the library allows an application to indirectly access the capabilities without relying on hard references. The hope is that using this library, third-party applications and frameworks can begin to leverage IoC/Service Location without tying themselves down to a specific implementation.</p> </blockquote>  <p>Unfortunately, the shared interface defined in this project only contains method for <em>resolving</em> components.&#160; I really wanted to avoid having an Agatha-user be responsible for the correct <em>registration</em> of each component in their container, so the Common Service Locator project doesn’t really offer me any benefits.&#160; Instead, i defined my own IContainer interface in Agatha which looks like this:</p> 

<div>
[csharp]
    public interface IContainer
    {
        void Register(Type componentType, Type implementationType, Lifestyle lifeStyle);
        void Register&lt;TComponent, TImplementation&gt;(Lifestyle lifestyle);
        void RegisterInstance(Type componentType, object instance);
        void RegisterInstance&lt;TComponent&gt;(TComponent instance);
 
        TComponent Resolve&lt;TComponent&gt;();
        object Resolve(Type componentType);
 
        void Release(object component);
    }
[/csharp]
</div>

<p>This interface enables me to perform automatic registration of all of the required components, and whenever Agatha needs to resolve components it will also do so through an instance which implements this interface.&#160; We currently have two specific implementations of this interface: one for Castle Windsor, the other for Unity.&#160; I’m going to cover how these implementations work later on in this post (as it will be useful to anyone who wants to use another IOC container together with Agatha) but first i want to show how Agatha will either obtain a reference to your container, or instantiate its own container if you don’t give it a container instance.</p>  <p>Agatha has two configuration classes: ServiceLayerConfiguration and ClientConfiguration.&#160; ServiceLayerConfiguration defines the following constructors:</p> 

<div>
[csharp]
        public ServiceLayerConfiguration(Assembly requestHandlersAssembly, Assembly requestsAndResponsesAssembly, IContainer container)
        {
            // not relevant to this post, so i'm skipping this
        }
 
        public ServiceLayerConfiguration(Assembly requestHandlersAssembly, Assembly requestsAndResponsesAssembly, Type containerImplementation)
        {
            // not relevant to this post, so i'm skipping this
        }
[/csharp]
</div>

<p>And ClientConfiguration defines the following ones:</p> 

<div>
[csharp]
        public ClientConfiguration(Assembly requestsAndResponsesAssembly, IContainer container)
        {
            // not relevant to this post, so i'm skipping this
        }
 
         public ClientConfiguration(Assembly requestsAndResponsesAssembly, Type containerImplementation)
        {
            // not relevant to this post, so i'm skipping this
        }
[/csharp]
</div>

<p>As you can see, you can pass either an instance of an object that implements Agatha’s IContainer interface, or you can just pass in the type of the IContainer implementation.&#160; If you pass in an instance, Agatha will simply reuse that instance for both the registration and the resolving of components.&#160; If you pass a type instead of an instance, Agatha will create its own instance and use that for the registration and resolving of components.</p>  <p>Now, how do you integrate your own container? Quite simple, just create a type which implements the IContainer interface and pass an instance of that type to Agatha’s configuration objects.&#160; As an example, i’ll show how i did it for Castle Windsor.&#160; I created a new assembly called Agatha.Castle which contains the following Container class:</p> 

<div>
[csharp]
using System;
using Agatha.Common.InversionOfControl;
using Castle.MicroKernel.Registration;
using Castle.Windsor;
 
namespace Agatha.Castle
{
    public class Container : IContainer
    {
        private readonly IWindsorContainer windsorContainer;
 
        public Container() : this(new WindsorContainer()) {}
 
        public Container(IWindsorContainer windsorContainer)
        {
            this.windsorContainer = windsorContainer;
        }
 
        public void Register(Type componentType, Type implementationType, Lifestyle lifeStyle)
        {
            var registration = Component.For(componentType).ImplementedBy(implementationType);
            windsorContainer.Register(AddLifeStyleToRegistration(lifeStyle, registration));
        }
 
        public void Register&lt;TComponent, TImplementation&gt;(Lifestyle lifestyle)
        {
            Register(typeof(TComponent), typeof(TImplementation), lifestyle);
        }
 
        public void RegisterInstance(Type componentType, object instance)
        {
            windsorContainer.Register(Component.For(componentType).Instance(instance));
        }
 
        public void RegisterInstance&lt;TComponent&gt;(TComponent instance)
        {
            RegisterInstance(typeof(TComponent), instance);
        }
 
        public TComponent Resolve&lt;TComponent&gt;()
        {
            return windsorContainer.Resolve&lt;TComponent&gt;();
        }
 
        public object Resolve(Type componentType)
        {
            return windsorContainer.Resolve(componentType);
        }
 
        public void Release(object component)
        {
            windsorContainer.Release(component);
        }
 
        private static ComponentRegistration&lt;TInterface&gt; AddLifeStyleToRegistration&lt;TInterface&gt;(Lifestyle lifestyle, ComponentRegistration&lt;TInterface&gt; registration)
        {
            if (lifestyle == Lifestyle.Singleton)
            {
                registration = registration.LifeStyle.Singleton;
            }
            else if (lifestyle == Lifestyle.Transient)
            {
                registration = registration.LifeStyle.Transient;
            }
            else
            {
                throw new ArgumentOutOfRangeException(&quot;lifestyle&quot;, &quot;Only Transient and Singleton is supported&quot;);
            }
 
            return registration;
        }
    }
}
[/csharp]
</div>

<p>The Agatha.Castle.Container class defines two constructors.&#160; One is the default constructor, which will create a new instance of the Windsor container, and the other requires you to pass an instance to a Windsor container.&#160; The rest of the type simply implements Agatha’s IContainer interface methods and delegates to the real container instance.&#160; If you pass an instance of the Agatha.Castle.Container class to Agatha’s ServiceLayerConfiguration class, Agatha will use this type to register and resolve all components.&#160; Which means it either reuses <em>your</em> container instance, or creates its own if you don’t pass one (which i’d only recommend if you’re not using an IOC container in your code). This makes it pretty easy to integrate whatever IOC container you want to use.&#160; </p>  <p>Agatha currently has ‘out-of-the-box’ support for Castle Windsor and Microsoft’s Unity.&#160; If you want to use your own container, you now know how easily you can integrate it with Agatha.&#160; And i’d be very happy to accept patches which add more Agatha.YourPreferredContainer assemblies :)</p>