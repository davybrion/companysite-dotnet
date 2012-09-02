The Windsor container makes it quite easy to add behavior to components, without having to modify their implementation. This could be useful in many scenario's. Suppose you need to log whenever a method from our OrderRepository class is called.  But we should be able to turn the logging on and off whenever we want.  Preferably, without having to modify the code all the time. Now, you could easily write a logger class that checks for a configuration setting and only logs when needed. This approach would definitely work. But then there's logging code all over the OrderRepository class and in most cases, it's not even necessary since they only want to be able to log under certain circumstances. Should the OrderRepository class really care about the logging? Why litter the code with logging statements?

If you're using the Windsor container, you could easily add logging behavior to the OrderRepository class without having to change any of the existing code. Windsor has this concept of Interceptors. Basically you can assign an interceptor to any component and you can plug in your custom behavior when the component is called. Lets get into an example... Since logging is such a common requirement, we decided to put it in one class instead of littering our entire code base with logging statements. So we wrote the following class:

<div style="font-family:Consolas;font-size:10pt;color:black;background:white;">
<p style="margin:0;">&nbsp;&nbsp;&nbsp; <span style="color:blue;">public</span> <span style="color:blue;">class</span> <span style="color:#2b91af;">LoggingInterceptor</span> : Castle.Core.Interceptor.<span style="color:#2b91af;">IInterceptor</span></p>
<p style="margin:0;">&nbsp;&nbsp;&nbsp; {</p>
<p style="margin:0;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span style="color:blue;">private</span> <span style="color:blue;">readonly</span> <span style="color:#2b91af;">ILogger</span> logger;</p>
<p style="margin:0;">&nbsp;</p>
<p style="margin:0;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span style="color:blue;">public</span> LoggingInterceptor(<span style="color:#2b91af;">ILogger</span> logger)</p>
<p style="margin:0;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; {</p>
<p style="margin:0;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span style="color:blue;">this</span>.logger = logger;</p>
<p style="margin:0;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; }</p>
<p style="margin:0;">&nbsp;</p>
<p style="margin:0;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span style="color:blue;">public</span> <span style="color:blue;">void</span> Intercept(<span style="color:#2b91af;">IInvocation</span> invocation)</p>
<p style="margin:0;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; {</p>
<p style="margin:0;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span style="color:blue;">string</span> methodName = </p>
<p style="margin:0;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; invocation.TargetType.FullName + <span style="color:#a31515;">"."</span> + invocation.GetConcreteMethod().Name;</p>
<p style="margin:0;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; Log(<span style="color:#a31515;">"Entering method: "</span> + methodName);</p>
<p style="margin:0;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; invocation.Proceed();</p>
<p style="margin:0;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; Log(<span style="color:#a31515;">"Leaving mehod: "</span> + methodName);</p>
<p style="margin:0;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; }</p>
<p style="margin:0;">&nbsp;</p>
<p style="margin:0;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span style="color:blue;">private</span> <span style="color:blue;">void</span> Log(<span style="color:blue;">string</span> line)</p>
<p style="margin:0;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; {</p>
<p style="margin:0;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; logger.WriteLine(<span style="color:#2b91af;">DateTime</span>.Now.TimeOfDay + <span style="color:#a31515;">" "</span> + line);</p>
<p style="margin:0;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; }</p>




<p style="margin:0;">&nbsp;&nbsp;&nbsp; }</p>
</div>

This class implements the IInterceptor interface by implementing the Intercept method. When that method is called we simply construct the full method name, log when we enter the method, call the original method and then we log again when we leave the method.  Nothing more, nothing less. Also notice how the LoggingInterceptor has a dependency on an ILogger instance. That instance will be injected by the container as well.

So first of all, we need to define the ILogger and LoggingInterceptor components:

<div style="font-family:Consolas;font-size:10pt;color:black;background:white;">
<p style="margin:0;"><span style="color:blue;">&nbsp; &nbsp; &nbsp; &lt;</span><span style="color:#a31515;">component</span><span style="color:blue;"> </span><span style="color:red;">id</span><span style="color:blue;">=</span>"<span style="color:blue;">ILogger</span>"<span style="color:blue;"> </span><span style="color:red;">service</span><span style="color:blue;">=</span>"<span style="color:blue;">Components.ILogger, Components</span>"<span style="color:blue;"> </span></p>
<p style="margin:0;"><span style="color:blue;">&nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp;  </span><span style="color:red;">type</span><span style="color:blue;">=</span>"<span style="color:blue;">Components.Logger, Components</span>"<span style="color:blue;"> /&gt;</span></p>
<p style="margin:0;">&nbsp;</p>
<p style="margin:0;"><span style="color:blue;">&nbsp; &nbsp; &nbsp; &lt;</span><span style="color:#a31515;">component</span><span style="color:blue;"> </span><span style="color:red;">id</span><span style="color:blue;">=</span>"<span style="color:blue;">LoggingInterceptor</span>"<span style="color:blue;"> </span><span style="color:red;">service</span><span style="color:blue;">=</span>"<span style="color:blue;">Components.LoggingInterceptor, Components</span>"</p>
<p style="margin:0;"><span style="color:blue;">&nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp;  </span><span style="color:red;">type</span><span style="color:blue;">=</span>"<span style="color:blue;">Components.LoggingInterceptor, Components</span>"</p>
<p style="margin:0;"><span style="color:blue;">&nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp;  </span><span style="color:red;">lifestyle</span><span style="color:blue;">=</span>"<span style="color:blue;">transient</span>"<span style="color:blue;"> /&gt;</span></p>
</div>

Right... so now we need to add this behavior to the OrderRepository class. This only requires modifying the registration of the IOrderRepository component:

<div style="font-family:Consolas;font-size:10pt;color:black;background:white;">
<p style="margin:0;"><span style="color:blue;">&nbsp; &nbsp; &nbsp; &lt;</span><span style="color:#a31515;">component</span><span style="color:blue;"> </span><span style="color:red;">id</span><span style="color:blue;">=</span>"<span style="color:blue;">IOrderRepository</span>"</p>
<p style="margin:0;"><span style="color:blue;">&nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp;  </span><span style="color:red;">service</span><span style="color:blue;">=</span>"<span style="color:blue;">Components.IOrderRepository, Components</span>"</p>
<p style="margin:0;"><span style="color:blue;">&nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp;  </span><span style="color:red;">type</span><span style="color:blue;">=</span>"<span style="color:blue;">Components.OrderRepository, Components</span>"<span style="color:blue;">&gt;</span></p>
<p style="margin:0;">&nbsp;</p>
<p style="margin:0;"><span style="color:blue;">&nbsp; &nbsp; &nbsp; &nbsp; &lt;</span><span style="color:#a31515;">interceptors</span><span style="color:blue;">&gt;</span></p>
<p style="margin:0;"><span style="color:blue;">&nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &lt;</span><span style="color:#a31515;">interceptor</span><span style="color:blue;">&gt;</span>${LoggingInterceptor}<span style="color:blue;">&lt;/</span><span style="color:#a31515;">interceptor</span><span style="color:blue;">&gt;</span></p>
<p style="margin:0;"><span style="color:blue;">&nbsp; &nbsp; &nbsp; &nbsp; &lt;/</span><span style="color:#a31515;">interceptors</span><span style="color:blue;">&gt;</span></p>
<p style="margin:0;">&nbsp;</p>
<p style="margin:0;"><span style="color:blue;">&nbsp; &nbsp; &nbsp; &lt;/</span><span style="color:#a31515;">component</span><span style="color:blue;">&gt;</span></p>
</div>

What we basically did was tell Windsor that whenever an IOrderRepository is requested, we should return an instance of OrderRepository and each time a method of that instance is called, it needs to be intercepted by our LoggingInterceptor.

So if we simply call the IOrderRepository methods like this (obviously these are dummy calls without real parameters and we're also ignoring return values):

<div style="font-family:Consolas;font-size:10pt;color:black;background:white;">
<p style="margin:0;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span style="color:blue;">var</span> repository = <span style="color:#2b91af;">Container</span>.Resolve&lt;<span style="color:#2b91af;">IOrderRepository</span>&gt;();</p>
<p style="margin:0;">&nbsp;</p>
<p style="margin:0;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; repository.GetAll();</p>
<p style="margin:0;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; repository.FindOne(<span style="color:blue;">new</span> <span style="color:#2b91af;">Criteria</span>());</p>
<p style="margin:0;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; repository.FindMany(<span style="color:blue;">new</span> <span style="color:#2b91af;">Criteria</span>());</p>
<p style="margin:0;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; repository.GetById(<span style="color:#2b91af;">Guid</span>.Empty);</p>
<p style="margin:0;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; repository.Store(<span style="color:blue;">null</span>);</p>
</div>

The following output is logged:

<pre>
21:53:42.6942016 Entering method: Components.OrderRepository.GetAll
21:53:42.6942016 Leaving mehod: Components.OrderRepository.GetAll
21:53:42.6942016 Entering method: Components.OrderRepository.FindOne
21:53:42.6942016 Leaving mehod: Components.OrderRepository.FindOne
21:53:42.6942016 Entering method: Components.OrderRepository.FindMany
21:53:42.6942016 Leaving mehod: Components.OrderRepository.FindMany
21:53:42.7042160 Entering method: Components.OrderRepository.GetById
21:53:42.7042160 Leaving mehod: Components.OrderRepository.GetById
21:53:42.7042160 Entering method: Components.OrderRepository.Store
21:53:42.7042160 Leaving mehod: Components.OrderRepository.Store
</pre>

And we didn't have to change the OrderRepository implementation. In fact, we can use our LoggingInterceptor wherever we like, as long as the component to be logged is registered with Windsor.  And we can easily switch between logging or no logging by switching config files.

Obviously, this was just a really simple example but i hope you realize how powerful this technique is and how far you can go with this.
