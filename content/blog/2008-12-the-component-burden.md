In my <a href="http://davybrion.com/blog/2008/12/the-importance-of-releasing-your-components-through-windsor/">previous post</a> i showed you how important it is to properly release the components you've resolved through the Windsor IoC container.  At the end of that post, i showed an example where a disposable dependency of a component had to be disposed by the component.  Basically something like this:

<div>
[csharp]
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
        }
    }
[/csharp]
</div>

This prompted the following <a href="http://elegantcode.com/2008/12/13/the-importance-of-releasing-your-components-through-windsor/#comment-39632">comment</a> by Bryan Watts:

<blockquote>
So Controller is responsible for disposing of something it did not explicitly create? That’s a little presumptuous isn’t it?

<blockquote>
> if you own a reference to an IDisposable instance, you are responsible for properly disposing of that instance.
</blockquote>

I agree (and I’ve read your other article). However, in this case, Controller clearly does not own its dependency.
</blockquote>

And Bryan is right.  For a lot of languages, memory management rules can be summarized (somewhat simplified) like this:

<ul>
	<li>If you create an instance of a class, you are responsible for releasing/freeing/disposing it.</li>
	<li>If a factory creates an instance of a class, the factory is not responsible for releasing/freeing/disposing it.  This burden falls upon the object which requested the object from the factory</li>
	<li>If you receive an instance of an object, you should not release/free/dispose it, unless you received the instance from a factory</li>
</ul>

So where does this leave us with the example code shown above?  Clearly, we did not create the instance of the disposable dependency, so in theory, we are not allowed to dispose it.  Then again, if we weren't using Dependency Injection we would have probably instantiated the disposable dependency in our constructor (or when we first need it).  In that case, we would have been responsible for disposing the disposable dependency.  

You could argue that the IoC container is in some way a factory which creates and supplies the dependency to our class.  So according to the rules stated above, it would be allowed to dispose the dependency.   The downside to this is that this means that our class (the Controller) has knowledge of the lifecycle of the dependency.  We know that the dependency has been configured with a Transient lifecycle (which means the container creates a new instance whenever an instance of its class is needed) so we can safely dispose it.  The problem is that this knowledge is implicit.  It is nowhere visible in the code of the Controller class.  The only place where this lifecycle is configured explicitly, is in the configuration of the IoC container so it would be wrong for the Controller to assume anything about the lifecycle of its dependencies.  

After all, suppose someone makes a change to the code of the dependency which makes it possible to be used with a different lifecycle.  Suppose someone makes the change to the dependency and then configures the IoC container to always return a singleton instance of this dependency.  If nobody thinks about the fact that the Controller still assumes a Transient lifecycle of the dependency, the disposal of the dependency will not be removed from the Controller class.  Once a Controller instance is created, and then disposed, it will try to dispose the instance of its dependency, which will cause problems because that instance is supposed to be used by other objects as well!

So what do we do? Do we dispose of the dependency in the Controller class because we know it's safe (until someone changes the dependency and/or its configuration with the container)?  Or do we simply not dispose it (since we're not really allowed to do so without some kind of implicit knowledge of the 'outside world')?

Neither option sounds very appealing.  The first option <strong>could</strong> lead to problems depending on possible future changes outside of the Controller class.  The second option <strong>is already problematic</strong> because a disposable dependency is not being disposed of as fast as it should be.  The disposal would basically be postponed until the instance is collected by the garbage collector.

Another alternative is to have the container inject a factory which can create transient instances of the dependency, instead of having the container inject the transient instance directly.  This would fix all of the discussed problems: the controller would not need to dispose the injected dependency, and can safely dispose the instantiated disposable instances.  However, having to create factories simply to avoid this problem seems a bit cumbersome.  The implementation of the factory could lead to other discussions as well because of how the factory should create the actual instance <strong>and its dependencies</strong>, which is exactly the reason why we wanted to use an IoC container in the first place.  True, the factory could resolve the instance directly through the IoC container, but then you're just moving the problem to a different class without actually fixing it properly.

This entire problem has been described as the <a href="http://hammett.castleproject.org/?p=252">Component Burden</a> by Hammet, the original creator of the Windsor IoC container.  Obviously, this is something that the container should solve for us instead of us having to worry about it.  Ideally, the container would keep track of all of the disposable dependencies it has created to satisfy the creation of requested components.  Then when the requested component would be released, the container could safely dispose of each transient disposable dependency it created to create the requested component.  Luckily, the Castle developers have implemented this behavior <a href="http://www.nabble.com/Component-burden-impl-committed-td19848831.html">recently</a>, so if you're using a recent build of the Windsor IoC container,  this problem should no longer occur and you shouldn't be forced to dispose of dependencies that were injected into your objects. 

The reason why i'm not using it yet is because there were many attempts at implementing this, and most attempts had considerable downsides to them.  This one has been committed to the trunk, so it's probably a good solution, but i'm not going to use this solution in production systems until it has had some more time to prove itself.  So for now, i'm sticking with my approach where i manually dispose the injected dependencies.  I know it's not the best approach, but until the real solution in the container is somewhat more proven to work without downsides, manually disposing the dependencies seems to be the lesser of multiple evils.

Keep in mind that i'm only talking about the Windsor IoC container.  I have no experience with StructureMap, NInject or Unity, so i have no idea how these containers deal with this problems.  If anyone with knowledge about these container could shed some light on how they deal with this, i'd be very interested in reading it :)
