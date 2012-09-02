I wrote a post last week about a memory leak i had introduced in my code due to <a href="http://davybrion.com/blog/2008/12/the-importance-of-releasing-your-components-through-windsor/">not properly releasing resolved components through the Windsor IoC container</a>.  I wanted to try to make sure that i'd never make that mistake again and this is the approach i came up with.

If you're using an IOC container it's important to not use it all over the place.  You basically use it in as few places as possible to resolve a component and you let the container sort out all of the dependencies.  So in the few places where you use the container directly, you need to resolve the component, and in case of transient components you also need to release them through the container.  Releasing it is very easy to forget, so i wanted something that would guarantee that the component would be properly released.  Enter the Resolvable class:

<div>
[csharp]
    public class Resolvable&lt;T&gt; : Disposable
    {
        private readonly T instance;
 
        public Resolvable() : this(null) {}
 
        public Resolvable(object argumentsAsAnonymousType)
        {
            if (argumentsAsAnonymousType == null)
            {
                instance = IoC.Container.Resolve&lt;T&gt;();
            }
            else
            {
                instance = IoC.Container.Resolve&lt;T&gt;(argumentsAsAnonymousType);
            }
        }
 
        public T Instance
        {
            get { return instance; }
        }
 
        protected override void DisposeManagedResources()
        {
            IoC.Container.Release(instance);
        }
    }
[/csharp]
</div>

The Resolvable class inherits from my <a href="http://davybrion.com/blog/2008/06/disposing-of-the-idisposable-implementation/">Disposable class</a>, so the Disposable pattern is correctly implemented.

From now on, instead of calling the container directly, i just instantiate a new Resolvable in a using block.  Let's try it out.

I'm reusing my test component with a dependency from one of the previous posts:

<div>
[csharp]
    public interface IDependency : IDisposable
    {
        bool Disposed { get; set; }
    }
 
    public class MyDependency : IDependency
    {
        public bool Disposed { get; set; }
 
        public void Dispose()
        {
            Disposed = true;
        }
    }
 
    public interface IController : IDisposable
    {
        bool Disposed { get; set; }
        IDependency Dependency { get; }
    }
 
    public class Controller : IController
    {
        public IDependency Dependency { get; private set; }
 
        public Controller(IDependency myDependency)
        {
            Dependency = myDependency;
        }
 
        public void Dispose()
        {
            Dependency.Dispose();
            Disposed = true;
        }
 
        public bool Disposed { get; set; }
    }
[/csharp]
</div>

Now, instead of resolving an IController directly through the container and having to dispose of it, i just do this:

<div>
[csharp]
        [Test]
        public void ResolvableInstanceIsProperlyReleasedAfterDisposal()
        {
            IoC.Container.Register(Component.For&lt;IController&gt;().ImplementedBy&lt;Controller&gt;().LifeStyle.Transient);
            IoC.Container.Register(Component.For&lt;IDependency&gt;().ImplementedBy&lt;MyDependency&gt;().LifeStyle.Transient);
 
            IController controller;
            IDependency dependency;
 
            using (var resolvable = new Resolvable&lt;IController&gt;())
            {
                controller = resolvable.Instance;
                dependency = controller.Dependency;
            }
 
            Assert.IsTrue(controller.Disposed);
            Assert.IsTrue(dependency.Disposed);
            Assert.IsFalse(IoC.Container.Kernel.ReleasePolicy.HasTrack(controller));
            Assert.IsFalse(IoC.Container.Kernel.ReleasePolicy.HasTrack(dependency));
        }
[/csharp]
</div>

The container doesn't hold the reference to the instance, and both the instance and its dependency is properly disposed. 