In my previous <a href="http://davybrion.com/blog/2008/04/introduction-to-ioc-with-windsor/">Windsor post</a> i showed you how you can use the Windsor container to manage your components and their dependencies.  Since it was merely an introductory post on Windsor, i only showed how you can use it to handle dependencies. But there's a lot more you can do with it, and that you should know about.

One thing you'll definitely need to know to properly use Windsor, is that of component instance lifetimes. After all, you want the container to manage your components and their dependencies. But there's more to the management of components than merely filling in dependencies. Should the container return a new instance of a component? Should it return an already existing instance? How do you control that behavior without having clients know about it? After all, should clients of components really know about that? Is that not an implementation detail that might be better of being properly encapsulated from clients?

Windsor allows you to register components with specific lifestyles. These are the lifestyles you can use:

<ul>
	<li>Singleton: components are instantiated once, and shared between all clients</li>
	<li>Transient: components are created on demand</li>
	<li>PerWebRequest: components are created once per Http Request</li>
	<li>Thread: components have a unique instance per thread</li>
	<li>Pooled: Optimization of transient components that keeps instance in a pool instead of always creating them</li>
	<li>Custom: allows you to specify a custom lifestyle... you'd have to specify a type that implements the ILifeStyleManager interface</li>
</ul>

The Singleton lifestyle is actually the default. I'm not so happy with that being the default, but oh well... If we continue with our previous sample, we can verify that Singleton is indeed the default lifestyle for a registered component.  Suppose the component is registered like this:

<div style="font-family:Consolas;font-size:10pt;color:black;background:white;">
<p style="margin:0;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; _container.AddComponent&lt;<span style="color:#2b91af;">IOrderRepository</span>, <span style="color:#2b91af;">OrderRepository</span>&gt;();</p>
</div>

Then the following test would pass:

<div style="font-family:Consolas;font-size:10pt;color:black;background:white;">
<p style="margin:0;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span style="color:blue;">var</span> r1 = <span style="color:#2b91af;">Container</span>.Resolve&lt;<span style="color:#2b91af;">IOrderRepository</span>&gt;();</p>
<p style="margin:0;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span style="color:blue;">var</span> r2 = <span style="color:#2b91af;">Container</span>.Resolve&lt;<span style="color:#2b91af;">IOrderRepository</span>&gt;();</p>
<p style="margin:0;">&nbsp;</p>
<p style="margin:0;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span style="color:#2b91af;">Assert</span>.That(ReferenceEquals(r1, r2));</p>
</div>

But if we change the registration to this:

<div style="font-family:Consolas;font-size:10pt;color:black;background:white;">
<p style="margin:0;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; _container.AddComponentWithLifestyle&lt;<span style="color:#2b91af;">IOrderRepository</span>, <span style="color:#2b91af;">OrderRepository</span>&gt;(<span style="color:#2b91af;">LifestyleType</span>.Transient);</p>
</div>

Then the test obviously fails because both requests to get an IOrderRepository instance will create a new OrderRepository instance.

You might be wondering what happens when you define a component as a singleton, but one of its dependencies is transient:

<div style="font-family:Consolas;font-size:10pt;color:black;background:white;">
<p style="margin:0;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; _container.AddComponentWithLifestyle&lt;<span style="color:#2b91af;">IOrderDataAccessor</span>, <span style="color:#2b91af;">OrderDataAccessor</span>&gt;(<span style="color:#2b91af;">LifestyleType</span>.Transient);</p>
<p style="margin:0;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; _container.AddComponentWithLifestyle&lt;<span style="color:#2b91af;">IOrderRepository</span>, <span style="color:#2b91af;">OrderRepository</span>&gt;(<span style="color:#2b91af;">LifestyleType</span>.Singleton);</p>
</div>

The answer is pretty straightforward: when you request an instance of type IOrderRepository the first time, it will create a new IOrderAccessor instance as well and pass it to the OrderRepository constructor. The second time you request an instance of type IOrderRepository, the container already has the singleton instance cached, so a new IOrderAccessor instance is not created.

If you really want this behavior (a new IOrderDataAccessor instance whenever the singleton IOrderRepository is requested) you can get it working pretty easily. Right now, our OrderRepository implementation uses Constructor Injection:

<div style="font-family:Consolas;font-size:10pt;color:black;background:white;">
<p style="margin:0;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span style="color:blue;">private</span> <span style="color:#2b91af;">IOrderDataAccessor</span> _accessor;</p>
<p style="margin:0;">&nbsp;</p>
<p style="margin:0;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span style="color:blue;">public</span> OrderRepository(<span style="color:#2b91af;">IOrderDataAccessor</span> accessor)</p>
<p style="margin:0;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; {</p>
<p style="margin:0;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; _accessor = accessor;</p>
<p style="margin:0;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; }</p>
</div>

If we add Setter Injection as well the container will use the setter injector when we request the IOrderRepository instance after it has already been created:

<div style="font-family:Consolas;font-size:10pt;color:black;background:white;">
<p style="margin:0;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span style="color:blue;">public</span> <span style="color:#2b91af;">IOrderDataAccessor</span> DataAccessor</p>
<p style="margin:0;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; {</p>
<p style="margin:0;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span style="color:blue;">set</span> { _accessor = <span style="color:blue;">value</span>; }</p>
<p style="margin:0;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; }</p>
</div>

Because we have both Constructor Injection and Setter Injection, the container will supply a new IOrderDataAccessor when the OrderRepository instance is created. And when it is requested again after its creation, the container will supply a new IOrderDataAccessor instance to the OrderRepository using the setter of the dependency.

It's nice to know that this is possible, but i wouldn't recommend this approach... It's very confusing and would certainly cause problems in multi-threaded scenarios.  You're better off injecting an IOrderDataAccessorFactory object when the repository is created, and then let the repository request a new IOrderDataAccessor instance to be used locally whenever it's needed (as in: as a local variable during method execution, but certainly not as a field of the class).

There's also the other way around of course... suppose the dependency is configured as a singleton, and the component to be used is configured to have the Transient lifestyle.  The singleton dependency will only be created once, and every time you request a transient component that is dependent on a singleton component, the container injects the singleton instance in the transient component.

By now, I hope you realize that an Inversion Of Control Container is about more than merely Dependency Injection and increasing testability.  There's most certainly a lot more to it than that :). I have a few more posts coming up about how using an IoC container can make your life as a developer easier.
