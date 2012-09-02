Suppose you have the following abstract class:

<code>
<div style="font-family: Consolas; font-size: 10pt; color: black; background: white;">
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; <span style="color: blue;">public</span> <span style="color: blue;">abstract</span> <span style="color: blue;">class</span> <span style="color: #2b91af;">MyAbstractClass</span></p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; {</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span style="color: blue;">public</span> <span style="color: blue;">void</span> DoSomethingInteresting()</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; {</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span style="color: green;">// some stuff would go here...</span></p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; DoSomethingSpecific();</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span style="color: green;">// more stuff would go here...</span></p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; }</p>
<p style="margin: 0px;">&nbsp;</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span style="color: blue;">protected</span> <span style="color: blue;">abstract</span> <span style="color: blue;">void</span> DoSomethingSpecific();</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; }</p>
</div>
</code>

What do you do if you want to write tests that set expectations on the protected abstract method?

Ideally, i'd be able to do this:

<code>
<div style="font-family: Consolas; font-size: 10pt; color: black; background: white;">
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; [<span style="color: #2b91af;">Test</span>]</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span style="color: blue;">public</span> <span style="color: blue;">void</span> CallsProtectedAbstractMethod()</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; {</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span style="color: blue;">var</span> mocks = <span style="color: blue;">new</span> <span style="color: #2b91af;">MockRepository</span>();</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span style="color: blue;">var</span> myObject = mocks.DynamicMock&lt;<span style="color: #2b91af;">MyAbstractClass</span>&gt;();</p>
<p style="margin: 0px;">&nbsp;</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; myObject.Expect(m =&gt; m.DoSomethingSpecific()); <span style="color: green;">// &lt;= compile error</span></p>
<p style="margin: 0px;">&nbsp;</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; myObject.DoSomethingInteresting();</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; myObject.VerifyAllExpectations();</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; }</p>
</div>
</code>

But as the comment points out, this causes a compiler error because the DoSomethingSpecific method is inaccessible to anything except derived classes.

So what is the best way to deal with this? For this example, i could manually create a derived class, set a flag in its DoSomethingSpecific implementation and then assert on the value of that flag.  But that approach doesn't really scale well to complexer classes.  If i want to use a mocking framework for this, i have two options (that i can think of right now): i either make the abstract method public so the above code would compile, or i can make it protected internal and then allow my internal members to be visible to my Test project.

I dislike both approaches. I wholeheartedly agree with the <a href="http://ayende.com/Blog/archive/2008/06/25/Public-vs.-Published.aspx">"Public vs Published"</a> stance, but making a method that really should only be accessible from derived classes public merely for testing purposes just doesn't sit well at all with me.  Changing the abstract method's visibility from protected to protected internal seems to be the lesser of two evils, although i've always seriously disliked the internal keyword and especially extending the visibility of it through the InternalsVisibleTo keyword. This approach just screams "HACK!" to me, but in this case i can't really think of anything better.

If anyone call tell me what the best way would be to deal with this specific scenario, please do share :)

On a sidenote: i am happy about the fact that i do seem to periodically reflect on the merit of my many 'rules' such as "thou shalt not expose thy privates to an even larger group than the internal circle to whom they've already been shown".  I've already mentioned that i've had it with rules that serve only theoretical purposes without any practical merit... there's no reason why my own rules should be excluded from this :)