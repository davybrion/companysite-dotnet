Ever been in a situation where you notice that one of the team-members used classes from an assembly that really shouldn't be used in that part of the code? For instance, accessing the data layer from the presentation layer. It's not always easy to keep an eye on improper assembly usage. You could keep an eye on the referenced assemblies manually. You could write an FxCop rule that checks for disallowed assembly references.  There's a lot of stuff you can do, but it'll always be an after-the-facts check. By that time, the 'illegal' code is already there.

Wouldn't it be cool if you could break the compilation whenever a developer tries to build code that has improper assembly usage? Actually, with PostSharp, we can do just that.  You can simply create an aspect like this:

<code>

<div style="font-family: Consolas; font-size: 10pt; color: black; background: white;">
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; [<span style="color: #2b91af;">Serializable</span>]</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; [<span style="color: #2b91af;">AttributeUsage</span>(<span style="color: #2b91af;">AttributeTargets</span>.Assembly)]</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; <span style="color: blue;">public</span> <span style="color: blue;">class</span> <span style="color: #2b91af;">SanityCheck</span> : <span style="color: #2b91af;">OnMethodInvocationAspect</span></p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; {</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span style="color: blue;">public</span> <span style="color: blue;">override</span> <span style="color: blue;">bool</span> CompileTimeValidate(System.Reflection.<span style="color: #2b91af;">MethodBase</span> method)</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; {</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span style="color: blue;">var</span> methodName = method.DeclaringType.FullName + <span style="color: #a31515;">"."</span> + method.Name;</p>
<p style="margin: 0px;">&nbsp;</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span style="color: blue;">var</span> message = <span style="color: blue;">new</span> <span style="color: #2b91af;">Message</span>(<span style="color: #2b91af;">SeverityType</span>.Fatal, <span style="color: #a31515;">"ProhibitedMethodCall"</span>, <span style="color: #2b91af;">String</span>.Format(</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span style="color: #a31515;">"Sorry, but we can't allow you to call {0} from the current assembly"</span>, methodName),</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span style="color: #a31515;">"SanityCheck"</span>);</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span style="color: #2b91af;">MessageSource</span>.MessageSink.Write(message);</p>
<p style="margin: 0px;">&nbsp;</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span style="color: blue;">return</span> <span style="color: blue;">false</span>;</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; }</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; }</p>
</div>

</code>

First of all, we declare that the SanityCheck attribute can only be applied on the assembly level. Notice that we inherit from OnMethodInvocationAspect. This aspect is applied on events, properties, and methods. So basically, whenever you call a property or method or try to hook to an event in an assembly that you've applied this attribute to, our SanityCheck aspect will run.  Well, actually we won't get that far. We override the virtual CompileTimeValidate method where we display a message and then we return false. Meaning that this compilation is invalid.

Suppose you've used the attribute in the AssemblyInfo.cs file of your presentation layer like this:

<code>

<div style="font-family: Consolas; font-size: 10pt; color: black; background: white;">
<p style="margin: 0px;">[<span style="color: blue;">assembly</span>: <span style="color: #2b91af;">SanityCheck</span>(AttributeTargetAssemblies = <span style="color: #a31515;">"System.Data"</span>)]</p>
</div>

</code>

If you try to compile the following code:

<code>

<div style="font-family: Consolas; font-size: 10pt; color: black; background: white;">
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span style="color: #2b91af;">DataTable</span> table = <span style="color: blue;">new</span> <span style="color: #2b91af;">DataTable</span>();</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; table.NewRow();</p>
</div>

</code>

You'll see the following line in your build output:

<code>

EXEC : error ProhibitedMethodCall: Sorry, but we can't allow you to call System.Data.DataTable.NewRow from the current assembly

</code>

And most importantly:

<code>

========== Build: 0 succeeded or up-to-date, 1 failed, 0 skipped ==========

</code>

Obviously, this will only cause compile errors when you try to touch the prohibited parts within the assembly where you used the SanityCheck attribute.  If you call a method in another assembly that is allowed to touch System.Data, it will not cause a compile error.
