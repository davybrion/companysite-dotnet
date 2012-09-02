Some classes need configuration data to function properly. This configuration data could be a database connection string, a path, a hostname, a network port, whatever. You typically deal with this by putting the configuration data in your app.config or web.config... either through a Settings file or in the AppSettings or maybe you've created your own configuration section or whatever.  And in most cases, when a class needs this data, it simply retrieves it from the Configuration class or the class that was created through your Settings class.

By doing this, you actually create a strong dependency between your class, and the object that provides the configuration data. But you're not really dependent on the object providing the data, since you really only need a bit of data to function.  So why not treat the data itself as a dependency?

Let's use our <a href="http://davybrion.com/blog/2008/04/introduction-to-ioc-with-windsor/">previous example</a>.  The OrderDataAccessor class will retrieve Orders from a database. In order to do that, it needs a connection string.  Instead of letting the OrderDataAccessor class retrieve that connection string from a config file itself, we'll modify the constructor so that each instance retrieves the connection string when it is created:

<div style="font-family:Consolas;font-size:10pt;color:black;background:white;">
<p style="margin:0;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span style="color:blue;">private</span> <span style="color:blue;">readonly</span> <span style="color:blue;">string</span> _connectionString;</p>
<p style="margin:0;">&nbsp;</p>
<p style="margin:0;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span style="color:blue;">public</span> OrderDataAccessor(<span style="color:blue;">string</span> connectionString)</p>
<p style="margin:0;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; {</p>
<p style="margin:0;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; _connectionString = connectionString;</p>
<p style="margin:0;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; }</p>
</div>

If the container now needs to create an OrderDataAccessor instance, we get the following exception:

<pre>
Castle.MicroKernel.Resolvers.DependencyResolverException : Could not resolve non-optional
dependency for 'Components.OrderDataAccessor' (Components.OrderDataAccessor).
Parameter 'connectionString' type 'System.String'
</pre>

Which makes sense, since we haven't told the container about this 'dependency' yet. Since we're dealing with configuration data now, it's probably better to move our Windsor configuration to a config file as well.  First we'll define the castle configuration section in our app.config:

<div style="font-family:Consolas;font-size:10pt;color:black;background:white;">
<p style="margin:0;"><span style="color:blue;">&nbsp; &lt;</span><span style="color:#a31515;">configSections</span><span style="color:blue;">&gt;</span></p>
<p style="margin:0;"><span style="color:blue;">&nbsp; &nbsp; &lt;</span><span style="color:#a31515;">section</span><span style="color:blue;"> </span><span style="color:red;">name</span><span style="color:blue;">=</span>"<span style="color:blue;">castle</span>"<span style="color:blue;"> </span><span style="color:red;">type</span><span style="color:blue;">=</span>"<span style="color:blue;">Castle.Windsor.Configuration.AppDomain.CastleSectionHandler, Castle.Windsor</span>"<span style="color:blue;"> /&gt;</span></p>
<p style="margin:0;"><span style="color:blue;">&nbsp; &lt;/</span><span style="color:#a31515;">configSections</span><span style="color:blue;">&gt;</span></p>
</div>

Then we configure our components:

<div style="font-family:Consolas;font-size:10pt;color:black;background:white;">
<p style="margin:0;"><span style="color:blue;">&nbsp; &lt;</span><span style="color:#a31515;">castle</span><span style="color:blue;">&gt;</span></p>
<p style="margin:0;"><span style="color:blue;">&nbsp; &nbsp; &lt;</span><span style="color:#a31515;">components</span><span style="color:blue;">&gt;</span></p>
<p style="margin:0;"><span style="color:blue;">&nbsp; &nbsp; &nbsp; &lt;</span><span style="color:#a31515;">component</span><span style="color:blue;"> </span><span style="color:red;">id</span><span style="color:blue;">=</span>"<span style="color:blue;">IOrderDataAccessor</span>"</p>
<p style="margin:0;"><span style="color:blue;">&nbsp; &nbsp; &nbsp; &nbsp; &nbsp; </span><span style="color:red;">service</span><span style="color:blue;">=</span>"<span style="color:blue;">Components.IOrderDataAccessor, Components</span>"</p>
<p style="margin:0;"><span style="color:blue;">&nbsp; &nbsp; &nbsp; &nbsp; &nbsp; </span><span style="color:red;">type</span><span style="color:blue;">=</span>"<span style="color:blue;">Components.OrderDataAccessor, Components</span>"<span style="color:blue;">&gt;</span></p>
<p style="margin:0;">&nbsp;</p>
<p style="margin:0;"><span style="color:blue;">&nbsp; &nbsp; &nbsp; &nbsp; &lt;</span><span style="color:#a31515;">parameters</span><span style="color:blue;">&gt;</span></p>
<p style="margin:0;"><span style="color:blue;">&nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &lt;</span><span style="color:#a31515;">connectionString</span><span style="color:blue;">&gt;</span>myConnectionString<span style="color:blue;">&lt;/</span><span style="color:#a31515;">connectionString</span><span style="color:blue;">&gt;</span></p>
<p style="margin:0;"><span style="color:blue;">&nbsp; &nbsp; &nbsp; &nbsp; &lt;/</span><span style="color:#a31515;">parameters</span><span style="color:blue;">&gt;</span></p>
<p style="margin:0;">&nbsp;</p>
<p style="margin:0;"><span style="color:blue;">&nbsp; &nbsp; &nbsp; &lt;/</span><span style="color:#a31515;">component</span><span style="color:blue;">&gt;</span></p>
<p style="margin:0;">&nbsp;</p>
<p style="margin:0;"><span style="color:blue;">&nbsp; &nbsp; &nbsp; &lt;</span><span style="color:#a31515;">component</span><span style="color:blue;"> </span><span style="color:red;">id</span><span style="color:blue;">=</span>"<span style="color:blue;">IOrderRepository</span>"</p>
<p style="margin:0;"><span style="color:blue;">&nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp;  </span><span style="color:red;">service</span><span style="color:blue;">=</span>"<span style="color:blue;">Components.IOrderRepository, Components</span>"</p>
<p style="margin:0;"><span style="color:blue;">&nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp;  </span><span style="color:red;">type</span><span style="color:blue;">=</span>"<span style="color:blue;">Components.OrderRepository, Components</span>"<span style="color:blue;"> /&gt;</span></p>
<p style="margin:0;">&nbsp;</p>
<p style="margin:0;"><span style="color:blue;">&nbsp; &nbsp; &lt;/</span><span style="color:#a31515;">components</span><span style="color:blue;">&gt;</span></p>
<p style="margin:0;">&nbsp;</p>
<p style="margin:0;"><span style="color:blue;">&nbsp; &lt;/</span><span style="color:#a31515;">castle</span><span style="color:blue;">&gt;</span></p>
</div>

And that's it... Whenever the container instantiates an OrderDataAccessor instance, it will pass 'myConnectionString' to the connectionString parameter.

There's one issue with this though... In a real system, you'd have more than one DataAccessor class, and having to specify the connectionString for each one of them would be a prime example of suckage. So let's modify our config file a little bit:

<div style="font-family:Consolas;font-size:10pt;color:black;background:white;">
<p style="margin:0;"><span style="color:blue;">&nbsp; &lt;</span><span style="color:#a31515;">castle</span><span style="color:blue;">&gt;</span></p>
<p style="margin:0;">&nbsp;</p>
<p style="margin:0;"><span style="color:blue;">&nbsp; &nbsp; &lt;</span><span style="color:#a31515;">properties</span><span style="color:blue;">&gt;</span></p>
<p style="margin:0;"><span style="color:blue;">&nbsp; &nbsp; &nbsp; &lt;</span><span style="color:#a31515;">connectionString</span><span style="color:blue;">&gt;</span>myConnectionString<span style="color:blue;">&lt;/</span><span style="color:#a31515;">connectionString</span><span style="color:blue;">&gt;</span></p>
<p style="margin:0;"><span style="color:blue;">&nbsp; &nbsp; &lt;/</span><span style="color:#a31515;">properties</span><span style="color:blue;">&gt;</span></p>
<p style="margin:0;">&nbsp;</p>
<p style="margin:0;"><span style="color:blue;">&nbsp; &nbsp; &lt;</span><span style="color:#a31515;">components</span><span style="color:blue;">&gt;</span></p>
<p style="margin:0;"><span style="color:blue;">&nbsp; &nbsp; &nbsp; &lt;</span><span style="color:#a31515;">component</span><span style="color:blue;"> </span><span style="color:red;">id</span><span style="color:blue;">=</span>"<span style="color:blue;">IOrderDataAccessor</span>"</p>
<p style="margin:0;"><span style="color:blue;">&nbsp; &nbsp; &nbsp; &nbsp; &nbsp; </span><span style="color:red;">service</span><span style="color:blue;">=</span>"<span style="color:blue;">Components.IOrderDataAccessor, Components</span>"</p>
<p style="margin:0;"><span style="color:blue;">&nbsp; &nbsp; &nbsp; &nbsp; &nbsp; </span><span style="color:red;">type</span><span style="color:blue;">=</span>"<span style="color:blue;">Components.OrderDataAccessor, Components</span>"<span style="color:blue;">&gt;</span></p>
<p style="margin:0;">&nbsp;</p>
<p style="margin:0;"><span style="color:blue;">&nbsp; &nbsp; &nbsp; &nbsp; &lt;</span><span style="color:#a31515;">parameters</span><span style="color:blue;">&gt;</span></p>
<p style="margin:0;"><span style="color:blue;">&nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &lt;</span><span style="color:#a31515;">connectionString</span><span style="color:blue;">&gt;</span>#{connectionString}<span style="color:blue;">&lt;/</span><span style="color:#a31515;">connectionString</span><span style="color:blue;">&gt;</span></p>
<p style="margin:0;"><span style="color:blue;">&nbsp; &nbsp; &nbsp; &nbsp; &lt;/</span><span style="color:#a31515;">parameters</span><span style="color:blue;">&gt;</span></p>
<p style="margin:0;">&nbsp;</p>
<p style="margin:0;"><span style="color:blue;">&nbsp; &nbsp; &nbsp; &lt;/</span><span style="color:#a31515;">component</span><span style="color:blue;">&gt;</span></p>
<p style="margin:0;">&nbsp;</p>
<p style="margin:0;"><span style="color:blue;">&nbsp; &nbsp; &nbsp; &lt;</span><span style="color:#a31515;">component</span><span style="color:blue;"> </span><span style="color:red;">id</span><span style="color:blue;">=</span>"<span style="color:blue;">IOrderRepository</span>"</p>
<p style="margin:0;"><span style="color:blue;">&nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp;  </span><span style="color:red;">service</span><span style="color:blue;">=</span>"<span style="color:blue;">Components.IOrderRepository, Components</span>"</p>
<p style="margin:0;"><span style="color:blue;">&nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp;  </span><span style="color:red;">type</span><span style="color:blue;">=</span>"<span style="color:blue;">Components.OrderRepository, Components</span>"<span style="color:blue;"> /&gt;</span></p>
<p style="margin:0;">&nbsp;</p>
<p style="margin:0;"><span style="color:blue;">&nbsp; &nbsp; &lt;/</span><span style="color:#a31515;">components</span><span style="color:blue;">&gt;</span></p>
<p style="margin:0;">&nbsp;</p>
<p style="margin:0;"><span style="color:blue;">&nbsp; &lt;/</span><span style="color:#a31515;">castle</span><span style="color:blue;">&gt;</span></p>
</div>

That's better... Now we can just refer to the connectionString whenever we need it so we'd only have to modify it in one place.

Keep in mind that if you put the Windsor configuration in your app.config/web.config file, you need to instantiate the container like this:

<div style="font-family:Consolas;font-size:10pt;color:black;background:white;">
<p style="margin:0;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; _container = <span style="color:blue;">new</span> <span style="color:#2b91af;">WindsorContainer</span>(<span style="color:blue;">new</span> <span style="color:#2b91af;">XmlInterpreter</span>());</p>
</div>

So as you can see, you can also use the IoC container to keep dependencies on configuration-providing-classes completely out of your code by 'promoting' the required configuration data to actual dependencies of your components.
