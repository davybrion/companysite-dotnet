A little while ago, i talked about <a href="http://davybrion.com/blog/2008/05/adding-behavior-without-modifying-existing-code-with-windsor/">adding behavior to components using Windsor's Interceptors</a>.  I wanted to try something similar with <a href="http://www.postsharp.org/">PostSharp</a>, which is a very powerful <a href="http://en.wikipedia.org/wiki/Aspect-Oriented_Programming">Aspect Oriented Programming</a> framework for the .NET world.  Discussing everything PostSharp can do (which is <strong>a lot</strong>) is way beyond the scope of this post, so we'll just focus on what i'm trying to do, and how PostSharp helps us with that. 

In the previous post, i used a logging example... i didn't want logging code mixed up with my real code, so i used a Windsor Interceptor to intercept calls to classes that i had configured to be logged.  The interceptor would then log before and after the method calls were executed.  In this post we'll do something very similar, but a bit more advanced.  We're gonna write some tracing logic that will write a trace statement when the method is entered, along with the parameter values for each of its parameters.  Then after the original method has executed, we'll write a trace statement to indicate that we have left the method, and we'll display the return value of the method as well, if there is one. That kind of tracing information can be very valuable, but writing that code is extremely tedious work.  Nobody wants to do that, right? I sure as hell don't.

We'll use the following simple class as an example:

<code>

<div style="font-family: Consolas; font-size: 10pt; color: black; background: white;">
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; <span style="color: blue;">public</span> <span style="color: blue;">class</span> <span style="color: #2b91af;">Calculator</span></p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; {</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span style="color: blue;">public</span> <span style="color: blue;">int</span> Add(<span style="color: blue;">int</span> firstValue, <span style="color: blue;">int</span> secondValue)</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; {</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span style="color: blue;">return</span> firstValue + secondValue;</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; }</p>
<p style="margin: 0px;">&nbsp;</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span style="color: blue;">public</span> <span style="color: blue;">int</span> Subtract(<span style="color: blue;">int</span> firstValue, <span style="color: blue;">int</span> secondValue)</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; {</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span style="color: blue;">return</span> firstValue - secondValue;</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; }</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; }</p>
</div>

</code>

This calculator has a pretty limited feature-set, but we don't need anything more for this example. Ok, so how do we add the tracing code without actually adding it to this code? We can just compile the code, and then we can have PostSharp inject some extra code to the compiled code.  Sounds pretty hard, right? It is. But PostSharp Laos is a small framework that runs on top of PostSharp and it takes care of the hard work for you.  PostSharp Laos offers some base classes which make it really easy for you to write your aspects (if you don't understand that term, you should have clicked on the aspect oriented programming link earlier in the post ;)).  There are a couple of approaches you can choose between, and they all have their pro's and con's.  A nice overview of the different kinds of aspects you can inherit from can be found <a href="http://doc.postsharp.org/1.0/UserGuide/Laos/AspectKinds/Overview.html">here</a>.  For this example, we'll use the OnMethodInvocationAspect base class. This basically intercepts calls to methods and allows you to add some logic. 

So what would our tracing aspect look like? How about this:

<code>

<div style="font-family: Consolas; font-size: 10pt; color: black; background: white;">
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; [<span style="color: #2b91af;">Serializable</span>]</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; <span style="color: blue;">public</span> <span style="color: blue;">class</span> <span style="color: #2b91af;">TraceAspect</span> : <span style="color: #2b91af;">OnMethodInvocationAspect</span></p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; {</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span style="color: blue;">public</span> <span style="color: blue;">override</span> <span style="color: blue;">void</span> OnInvocation(<span style="color: #2b91af;">MethodInvocationEventArgs</span> eventArgs)</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; {</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span style="color: blue;">string</span> methodName = GetFullMethodName(eventArgs);</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; WriteOutput(<span style="color: #2b91af;">String</span>.Format(<span style="color: #a31515;">"Entering {0}"</span>, methodName));</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; WriteParameterInfo(eventArgs.Delegate.Method.GetParameters(), eventArgs.GetArgumentArray());</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span style="color: blue;">base</span>.OnInvocation(eventArgs); <span style="color: green;">// calls the original method</span></p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; WriteOutput(<span style="color: #2b91af;">String</span>.Format(<span style="color: #a31515;">"Leaving {0}"</span>, methodName));</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; WriteReturnValueInfo(eventArgs.Delegate.Method.ReturnType, eventArgs.ReturnValue);</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; }</p>
<p style="margin: 0px;">&nbsp;</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span style="color: blue;">private</span> <span style="color: blue;">static</span> <span style="color: blue;">void</span> WriteParameterInfo(<span style="color: #2b91af;">ParameterInfo</span>[] parameterInfos, <span style="color: blue;">object</span>[] parameters)</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; {</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span style="color: blue;">if</span> (parameterInfos == <span style="color: blue;">null</span> || parameterInfos.Length == 0)</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; {</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span style="color: blue;">return</span>;</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; }</p>
<p style="margin: 0px;">&nbsp;</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; WriteOutput(<span style="color: #a31515;">"With the following parameters: "</span>);</p>
<p style="margin: 0px;">&nbsp;</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span style="color: blue;">for</span> (<span style="color: blue;">int</span> i = 0; i &lt; parameterInfos.Length; i++)</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; {</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; WriteOutput(<span style="color: blue;">string</span>.Format(<span style="color: #a31515;">"{0} = {1}"</span>, parameterInfos[i].Name, parameters[i]));</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; }</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; }</p>
<p style="margin: 0px;">&nbsp;</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span style="color: blue;">private</span> <span style="color: blue;">static</span> <span style="color: blue;">void</span> WriteReturnValueInfo(<span style="color: #2b91af;">Type</span> returnType, <span style="color: blue;">object</span> returnValue)</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; {</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span style="color: blue;">if</span> (returnType == <span style="color: blue;">typeof</span>(<span style="color: blue;">void</span>))</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; {</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span style="color: blue;">return</span>;</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; }</p>
<p style="margin: 0px;">&nbsp;</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; WriteOutput(<span style="color: blue;">string</span>.Format(<span style="color: #a31515;">"Return Value: {0}"</span>, returnValue));&nbsp;&nbsp;&nbsp; </p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; }</p>
<p style="margin: 0px;">&nbsp;</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span style="color: blue;">private</span> <span style="color: blue;">static</span> <span style="color: blue;">string</span> GetFullMethodName(<span style="color: #2b91af;">MethodInvocationEventArgs</span> args)</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; {</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span style="color: blue;">return</span> args.Delegate.Target.GetType().FullName + <span style="color: #a31515;">"."</span> + args.Delegate.Method.Name;</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; }</p>
<p style="margin: 0px;">&nbsp;</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span style="color: blue;">private</span> <span style="color: blue;">static</span> <span style="color: blue;">void</span> WriteOutput(<span style="color: blue;">string</span> line)</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; {</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span style="color: #2b91af;">Trace</span>.WriteLine(<span style="color: #2b91af;">DateTime</span>.Now.TimeOfDay + <span style="color: #a31515;">" "</span> + line);</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; }</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; }</p>
</div>

</code>

So now we have a trace aspect which shows some valuable information about method calls... it shows when the method is entered, which parameters were passed in, when we leave the method and what the return value is.  

So how do we apply this aspect to our code? We modify the definition of the Calculator class like this:

<code>

<div style="font-family: Consolas; font-size: 10pt; color: black; background: white;">
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; [<span style="color: #2b91af;">TraceAspect</span>]</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; <span style="color: blue;">public</span> <span style="color: blue;">class</span> <span style="color: #2b91af;">Calculator</span></p>
</div>

</code>

And then we compile... Well actually you have to add something to your project file so PostSharp can modify the compiled code, but since that's covered nicely in the PostSharp documentation we'll skip that step in this post.  Anyways, when you compile this, you'll get the following build output:

<code>

------ Build started: Project: UsingPostSharp, Configuration: Debug Any CPU ------
C:\WINDOWS\Microsoft.NET\Framework\v3.5\Csc.exe /noconfig /nowarn:1701,1702 /errorreport:prompt /warn:4 /define:POSTSHARP;DEBUG;TRACE /reference:..\Libs\postsharp\PostSharp.Laos.dll /reference:..\Libs\postsharp\PostSharp.Public.dll /reference:"C:\Program Files\Reference Assemblies\Microsoft\Framework\v3.5\System.Core.dll" /reference:"C:\Program Files\Reference Assemblies\Microsoft\Framework\v3.5\System.Data.DataSetExtensions.dll" /reference:C:\WINDOWS\Microsoft.NET\Framework\v2.0.50727\System.Data.dll /reference:C:\WINDOWS\Microsoft.NET\Framework\v2.0.50727\System.dll /reference:C:\WINDOWS\Microsoft.NET\Framework\v2.0.50727\System.Xml.dll /reference:"C:\Program Files\Reference Assemblies\Microsoft\Framework\v3.5\System.Xml.Linq.dll" /debug+ /debug:full /filealign:512 /optimize- /out:obj\Debug\UsingPostSharp.exe /target:exe Program.cs Properties\AssemblyInfo.cs TraceAspect.cs

Compile complete -- 0 errors, 0 warnings
<strong>"C:\mydocs\src\dbr\Experiments\UsingPostSharp\Libs\postsharp\PostSharp.exe"  "C:\mydocs\src\dbr\Experiments\UsingPostSharp\Libs\postsharp\Default.psproj" "C:\mydocs\src\dbr\Experiments\UsingPostSharp\UsingPostSharp\obj\Debug\UsingPostSharp.exe" "/P:Output=obj\Debug\PostSharp\UsingPostSharp.exe " "/P:ReferenceDirectory=C:\mydocs\src\dbr\Experiments\UsingPostSharp\UsingPostSharp " "/P:Configuration=Debug " "/P:Platform=AnyCPU " "/P:SearchPath=bin\Debug\, " "/P:IntermediateDirectory=obj\Debug\PostSharp " "/P:CleanIntermediate=False " "/P:MSBuildProjectFullPath=C:\mydocs\src\dbr\Experiments\UsingPostSharp\UsingPostSharp\UsingPostSharp.csproj " "/P:SignAssembly=False " "/P:PrivateKeyLocation= "
PostSharp 1.0 [1.0.9.365] - Copyright (c) Gael Fraiteur, 2005-2008.


info PS0035: C:\WINDOWS\Microsoft.NET\Framework\v2.0.50727\ilasm.exe "C:\mydocs\src\dbr\Experiments\UsingPostSharp\UsingPostSharp\obj\Debug\PostSharp\UsingPostSharp.il" /QUIET /EXE /PDB "/RESOURCE=C:\mydocs\src\dbr\Experiments\UsingPostSharp\UsingPostSharp\obj\Debug\PostSharp\UsingPostSharp.res" "/OUTPUT=C:\mydocs\src\dbr\Experiments\UsingPostSharp\UsingPostSharp\obj\Debug\PostSharp\UsingPostSharp.exe" /SUBSYSTEM=3 /FLAGS=1 /BASE=4194304 /STACK=1048576 /ALIGNMENT=512 /MDV=v2.0.50727 
UsingPostSharp -> C:\mydocs\src\dbr\Experiments\UsingPostSharp\UsingPostSharp\bin\Debug\UsingPostSharp.exe</strong>
========== Build: 1 succeeded or up-to-date, 0 failed, 0 skipped ==========

</code>

If we use the calculator class like this:

<code>

<div style="font-family: Consolas; font-size: 10pt; color: black; background: white;">
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span style="color: blue;">var</span> calculator = <span style="color: blue;">new</span> <span style="color: #2b91af;">Calculator</span>();</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; calculator.Add(10, 15);</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; calculator.Subtract(10, 15);</p>
</div>

</code>

We'll get the following trace output:

<code>

21:48:49.9414848 Entering UsingPostSharp.Calculator.~Add
21:48:49.9615136 With the following parameters: 
21:48:49.9615136 firstValue = 10
21:48:49.9615136 secondValue = 15
21:48:49.9615136 Leaving UsingPostSharp.Calculator.~Add
21:48:49.9615136 Return Value: 25
21:48:49.9615136 Entering UsingPostSharp.Calculator.~Subtract
21:48:49.9615136 With the following parameters: 
21:48:49.9615136 firstValue = 10
21:48:49.9615136 secondValue = 15
21:48:49.9615136 Leaving UsingPostSharp.Calculator.~Subtract
21:48:49.9615136 Return Value: -5

</code>

How nice is that? With some exception handling, better formatting and some clever indenting, this tracing aspect could be the only tracing code you'll ever need from now on :)

So how does it work? Well, we can look at the compiled code in reflector for that.  Here's how the Add method looks like after PostSharp modified the code:

<code>

    private int ~Add(int firstValue, int secondValue)
    {
        return (firstValue + secondValue);
    }

    [DebuggerNonUserCode, CompilerGenerated]
    public int Add(int firstValue, int secondValue)
    {
        Delegate delegateInstance = new ~PostSharp~Laos~Implementation.~delegate~0(this.~Add);
        object[] arguments = new object[] { firstValue, secondValue };
        MethodInvocationEventArgs eventArgs = new MethodInvocationEventArgs(delegateInstance, arguments);
        ~PostSharp~Laos~Implementation.TraceAspect~1.OnInvocation(eventArgs);
        return (int) eventArgs.ReturnValue;
    }

</code>

As you can see, it's renamed our Add method to ~Add, and it added another Add method which calls the OnInvocation method of our TraceAspect class with the correct MethodInvocationEventArgs.  Pretty cool stuff IMO.  Oh and btw, if you're debugging this code in Visual Studio, you still only see your own code, with the original method name.  While you step through it, the code runs just like it normally would, and the applied aspects are also executed. Very impressive.

If you use the OnMethodBoundaryAspect instead of the OnMethodInvocationAspect, you'll see that the modified code doesn't replace your method like it did here.  But it will add a lot of extra code to it as well.  All in all, there are a lot of possibilities here.  If i'm not mistaken, i can now even use our TraceAspect and use it on classes in other assemblies, even if i don't have access to them.  We'll look into that in a future post :)

Anyway, PostSharp is an extremely impressive piece of software which provides a whole lot of power and flexibility. You should definitely check it out and play around with it... i know i'm gonna be experimenting with it more often to see what kind of weird and cool stuff we can make it do :) 
