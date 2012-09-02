As you may or may not know, i'm a bit of a fan of <a href="http://davybrion.com/blog/2007/07/introduction-to-dependency-injection/">dependency injection</a>.  If you're only using it on a small scale, you don't really need any tools to use the technique.  But once you're used to this design technique, you'll quickly start using it in many places of your code. If you do, it quickly becomes cumbersome to deal with the real instances of your runtime dependencies manually. This is where tools like Inversion Of Control (IoC) containers come in to play. There are a few solid containers available for the .NET world, and even Microsoft has released their <a href="http://www.codeplex.com/unity">own container</a>.  Basically, what the IoC container does for you, is take care of providing dependencies to components in a flexible and customizable way. It allows clients to remain completely oblivious to the dependencies of components they use.  This makes it easy to change components without having to modify client code. Not to mention the fact that your components are a lot easier to test, since you can simply inject fake dependencies during your tests.

How about some code to demonstrate? Suppose we have a class called OrderRepository which exposes methods such as GetById, GetAll, FindOne, FindMany and Store. Obviously, the OrderRepository has a dependency on a class that can actually communicate with some kind of physical datastore, either a database or an xml file or whatever.  Either way, it needs another object to access the Order data. Suppose we have an OrderAccessor class which implements an IOrderAccessor interface.  The interface declares all the methods we need to retrieve or store our Orders.  So our OrderRepository would need to communicate with an object that implements the IOrderAccessor interface.  Instead of letting the OrderRepository instantiate that object itself, it will receive it as a parameter in it's constructor:

<div style="font-family:Consolas;font-size:10pt;color:black;background:white;">
<p style="margin:0;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span style="color:blue;">private</span> <span style="color:blue;">readonly</span> <span style="color:#2b91af;">IOrderDataAccessor</span> _accessor;</p>
<p style="margin:0;">&nbsp;</p>
<p style="margin:0;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span style="color:blue;">public</span> OrderRepository(<span style="color:#2b91af;">IOrderDataAccessor</span> accessor)</p>
<p style="margin:0;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; {</p>
<p style="margin:0;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; _accessor = accessor;</p>
<p style="margin:0;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; }</p>
</div>

This makes it easy to test the OrderRepository class, and it's also easy to make it use different implementations of IOrderDataAccessor later on, should we need to.  Now obviously, you really don't want to do this when you need to instantiate the OrderRepository in your production code:

<div style="font-family:Consolas;font-size:10pt;color:black;background:white;">
<p style="margin:0;"><span style="color:#2b91af;">OrderRepository</span> repo = <span style="color:blue;">new</span> <span style="color:#2b91af;">OrderRepository</span>(<span style="color:blue;">new</span> <span style="color:#2b91af;">OrderDataAccessor</span>());</p>
</div>

As a consumer of the OrderRepository, you shouldn't need to know what its dependencies are and you most certainly shouldn't need to pass the right dependencies into the constructor.  Instead, you just want a valid instance of OrderRepository. You really don't care how it was constructed, which dependencies it has and how they're provided.  You just need to be able to use it. That's all.  This is where the IoC container comes in to help you.  Suppose we wrap the IoC container in a Container class that has a few static methods to help you with instantiating instances of types.  We could then do this:

<div style="font-family:Consolas;font-size:10pt;color:black;background:white;">
<p style="margin:0;"><span style="color:#2b91af;">OrderRepository</span> repository = <span style="color:#2b91af;">Container</span>.Resolve&lt;<span style="color:#2b91af;">OrderRepository</span>&gt;();</p>
</div>

That would leave you with a valid OrderRepository instance... one that has a usable IOrderDataAccessor but you don't even know about it, nor do you care how it got there. In other words, you can use the OrderRepository without knowing anything about its underlying implementation.

Let's take a look at the implementation of the Container class:

<div style="font-family:Consolas;font-size:10pt;color:black;background:white;">
<p style="margin:0;">&nbsp;&nbsp;&nbsp; <span style="color:blue;">public</span> <span style="color:blue;">static</span> <span style="color:blue;">class</span> <span style="color:#2b91af;">Container</span></p>
<p style="margin:0;">&nbsp;&nbsp;&nbsp; {</p>
<p style="margin:0;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span style="color:blue;">private</span> <span style="color:blue;">static</span> <span style="color:blue;">readonly</span> <span style="color:#2b91af;">IWindsorContainer</span> _container;</p>
<p style="margin:0;">&nbsp;</p>
<p style="margin:0;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span style="color:blue;">static</span> Container()</p>
<p style="margin:0;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; {</p>
<p style="margin:0;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; _container = <span style="color:blue;">new</span> <span style="color:#2b91af;">WindsorContainer</span>();</p>
<p style="margin:0;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; _container.AddComponent&lt;<span style="color:#2b91af;">IOrderDataAccessor</span>, <span style="color:#2b91af;">OrderDataAccessor</span>&gt;();</p>
<p style="margin:0;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; _container.AddComponent&lt;<span style="color:#2b91af;">OrderRepository</span>&gt;();</p>
<p style="margin:0;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; }</p>
<p style="margin:0;">&nbsp;</p>
<p style="margin:0;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span style="color:blue;">public</span> <span style="color:blue;">static</span> T Resolve&lt;T&gt;()</p>
<p style="margin:0;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; {</p>
<p style="margin:0;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span style="color:blue;">return</span> _container.Resolve&lt;T&gt;();</p>
<p style="margin:0;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; }</p>
<p style="margin:0;">&nbsp;&nbsp;&nbsp; }</p>
</div>

It just uses a static instance of Windor's Container and it registers the types we need... let's examine the following line:
<div style="font-family:Consolas;font-size:10pt;color:black;background:white;">
<p style="margin:0;">_container.AddComponent&lt;<span style="color:#2b91af;">IOrderDataAccessor</span>, <span style="color:#2b91af;">OrderDataAccessor</span>&gt;();</p>
</div>

this basically sets up the container to return a new instance of OrderDataAccessor whenever an instance of IOrderDataAcessor is requested.

We still have to make sure the Windsor container knows about the OrderRepository class by adding it as a known component like this:

<div style="font-family:Consolas;font-size:10pt;color:black;background:white;">
<p style="margin:0;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; _container.AddComponent&lt;<span style="color:#2b91af;">OrderRepository</span>&gt;();</p>
</div>

By doing this, the Windsor container will inspect the type (in this case, OrderRepository) and it will see that its constructor requires an IOrderDataAccessor instance. We 'registered' the IOrderDataAccessor type with the container to return an instance of the OrderDataAccessor type. So basically, whenever someone asks the container to return an instance of an OrderRepository class, the container knows to instantiate an OrderDataAccessor instance to pass along as the required IOrderDataAccessor object to the OrderRepository constructor.

At this point, you may be wondering: "Why go through all this trouble to register the concrete implementation of IOrderDataAccessor to be used in code? We could just as well instantiate the type ourselves!".  That's certainly true.  The code would be slightly uglier, but you'd get the same behavior.  Of course, the Windsor container supports XML configuration (either in the app.config or web.config or in a custom configuration file) as well as explicit configuration through code. So you can configure the container through code explicitly, but if there is a config file present, the container will use that configuration instead of the one provided through code.  So you could define the defaults in code, and should you need to change it later on, you can just provide a config file.

You know what bothers me about our current implementation? We're still communicating with an OrderRepository instance. If we wanna be really flexible, it would be better if we were communicating with an object that implemented an IOrderRepository interface.  So let's just define the following interface:

<div style="font-family:Consolas;font-size:10pt;color:black;background:white;">
<p style="margin:0;">&nbsp;&nbsp;&nbsp; <span style="color:blue;">public</span> <span style="color:blue;">interface</span> <span style="color:#2b91af;">IOrderRepository</span></p>
<p style="margin:0;">&nbsp;&nbsp;&nbsp; {</p>
<p style="margin:0;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span style="color:#2b91af;">Order</span> GetById(<span style="color:#2b91af;">Guid</span> id);</p>
<p style="margin:0;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span style="color:#2b91af;">IEnumerable</span>&lt;<span style="color:#2b91af;">Order</span>&gt; GetAll();</p>
<p style="margin:0;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span style="color:#2b91af;">Order</span> FindOne(<span style="color:#2b91af;">Criteria</span> criteria);</p>
<p style="margin:0;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span style="color:#2b91af;">IEnumerable</span>&lt;<span style="color:#2b91af;">Order</span>&gt; FindMany(<span style="color:#2b91af;">Criteria</span> criteria);</p>
<p style="margin:0;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span style="color:blue;">void</span> Store(<span style="color:#2b91af;">Order</span> order);</p>
<p style="margin:0;">&nbsp;&nbsp;&nbsp; }</p>
</div>

After all, that's all we care about as consumers of a IOrderRepository type. We shouldn't really care about the concrete implementation.  We just need an interface to program to.  So let's change the OrderRepository definition to this:

<div style="font-family:Consolas;font-size:10pt;color:black;background:white;">
<p style="margin:0;">&nbsp;&nbsp;&nbsp; <span style="color:blue;">public</span> <span style="color:blue;">class</span> <span style="color:#2b91af;">OrderRepository</span> : <span style="color:#2b91af;">IOrderRepository</span></p>
</div>

And then when we configure our IoC container we do it like this:

<div style="font-family:Consolas;font-size:10pt;color:black;background:white;">
<p style="margin:0;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span style="color:blue;">static</span> Container()</p>
<p style="margin:0;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; {</p>
<p style="margin:0;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; _container = <span style="color:blue;">new</span> <span style="color:#2b91af;">WindsorContainer</span>();</p>
<p style="margin:0;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; _container.AddComponent&lt;<span style="color:#2b91af;">IOrderDataAccessor</span>, <span style="color:#2b91af;">OrderDataAccessor</span>&gt;();</p>
<p style="margin:0;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; _container.AddComponent&lt;<span style="color:#2b91af;">IOrderRepository</span>, <span style="color:#2b91af;">OrderRepository</span>&gt;();</p>
<p style="margin:0;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; }</p>
</div>

Now we can no longer ask the contianer for an OrderRepository interface. But we can ask for an instance that implements the IOrderRepository interface like this:

<div style="font-family:Consolas;font-size:10pt;color:black;background:white;">
<p style="margin:0;"><span style="color:#2b91af;">IOrderRepository</span> repository = <span style="color:#2b91af;">Container</span>.Resolve&lt;<span style="color:#2b91af;">IOrderRepository</span>&gt;();</p>
</div>

So now our client is completely decoupled from the implementation of IOrderRepository, as well as the dependencies it may or may not have.

Ok, lets suppose that this implementation makes it to the production environment.  Everything's working but for some reason, someone makes a decision to retrieve the orders from a specially prepared XML file instead of the database.  Unfortunately, your OrderDataAccessor class communicates with a SQL server database. Luckily, the OrderRepository implementation doesn't know which specific implementation of IOrderDataAccessor it's using.  We just need to make sure that every time someone needs an IOrderRepository instance, it uses the new xml-based IOrderDataAccessor implementation instead of the one we originally intended.

Because we're using Dependency Injection and an IoC container, this only requires changing one line of code:

<div style="font-family:Consolas;font-size:10pt;color:black;background:white;">
<p style="margin:0;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; _container.AddComponent&lt;<span style="color:#2b91af;">IOrderDataAccessor</span>, <span style="color:#2b91af;">XmlOrderDataAccessor</span>&gt;();</p>
</div>

Actually, if we'd put the mapping between the IOrderDataAccessor type and the XmlOrderDataAccessor implementation in an xml file, we wouldn't even have to change any code! Well, except for the XmlOrderDataAccessor implementation obviously.

We can even take this one step further... After the change to the xml-based OrderDataAccessor went successfully, they (the 'business') all of a sudden want to log who retrieves or saves each order for auditing purposes.

Hmmm, alright then... We create an implementation of IOrderRepository which keeps extensive auditing logs so they can be retrieved later on. We could just inherit from the default OrderRepository implementation and add auditing logic before each method is executed.  Then we'd only have to configure our IoC container to return a different instance of the IOrderRepository type whenever someone requests it:

<div style="font-family:Consolas;font-size:10pt;color:black;background:white;">
<p style="margin:0;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span style="color:blue;">static</span> Container()</p>
<p style="margin:0;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; {</p>
<p style="margin:0;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; _container = <span style="color:blue;">new</span> <span style="color:#2b91af;">WindsorContainer</span>();</p>
<p style="margin:0;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; _container.AddComponent&lt;<span style="color:#2b91af;">IOrderDataAccessor</span>, <span style="color:#2b91af;">XmlOrderDataAccessor</span>&gt;();</p>
<p style="margin:0;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; _container.AddComponent&lt;<span style="color:#2b91af;">IOrderRepository</span>, <span style="color:#2b91af;">OrderRepositoryWithAuditing</span>&gt;();</p>
<p style="margin:0;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; }</p>
</div>

Again, our client code does not need to be modified in any way, yet we did modify the runtime behavior of the application.  Instead of retrieving the Orders from a SQL database, it's now retrieving them from an XML file, and the repository is performing auditing as well, without having to change any client code.

And if we were using the xml-configuration features of Windsor, we could get all of this working without even having to recompile the client-assemblies.

This was just an introduction to using an IoC contianer (Castle's Windsor specifically) and we briefly touched on benefits that you can achieve with this way of working.  The Windsor container can do much more, but you'll either have to figure that stuff out yourself, or wait for future posts about its other features/possibilities :)
