As i mentioned in <a href="http://davybrion.com/blog/2008/05/adding-behavior-without-modifying-existing-code-with-windsor/">a previous post</a>, Windsor's Interceptors are a great way to dynamically add behavior to a class.  But what does it cost? There's a lot of stuff going on behind to scenes to make that 'magic' work and surely, there's a performance penalty involved somewhere.

I wanted to see what the cost of this approach is, so i created the following interface and class:

<code>
<div style="font-family:Consolas;font-size:10pt;color:black;background:white;">
<p style="margin:0;">&nbsp;&nbsp;&nbsp; <span style="color:blue;">public</span> <span style="color:blue;">interface</span> <span style="color:#2b91af;">IDummy</span></p>
<p style="margin:0;">&nbsp;&nbsp;&nbsp; {</p>
<p style="margin:0;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span style="color:blue;">void</span> DoSomething();</p>
<p style="margin:0;">&nbsp;&nbsp;&nbsp; }</p>
<p style="margin:0;">&nbsp;</p>
<p style="margin:0;">&nbsp;&nbsp;&nbsp; <span style="color:blue;">public</span> <span style="color:blue;">class</span> <span style="color:#2b91af;">Dummy</span> : <span style="color:#2b91af;">IDummy</span></p>
<p style="margin:0;">&nbsp;&nbsp;&nbsp; {</p>
<p style="margin:0;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span style="color:blue;">public</span> <span style="color:blue;">void</span> DoSomething() {}</p>
<p style="margin:0;">&nbsp;&nbsp;&nbsp; }</p>
</div>
</code>

Nothing special there... just a class with a method that doesn't do anything. Combine that with an interceptor that doesn't do anything:

<code>
<div style="font-family:Consolas;font-size:10pt;color:black;background:white;">
<p style="margin:0;">&nbsp;&nbsp;&nbsp; <span style="color:blue;">public</span> <span style="color:blue;">class</span> <span style="color:#2b91af;">DummyInterceptor</span> : Castle.Core.Interceptor.<span style="color:#2b91af;">IInterceptor</span></p>
<p style="margin:0;">&nbsp;&nbsp;&nbsp; {</p>
<p style="margin:0;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span style="color:blue;">public</span> <span style="color:blue;">void</span> Intercept(<span style="color:#2b91af;">IInvocation</span> invocation)</p>
<p style="margin:0;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; {</p>
<p style="margin:0;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span style="color:green;">// do nothing, just proceed with the original call</span></p>
<p style="margin:0;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; invocation.Proceed();</p>
<p style="margin:0;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; }</p>
<p style="margin:0;">&nbsp;&nbsp;&nbsp; }</p>
</div>
</code>

And then we configure the component and the interceptor like this:

<code>
<div style="font-family:Consolas;font-size:10pt;color:black;background:white;">
<p style="margin:0;"><span style="color:blue;">&nbsp; &nbsp; &nbsp; &lt;</span><span style="color:#a31515;">component</span><span style="color:blue;"> </span><span style="color:red;">id</span><span style="color:blue;">=</span>"<span style="color:blue;">DummyInterceptor</span>"<span style="color:blue;"> </span><span style="color:red;">service</span><span style="color:blue;">=</span>"<span style="color:blue;">Components.DummyInterceptor, Components</span>"</p>
<p style="margin:0;"><span style="color:blue;">&nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp;  </span><span style="color:red;">type</span><span style="color:blue;">=</span>"<span style="color:blue;">Components.DummyInterceptor, Components</span>"</p>
<p style="margin:0;"><span style="color:blue;">&nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp;  </span><span style="color:red;">lifestyle</span><span style="color:blue;">=</span>"<span style="color:blue;">transient</span>"<span style="color:blue;"> /&gt;</span></p>
</div>

<div style="font-family:Consolas;font-size:10pt;color:black;background:white;">
<p style="margin:0;"><span style="color:blue;">&nbsp; &nbsp; &nbsp; &lt;</span><span style="color:#a31515;">component</span><span style="color:blue;"> </span><span style="color:red;">id</span><span style="color:blue;">=</span>"<span style="color:blue;">Dummy</span>"<span style="color:blue;"> </span><span style="color:red;">service</span><span style="color:blue;">=</span>"<span style="color:blue;">Components.IDummy, Components</span>"<span style="color:blue;"> </span><span style="color:red;">type</span><span style="color:blue;">=</span>"<span style="color:blue;">Components.Dummy, Components</span>"<span style="color:blue;">&gt;</span></p>
<p style="margin:0;">&nbsp;</p>
<p style="margin:0;"><span style="color:blue;">&nbsp; &nbsp; &nbsp; &nbsp; &lt;</span><span style="color:#a31515;">interceptors</span><span style="color:blue;">&gt;</span></p>
<p style="margin:0;"><span style="color:blue;">&nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &lt;</span><span style="color:#a31515;">interceptor</span><span style="color:blue;">&gt;</span>${DummyInterceptor}<span style="color:blue;">&lt;/</span><span style="color:#a31515;">interceptor</span><span style="color:blue;">&gt;</span></p>
<p style="margin:0;"><span style="color:blue;">&nbsp; &nbsp; &nbsp; &nbsp; &lt;/</span><span style="color:#a31515;">interceptors</span><span style="color:blue;">&gt;</span></p>
<p style="margin:0;">&nbsp;</p>
<p style="margin:0;"><span style="color:blue;">&nbsp; &nbsp; &nbsp; &lt;/</span><span style="color:#a31515;">component</span><span style="color:blue;">&gt;</span></p>
</div>
</code>

So what do we have now? We have a component which has a method that doesn't do anything. And we've assigned an interceptor that doesn't do anything... it just executes the original call without adding any behavior. Pretty useless, right? Right, but this is ideal to compare the runtime cost of merely intercepting calls to components. So the differences you'll see below are without adding the extra behavior to your components. The differences you'll see below are purely because each call is intercepted.

This is the code i used to test the difference:

<code>

<div style="font-family:Consolas;font-size:10pt;color:black;background:white;">
<p style="margin:0;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; [<span style="color:#2b91af;">Test</span>]</p>
<p style="margin:0;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span style="color:blue;">public</span> <span style="color:blue;">void</span> TestDummyPerformance()</p>
<p style="margin:0;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; {</p>
<p style="margin:0;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; Time(() =&gt; CallMethodXAmountOfTimes(<span style="color:blue;">new</span> <span style="color:#2b91af;">Dummy</span>(), 1000000));</p>
<p style="margin:0;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; Time(() =&gt; CallMethodXAmountOfTimes(<span style="color:#2b91af;">Container</span>.Resolve&lt;<span style="color:#2b91af;">IDummy</span>&gt;(), 1000000));</p>
<p style="margin:0;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; }</p>
<p style="margin:0;">&nbsp;</p>
<p style="margin:0;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span style="color:blue;">private</span> <span style="color:blue;">void</span> CallMethodXAmountOfTimes(<span style="color:#2b91af;">IDummy</span> dummy, <span style="color:blue;">int</span> times) </p>
<p style="margin:0;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; {</p>
<p style="margin:0;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span style="color:blue;">for</span> (<span style="color:blue;">int</span> i = 0; i &lt; times; i++)</p>
<p style="margin:0;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; {</p>
<p style="margin:0;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; dummy.DoSomething();&nbsp;&nbsp;&nbsp; </p>
<p style="margin:0;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; }</p>
<p style="margin:0;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; }</p>
<p style="margin:0;">&nbsp;</p>
<p style="margin:0;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span style="color:blue;">private</span> <span style="color:blue;">void</span> Time(<span style="color:#2b91af;">Action</span> action)</p>
<p style="margin:0;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; {</p>
<p style="margin:0;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span style="color:#2b91af;">DateTime</span> before = <span style="color:#2b91af;">DateTime</span>.Now;</p>
<p style="margin:0;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; action();</p>
<p style="margin:0;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span style="color:#2b91af;">Console</span>.WriteLine(<span style="color:#a31515;">"Time elapsed : "</span> + (<span style="color:#2b91af;">DateTime</span>.Now - before));</p>
<p style="margin:0;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; }</p>
</div>

</code>

First, we call the DoSomething method on a regular Dummy instance 1 million times. The time that takes is written to the Console. Then we call the DoSomething method (again, 1 million times) on an IDummy instance provided by the container, which has our interceptor attached to it (note: when you do not have an interceptor attached, there is NO runtime penalty!).

The output of the test (on my machine) is the following:

Time elapsed : 00:00:00.0100144
Time elapsed : 00:00:00.5808352

As you can see, the second time (using the intercepted instance) is significantly slower than using a concrete instance.  But honestly, this is after calling the method <strong>one million times</strong>. And it only takes about half a second (on a virtualized Windows XP running on a cheap Macbook ;)) to call this method one million times.  In a real-world scenario, you probably won't notice any performance hit unless the code that is intercepted is in a long, tight loop or something like that.

Having said that, i do think using the interceptor approach should be a temporary action in most cases. If you have to debug weird issues, i think adding an extensive logging/tracing interceptor to your components can be extremely valuable without having to pollute your code with logging/tracing statements.  But if you want certain behavior to be added to your classes without it being configurable, there certainly are better options to use. <a href="http://www.postsharp.org/">PostSharp</a> is a library which enables <a href="http://en.wikipedia.org/wiki/Aspect_oriented_programming">Aspect Oriented Programming</a> without performance penalties. This approach basically allows you to add specific behavior to methods or classes by placing attributes on them.  PostSharp will then modify the compiled byte-code to add the 'aspects' (the behavior that you want to add) to the real code. I'll write a post about this approach soon ;)
