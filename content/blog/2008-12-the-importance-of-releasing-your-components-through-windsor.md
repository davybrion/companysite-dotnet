We ran into a huge memory leak this week. A bit of memory profiling with JetBrains' <a href="http://www.jetbrains.com/profiler/index.html">dotTrace</a> quickly showed that the Windsor IoC container was holding on to a lot of references. It turned out that I actually forgot to release the components I was requesting Windsor to construct for me. I use the container in my web-layer to compose every page's Controller with its dependencies. And in my service layer, I use the container to compose every RequestHandler and its dependencies. So that's two instances of the Container, in two separate AppDomains, and both are leaking a lot of memory due to my mistake.

My mistake was that after I was done with the components that I asked Windsor to resolve, I simply disposed them (which in turn would dispose their dependencies) and I figured that would be enough, since my components are registered as Transient. That means that each time you request a Transient component, you get a new instance. This led me to believe that Windsor wouldn't need to hold a reference to the constructed components so I figured that simply disposing them would be enough, since they are IDisposables. Disposing them is a good thing obviously, but because the container was still holding references to the requested components, that's still a lot of memory that is being wasted because even though you've disposed them, they aren't eligible for Garbage Collection until they are no longer accessible. And because the container kept references to them, they remained accessible and were never collected. And there was my memory leak. Oops. In order to prevent this problem in the future for myself and anyone else who reads this, let's go over a few examples which should make it clear how you should make sure that your components are properly released so they are eligible for garbage collection.

Let's start really simple. We have an IController interface and a simple Controller class which implements the IController interface:

<script src="https://gist.github.com/3684127.js?file=s1.cs"></script>

In our tests, we'll use the following method to create and configure the container:

<script src="https://gist.github.com/3684127.js?file=s2.cs"></script>

The IController interface is registered with the container, and the container will return a new Controller instance (because of the Transient lifestyle) whenever someone requests an IController instance. The following test highlights the memory leak that I was experiencing:

<script src="https://gist.github.com/3684127.js?file=s3.cs"></script>

When you request a component from the Container, it keeps a reference to that instance in its Kernel's ReleasePolicy object. If you merely dispose of your requested component, the ReleasePolicy still holds the reference to the component. This is what caused my memory leak. So how do we avoid this problem? It's pretty easy actually:

<script src="https://gist.github.com/3684127.js?file=s4.cs"></script>

Instead of just disposing our controller, we tell the container to release it. The container in turn knows that because IController inherits from IDisposable, it should dispose the Controller instance. It also removes the instance from its Kernel's ReleasePolicy object and once your own reference to the Controller instance goes out of scope, it's eligible to be collected by the Garbage Collector. As you can see, it's very easy to make sure your components are properly released and eligible for garbage collection. But what about possible dependencies of your components? Let's take a look. Suppose we define the following interface and implementation:

<script src="https://gist.github.com/3684127.js?file=s5.cs"></script>

The dependency doesn't actually do anything, but bear with me :)

We modify the IController interface and Controller implementation like this:

<script src="https://gist.github.com/3684127.js?file=s6.cs"></script>

And then we modify the configuration of the container like this:

<script src="https://gist.github.com/3684127.js?file=s7.cs"></script>

Whenever we request an IController instance, the container will construct a Controller instance and will pass a MyDependency instance to the Controller's instance constructor. The question now is: does the container also track the instances of a requested component's dependencies? The answer is: no

<script src="https://gist.github.com/3684127.js?file=s8.cs"></script>

We request an IController instance, which is tracked by the container. The IController's Dependency property contains an IDependency instance, which was also created by the container. But as the last line of the test shows: the container does not track instances of the requested IController's dependencies. So what does that mean? If the dependencies don't require any cleanup, then this is great. We simply need to release the requested component, and the component and its dependencies will all be eligible for Garbage Collection.

(Note 27/01/10: the following part is no longer relevant as of Castle Windsor 2.1)

But what happens when the dependencies need to be disposed? Let's take another look. We modify the IDependency interface and MyDependency class so it looks like this:

<script src="https://gist.github.com/3684127.js?file=s9.cs"></script>

Now let's see what happens with the Controller's dependencies when we release the Controller:

<script src="https://gist.github.com/3684127.js?file=s10.cs"></script>

The container holds no references, but the Controller's Dependency instance is not disposed! Notice however that the Controller has been disposed by the container. As I've mentioned <a href="/blog/2008/08/net-memory-management/">earlier</a>, if you own a reference to an IDisposable instance, you are responsible for properly disposing of that instance. So we modify the Controller's Dispose method so that it looks like this:

<script src="https://gist.github.com/3684171.js?file=s1.cs"></script>

The previous test will now fail, because the Dependency will be properly disposed.

NOTE: I certainly don't recommend to implement your Dispose methods like I just did. This is just a simplified example. The proper way to implement the Disposable Pattern is discussed <a href="/blog/2008/06/disposing-of-the-idisposable-implementation/">here</a>.

Anyways, I hope it's clear now how you can make sure your IoC usage does not cause memory leaks, and that everything is properly disposed of.