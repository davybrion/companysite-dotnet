<a href="http://davybrion.com/blog/2008/05/windsors-interceptors-and-performance/#comment-222">Gael Fraiteur's comment</a> on my previous post made me realize that my performance test in the previous post wasn't all that good...

Let's go back to the part of the code that performed the test:

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

The Time method has an Action parameter, which basically points to a block of code that will be executed when you call it. In this case the action() line causes the block of code to be executed. I have a habit of trying to write concise code, so the instantiation of the dummy instance is in both cases inlined in the block of code that will be executed by the Time() method. As Gael points out in his comment, Windsor generates a proxy when you request a component that has an interceptor assigned to it, and this is obviously a more expensive operation than simply new-ing a concrete instance.  So this extra cost was reflected in the results as well.

If we change the test code to this:

<code>

<div style="font-family:Consolas;font-size:10pt;color:black;background:white;">
<p style="margin:0;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; [<span style="color:#2b91af;">Test</span>]</p>
<p style="margin:0;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span style="color:blue;">public</span> <span style="color:blue;">void</span> TestDummyPerformance()</p>
<p style="margin:0;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; {</p>
<p style="margin:0;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span style="color:blue;">var</span> dummy = <span style="color:blue;">new</span> <span style="color:#2b91af;">Dummy</span>();</p>
<p style="margin:0;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; Time(() =&gt; CallMethodXAmountOfTimes(dummy, 1000000));</p>
<p style="margin:0;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span style="color:blue;">var</span> interceptedDummy = <span style="color:#2b91af;">Container</span>.Resolve&lt;<span style="color:#2b91af;">IDummy</span>&gt;();</p>
<p style="margin:0;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; Time(() =&gt; CallMethodXAmountOfTimes(interceptedDummy, 1000000));</p>
<p style="margin:0;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; }</p>
</div>

</code>

Now, the difference is not so big as it was in the previous test:
Time elapsed : 00:00:00.0100144
Time elapsed : 00:00:00.2403456

Again, this is for one million method calls... in a real world scenario, you probably won't notice the performance hit.
