Manually dealing with KnownTypes in WCF is a bit of a pain. Well, at least if you have more than a few derived types you want your WCF services to serialize/deserialize.  Luckily for us, there is a way to do this automatically.  Using the <a href="http://msdn.microsoft.com/en-us/library/system.servicemodel.serviceknowntypeattribute.aspx">ServiceKnownType</a> attribute you can specify a class and a static method of that class which will provide the Known Types.  An example of this can be found on my ever-recurring Process service method:

<code>
<div style="font-family: Consolas; font-size: 10pt; color: black; background: white;">
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; [<span style="color: #2b91af;">OperationContract</span>]</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; [<span style="color: #2b91af;">ServiceKnownType</span>(<span style="color: #a31515;">"GetKnownTypes"</span>, <span style="color: blue;">typeof</span>(<span style="color: #2b91af;">KnownTypeProvider</span>))]</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span style="color: #2b91af;">Response</span>[] Process(<span style="color: blue;">params</span> <span style="color: #2b91af;">Request</span>[] requests); </p>
</div>
</code>

This works, but i don't want to write a class every time i want to specify some known types of a base type in a service method.  So i modified that KnownTypeProvider class so that we can now do the following:

<code>
<div style="font-family: Consolas; font-size: 10pt; color: black; background: white;">
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span style="color: #2b91af;">KnownTypeProvider</span>.RegisterDerivedTypesOf&lt;<span style="color: #2b91af;">Request</span>&gt;(mySharedAssembly);</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span style="color: #2b91af;">KnownTypeProvider</span>.RegisterDerivedTypesOf&lt;<span style="color: #2b91af;">Response</span>&gt;(mySharedAssembly);</p>
</div>
</code>

This registers each derived type of Request and Response in the mySharedAssembly reference (of type Assembly) with the KnownTypeProvider.  You can register as many known types for as many base types as you want.  Obviously, you have to do this before you start hosting your service. You also need to do this client-side, before you start using your service proxies.

You're probably thinking "wait, won't that return all registered known types for every service call where you use this stuff?".  That wouldn't be good, so i took care of that. The ServiceKnownType attribute of WCF requires your class to implement a method which receives a parameter of type ICustomAttributeProvider and which returns a list of types.  The ICustomAttributeProvider parameter is actually a MethodInfo instance at runtime.  This makes it possible to inspect the current service method's parameters and return type.  That information makes it possible to only return the KnownTypes that are relevant to the current service call.  All of this sounds expensive with regards to performance, so obviously this is all cached... so you only take the performance hit on the very first request of each service method where you use this.

The KnownTypeProvider makes it extremely easy to register your KnownTypes in a generic way, can be reused across multiple service methods and it only returns the relevant KnownTypes for each service method.  And it does so with a minimum of performance overhead. 

You can find the code in my <a href="http://davybrion.com/publicsvn/Brion.Library/Brion.Library.Common/WCF/KnownTypeProvider.cs">public svn repository</a> as a part of my utility library (the tests can be found <a href="http://davybrion.com/publicsvn/Brion.Library/Brion.Library.Tests/Common/WCF/KnownTypeProviderTests.cs">here</a>).  At this moment it is one of the only classes in the library, because i still have to add some other stuff before i'll put a 'proper release' online :)
