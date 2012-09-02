I stumbled upon a blog post by Kim Hamilton on <a href="http://blogs.msdn.com/kimhamil/archive/2008/11/05/when-to-call-dispose.aspx">When To Call Dispose</a>.  The most interesting part about the post is that this is from someone who works on the Base Class Libraries team.  The following part is especially striking:

<blockquote>
A recent internal email thread unearthed extreme differences of opinion about when Dispose(void) should be called on an IDisposable. The rival suggestions were:

1. Always call Dispose

2. Avoid calling Dispose; rely on cleanup at finalization

This led to a long discussion and a realization that -- while it seems like we’ve said everything there is to say about Dispose -- it’s time for some more Dispose guidance.
</blockquote>

I don't know about you guys, but the thought of Base Class Library developers not being sure on when Dispose should be called is something that makes me quite uncomfortable. If anyone should know, it's these guys, right?

Another interesting quote from the post:

<blockquote>
Whatever distinction we eventually use, API docs should explicitly call out IDisposables that must be Disposed. 
</blockquote>

Great... now we are supposed to rely on the MSDN docs to find out which IDisposables need to be disposed and which ones don't. 

I kinda like the IDisposable pattern, but it's truly a shame that there are so many types that implement the IDisposable interface without really needing to implement it.  I've always considered the IDisposable interface to be a contract which states "if you use me, you must dispose me". For certain types, this is true.  Unfortunately, for other types that implement the interface it's not true even though the <a href="http://msdn.microsoft.com/en-us/library/system.idisposable.aspx">IDisposable documentation</a> is pretty clear about this:

<blockquote>When calling a class that implements the IDisposable interface, use the try/finally pattern to make sure that unmanaged resources are disposed of even if an exception interrupts your application.</blockquote>

In a perfect .NET world, there would be no types implementing IDisposable without actually requiring disposal (IMHO).  Again, i think the interface is a contract that must be followed by everyone who makes use of it.  Then again, it does feel pretty pointless to dispose types of which you know that it really doesn't make a difference.

The introduction of explicit <a href="http://blogs.msdn.com/bclteam/archive/2008/11/11/introduction-to-code-contracts-melitta-andersen.aspx">Code Contracts in .NET 4.0</a>, got me thinking about trying to make disposal of types that really require it enforced by the compiler.  What i have in mind is not possible with the Code Contracts in .NET 4.0, but it's something that i think would be a good addition to the .NET Framework.

Suppose we could do the following:

<code>
<div style="font-family: Consolas; font-size: 10pt; color: black; background: white;">
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; [<span style="color: #2b91af;">RequiresDisposal</span>]</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; <span style="color: blue;">public</span> <span style="color: blue;">class</span> <span style="color: #2b91af;">MyDisposableType</span> : <span style="color: #2b91af;">IDisposable</span></p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; {</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span style="color: blue;">public</span> <span style="color: blue;">void</span> DoSomething()</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; {</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span style="color: green;">// does something really cool and/or important</span></p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; }</p>
<p style="margin: 0px;">&nbsp;</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span style="color: blue;">public</span> <span style="color: blue;">void</span> Dispose()</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; {</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span style="color: green;">// this would contain an important disposable implementation</span></p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; }</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; }</p>
</div>

</code>

Basically just a type which implements IDisposable, and there's an attribute that specifies that the disposal of this type is really required.  The following code should then fail to compile:

<code>
<div style="font-family: Consolas; font-size: 10pt; color: black; background: white;">
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span style="color: blue;">private</span> <span style="color: blue;">void</span> SomeMethod()</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; {</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span style="color: blue;">new</span> <span style="color: #2b91af;">MyDisposableType</span>().DoSomething();</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; }</p>
</div>
</code>
 
Whereas this code would compile without errors:

<code>
<div style="font-family: Consolas; font-size: 10pt; color: black; background: white;">
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span style="color: blue;">private</span> <span style="color: blue;">void</span> SomeMethod()</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; {</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span style="color: blue;">using</span> (<span style="color: blue;">var</span> disposableType = <span style="color: blue;">new</span> <span style="color: #2b91af;">MyDisposableType</span>())</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; {</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; disposableType.DoSomething();</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; }</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; }</p>
</div>
</code> 
 
It's just a thought and obviously, this isn't possible right now.  But i think it would be great if it were possible.  We could do all sorts of things with this approach... for instance, if an object owns a reference to an IDisposable type which has the [RequiresDisposal] attribute, the compiler could enforce that the object owning the reference must implement IDisposable and have the [RequiresDisposal] attribute as well.  I think we'd end up with a system were types that really require disposal can't be used without properly disposing them.

Thoughts?