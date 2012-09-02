Just read a <a href="http://www.bartonline.be/2008/04/19/Unit+Test+Exceptions.aspx">post</a> from a former co-worker about how to test exceptions. He uses the ExpectedException attribute (of MS Test) which has at least the following issues (IMO):

<ul>
	<li>There is some confusion about the message parameter in the attribute. In NUnit, the message you provide is the expected exception message that you want to test. With MS Test, it is the message that should be displayed when the exception is <b>not</b> thrown.  This effectively removes the possibility to use the attribute to test the message of the exception when using MS Test. And yes, it can definitely be useful to test the content of the message.</li>
	<li>It is not immediately clear which line of code is supposed to throw the exception. In the example he provides, when you only have 2 lines of code (one of which is instantiating the object) it's not hard to figure out where the exception is thrown. But when you have larger test methods, it's often confusing to see where the exception should be thrown. And yes, we all like to avoid large test methods, but sometimes it's hard to avoid.  And with large, i mean 10+ lines.</li>
	<li>You can't test for the general Exception type. Whether it is a good idea or not to throw a general Exception is not really relevant to this discussion. But i do want to be able to test for it when the situation calls for it</li>
	<li>If you have a custom exception with contains some properties that you want to test, using the ExpectedException attribute is insufficiant... yes, you can test that the exception is thrown, but if the exception has extra properties, the contents of those properties should be validated as well</li>
</ul>

Let's use his Order example, but with some more logic, to show some better ways to test exceptions. Suppose we have the following code:

<div style="font-family:Consolas;font-size:10pt;color:black;background:white;">
<p style="margin:0;">&nbsp;&nbsp;&nbsp; <span style="color:blue;">public</span> <span style="color:blue;">class</span> <span style="color:#2b91af;">Order</span></p>
<p style="margin:0;">&nbsp;&nbsp;&nbsp; {</p>
<p style="margin:0;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span style="color:blue;">public</span> <span style="color:#2b91af;">IEnumerable</span>&lt;<span style="color:#2b91af;">OrderLine</span>&gt; OrderLines { <span style="color:blue;">get</span>; <span style="color:blue;">set</span>; }</p>
<p style="margin:0;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span style="color:blue;">public</span> <span style="color:#2b91af;">Customer</span> Customer { <span style="color:blue;">get</span>; <span style="color:blue;">set</span>; }</p>
<p style="margin:0;">&nbsp;</p>
<p style="margin:0;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span style="color:blue;">public</span> Order() : <span style="color:blue;">this</span>(<span style="color:blue;">null</span>, <span style="color:blue;">null</span>) {}</p>
<p style="margin:0;">&nbsp;</p>
<p style="margin:0;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span style="color:blue;">public</span> Order(<span style="color:#2b91af;">Customer</span> customer) : <span style="color:blue;">this</span>(customer, <span style="color:blue;">null</span>) {}</p>
<p style="margin:0;">&nbsp;</p>
<p style="margin:0;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span style="color:blue;">public</span> Order(<span style="color:#2b91af;">Customer</span> customer, <span style="color:#2b91af;">IEnumerable</span>&lt;<span style="color:#2b91af;">OrderLine</span>&gt; orderLines)</p>
<p style="margin:0;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; {</p>
<p style="margin:0;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; Customer = customer;</p>
<p style="margin:0;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; OrderLines = orderLines;</p>
<p style="margin:0;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; }</p>
<p style="margin:0;">&nbsp;</p>
<p style="margin:0;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span style="color:blue;">public</span> <span style="color:blue;">decimal</span> CalculateTotal()</p>
<p style="margin:0;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; {</p>
<p style="margin:0;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span style="color:blue;">return</span> OrderLines.Sum(o =&gt; o.Price) * Customer.DiscountRate;</p>
<p style="margin:0;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; }</p>
<p style="margin:0;">&nbsp;&nbsp;&nbsp; }</p>
</div>

For maximum flexibility, we've provided 3 ways to create an Order object: without any of its dependencies, with one dependency (Customer) and with both dependencies (Customer and OrderLines). Depending on which constructor you've used, you might need to provide one or two dependencies through their setters before you can call the CalculateTotal method.  With the code as it is right now, we'll get a NullReferenceException if one of the dependencies hasn't been provided. So we'll modify the CalculateTotal method to guard against this.  I don't like creating Exception-derived types for everything that could possibly go wrong, so i'll define an enum with the validation problems that the Order class could have:

<div style="font-family:Consolas;font-size:10pt;color:black;background:white;">
<p style="margin:0;">&nbsp;&nbsp;&nbsp; <span style="color:blue;">public</span> <span style="color:blue;">enum</span> <span style="color:#2b91af;">OrderValidationProblem</span></p>
<p style="margin:0;">&nbsp;&nbsp;&nbsp; {</p>
<p style="margin:0;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; OrderLinesNotSet,</p>
<p style="margin:0;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; OrderLinesHasNoItems,</p>
<p style="margin:0;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; CustomerNotSet</p>
<p style="margin:0;">&nbsp;&nbsp;&nbsp; }</p>
</div>

Now we introduce an OrderValidationException type which will contain the type of validation problem:

<div style="font-family:Consolas;font-size:10pt;color:black;background:white;">
<p style="margin:0;">&nbsp;&nbsp;&nbsp; <span style="color:blue;">public</span> <span style="color:blue;">class</span> <span style="color:#2b91af;">OrderValidationException</span> : <span style="color:#2b91af;">Exception</span></p>
<p style="margin:0;">&nbsp;&nbsp;&nbsp; {</p>
<p style="margin:0;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span style="color:blue;">public</span> <span style="color:#2b91af;">OrderValidationProblem</span> Problem { <span style="color:blue;">get</span>; <span style="color:blue;">set</span>; }</p>
<p style="margin:0;">&nbsp;</p>
<p style="margin:0;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span style="color:blue;">public</span> OrderValidationException(<span style="color:#2b91af;">OrderValidationProblem</span> problem)</p>
<p style="margin:0;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; {</p>
<p style="margin:0;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; Problem = problem;</p>
<p style="margin:0;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; }</p>
<p style="margin:0;">&nbsp;&nbsp;&nbsp; }</p>
</div>

Since we try to be responsible programmers, we immediately write the tests so we can't ever forget to properly guard against these conditions:

<div style="font-family:Consolas;font-size:10pt;color:black;background:white;">
<p style="margin:0;">&nbsp;&nbsp;&nbsp; [<span style="color:#2b91af;">TestFixture</span>]</p>
<p style="margin:0;">&nbsp;&nbsp;&nbsp; <span style="color:blue;">public</span> <span style="color:blue;">class</span> <span style="color:#2b91af;">OrderValidationTests</span></p>
<p style="margin:0;">&nbsp;&nbsp;&nbsp; {</p>
<p style="margin:0;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; [<span style="color:#2b91af;">Test</span>]</p>
<p style="margin:0;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; [<span style="color:#2b91af;">ExpectedException</span>(<span style="color:blue;">typeof</span>(<span style="color:#2b91af;">OrderValidationException</span>))]</p>
<p style="margin:0;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span style="color:blue;">public</span> <span style="color:blue;">void</span> CustomerNotSetThrowsValidationException()</p>
<p style="margin:0;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; {</p>
<p style="margin:0;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span style="color:blue;">var</span> order = <span style="color:blue;">new</span> <span style="color:#2b91af;">Order</span>();</p>
<p style="margin:0;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; order.OrderLines = <span style="color:#2b91af;">EntityTestFactory</span>.CreateDummyOrderLines();</p>
<p style="margin:0;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; order.CalculateTotal();</p>
<p style="margin:0;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; }</p>
<p style="margin:0;">&nbsp;</p>
<p style="margin:0;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; [<span style="color:#2b91af;">Test</span>]</p>
<p style="margin:0;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; [<span style="color:#2b91af;">ExpectedException</span>(<span style="color:blue;">typeof</span>(<span style="color:#2b91af;">OrderValidationException</span>))]</p>
<p style="margin:0;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span style="color:blue;">public</span> <span style="color:blue;">void</span> OrderLinesNotSetThrowsValidationException()</p>
<p style="margin:0;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; {</p>
<p style="margin:0;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span style="color:blue;">var</span> order = <span style="color:blue;">new</span> <span style="color:#2b91af;">Order</span>(<span style="color:blue;">new</span> <span style="color:#2b91af;">Customer</span>());</p>
<p style="margin:0;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; order.CalculateTotal();</p>
<p style="margin:0;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; }</p>
<p style="margin:0;">&nbsp;</p>
<p style="margin:0;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; [<span style="color:#2b91af;">Test</span>]</p>
<p style="margin:0;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; [<span style="color:#2b91af;">ExpectedException</span>(<span style="color:blue;">typeof</span>(<span style="color:#2b91af;">OrderValidationException</span>))]</p>
<p style="margin:0;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span style="color:blue;">public</span> <span style="color:blue;">void</span> OrderLinesWithNoItemsThrowsValidationException()</p>
<p style="margin:0;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; {</p>
<p style="margin:0;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span style="color:blue;">var</span> order = <span style="color:blue;">new</span> <span style="color:#2b91af;">Order</span>(<span style="color:blue;">new</span> <span style="color:#2b91af;">Customer</span>(), <span style="color:blue;">new</span> <span style="color:#2b91af;">OrderLine</span>[] {});</p>
<p style="margin:0;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; order.CalculateTotal();</p>
<p style="margin:0;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; }</p>
<p style="margin:0;">&nbsp;&nbsp;&nbsp; }</p>
</div>

Now it's time to make the tests pass... so we modify the CalculateTotal method to provide the necessary guard clauses:

<div style="font-family:Consolas;font-size:10pt;color:black;background:white;">
<p style="margin:0;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span style="color:blue;">public</span> <span style="color:blue;">decimal</span> CalculateTotal()</p>
<p style="margin:0;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; {</p>
<p style="margin:0;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span style="color:blue;">if</span> (OrderLines == <span style="color:blue;">null</span>) <span style="color:blue;">throw</span> <span style="color:blue;">new</span> <span style="color:#2b91af;">OrderValidationException</span>(<span style="color:#2b91af;">OrderValidationProblem</span>.OrderLinesNotSet);</p>
<p style="margin:0;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span style="color:blue;">if</span> (OrderLines.Count() == 0) <span style="color:blue;">throw</span> <span style="color:blue;">new</span> <span style="color:#2b91af;">OrderValidationException</span>(<span style="color:#2b91af;">OrderValidationProblem</span>.OrderLinesHasNoItems);</p>
<p style="margin:0;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span style="color:blue;">if</span> (Customer == <span style="color:blue;">null</span>) <span style="color:blue;">throw</span> <span style="color:blue;">new</span> <span style="color:#2b91af;">OrderValidationException</span>(<span style="color:#2b91af;">OrderValidationProblem</span>.CustomerNotSet);</p>
<p style="margin:0;">&nbsp;</p>
<p style="margin:0;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span style="color:blue;">return</span> OrderLines.Sum(o =&gt; o.Price) * Customer.DiscountRate;</p>
<p style="margin:0;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; }</p>
</div>

At this point, the tests pass... but what do they prove? Sure, we throw the right exception when we need to. But we still don't know if the exception has been constructed properly. If we want to test that, we have to stop using the ExpectedException attribute. But i don't wan't to litter my tests with try/catch constructs either. We could create a helper method to perform the necessary check:

<div style="font-family:Consolas;font-size:10pt;color:black;background:white;">
<p style="margin:0;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span style="color:blue;">private</span> <span style="color:blue;">void</span> CheckExceptionAndProblem(<span style="color:#2b91af;">Func</span>&lt;<span style="color:#2b91af;">Decimal</span>&gt; function, </p>
<p style="margin:0;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span style="color:#2b91af;">OrderValidationProblem</span> expectedOrderValidationProblem)</p>
<p style="margin:0;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; {</p>
<p style="margin:0;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span style="color:blue;">try</span></p>
<p style="margin:0;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; {</p>
<p style="margin:0;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; function();</p>
<p style="margin:0;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; }</p>
<p style="margin:0;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span style="color:blue;">catch</span> (<span style="color:#2b91af;">OrderValidationException</span> e)</p>
<p style="margin:0;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; {</p>
<p style="margin:0;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span style="color:#2b91af;">Assert</span>.AreEqual(expectedOrderValidationProblem, e.Problem);</p>
<p style="margin:0;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span style="color:blue;">return</span>;</p>
<p style="margin:0;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; }</p>
<p style="margin:0;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span style="color:#2b91af;">Assert</span>.Fail(<span style="color:#a31515;">"Exception was not thrown"</span>);</p>
<p style="margin:0;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; }</p>
</div>

The generic Func type allows us to easily pass a delegate to the CalculateTotal method instead of having to declare a specific delegate for it first.

Then we'd modify our tests like this:

<div style="font-family:Consolas;font-size:10pt;color:black;background:white;">
<p style="margin:0;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; [<span style="color:#2b91af;">Test</span>]</p>
<p style="margin:0;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span style="color:blue;">public</span> <span style="color:blue;">void</span> CustomerNotSetThrowsValidationException()</p>
<p style="margin:0;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; {</p>
<p style="margin:0;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span style="color:blue;">var</span> order = <span style="color:blue;">new</span> <span style="color:#2b91af;">Order</span>();</p>
<p style="margin:0;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; order.OrderLines = <span style="color:#2b91af;">EntityTestFactory</span>.CreateDummyOrderLines();</p>
<p style="margin:0;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; CheckExceptionAndProblem(order.CalculateTotal, <span style="color:#2b91af;">OrderValidationProblem</span>.CustomerNotSet);</p>

<p style="margin:0;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; }</p>
<p style="margin:0;">&nbsp;</p>
<p style="margin:0;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; [<span style="color:#2b91af;">Test</span>]</p>
<p style="margin:0;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span style="color:blue;">public</span> <span style="color:blue;">void</span> OrderLinesNotSetThrowsValidationException()</p>
<p style="margin:0;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; {</p>
<p style="margin:0;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span style="color:blue;">var</span> order = <span style="color:blue;">new</span> <span style="color:#2b91af;">Order</span>(<span style="color:blue;">new</span> <span style="color:#2b91af;">Customer</span>());</p>
<p style="margin:0;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; CheckExceptionAndProblem(order.CalculateTotal, <span style="color:#2b91af;">OrderValidationProblem</span>.OrderLinesNotSet);</p>
<p style="margin:0;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; }</p>
<p style="margin:0;">&nbsp;</p>
<p style="margin:0;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; [<span style="color:#2b91af;">Test</span>]</p>
<p style="margin:0;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span style="color:blue;">public</span> <span style="color:blue;">void</span> OrderLinesWithNoItemsThrowsValidationException()</p>
<p style="margin:0;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; {</p>
<p style="margin:0;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span style="color:blue;">var</span> order = <span style="color:blue;">new</span> <span style="color:#2b91af;">Order</span>(<span style="color:blue;">new</span> <span style="color:#2b91af;">Customer</span>(), <span style="color:blue;">new</span> <span style="color:#2b91af;">OrderLine</span>[] {});</p>
<p style="margin:0;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; CheckExceptionAndProblem(order.CalculateTotal, <span style="color:#2b91af;">OrderValidationProblem</span>.OrderLinesHasNoItems);</p>
<p style="margin:0;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; }</p>
</div>

That's already much better i think... We can test that the exception is thrown, and that its Problem property is set correctly, and we only have one try/catch clause in our tests.

I'm still not happy with it though... The CheckExceptionAndProblem method is not reusable for anything other than OrderValidation, yet testing exceptions is a common problem so we should strive to provide something that'll help us anytime we need to test exceptions.

How about a custom assert method that asserts that any piece of code that is passed to it throws the expected exception, and then returns the exception so you can easily assert anything else you wanna check in the returned exception? Sounds pretty good to me... let's give it a shot:

<div style="font-family:Consolas;font-size:10pt;color:black;background:white;">
<p style="margin:0;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span style="color:blue;">private</span> T GetThrownException&lt;T&gt;(<span style="color:#2b91af;">Action</span> code) <span style="color:blue;">where</span> T : <span style="color:#2b91af;">Exception</span></p>
<p style="margin:0;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; {</p>
<p style="margin:0;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span style="color:blue;">try</span></p>
<p style="margin:0;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; {</p>
<p style="margin:0;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; code();</p>
<p style="margin:0;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; }</p>
<p style="margin:0;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span style="color:blue;">catch</span> (T expectedException)</p>
<p style="margin:0;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; {</p>
<p style="margin:0;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span style="color:blue;">return</span> expectedException;</p>
<p style="margin:0;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; }</p>




<p style="margin:0;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span style="color:blue;">catch</span> (<span style="color:#2b91af;">Exception</span> e) {}</p>
<p style="margin:0;">&nbsp;</p>
<p style="margin:0;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span style="color:#2b91af;">Assert</span>.Fail(<span style="color:#a31515;">"Expected exception of type {0} was not thrown"</span>, <span style="color:blue;">typeof</span>(T).FullName);</p>
<p style="margin:0;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span style="color:blue;">return</span> <span style="color:blue;">null</span>;</p>
<p style="margin:0;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; }</p>
</div>

This method runs the code that was passed in, catches the expected exception and returns it. If the expected exception is not caught, it fails the current test.

Now we can modify our CheckExceptionAndProblem method so it looks like this:

<div style="font-family:Consolas;font-size:10pt;color:black;background:white;">
<p style="margin:0;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span style="color:blue;">private</span> <span style="color:blue;">void</span> CheckExceptionAndProblem(<span style="color:#2b91af;">Func</span>&lt;<span style="color:#2b91af;">Decimal</span>&gt; function, </p>
<p style="margin:0;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span style="color:#2b91af;">OrderValidationProblem</span> expectedOrderValidationProblem)</p>
<p style="margin:0;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; {</p>
<p style="margin:0;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span style="color:blue;">var</span> expectedException = GetThrownException&lt;<span style="color:#2b91af;">OrderValidationException</span>&gt;(() =&gt; function());</p>
<p style="margin:0;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span style="color:#2b91af;">Assert</span>.AreEqual(expectedOrderValidationProblem, expectedException.Problem);</p>
<p style="margin:0;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; }</p>
</div>

Now we can use the GetThrownException method pretty much anywhere where want to test exceptions and their properties.

Anyways, this is just one possible approach of testing exceptions in a much better way than the ExpectedException attribute offers us.

Btw, i think the xUnit.Net testing framework already provides similar approaches to what i used in this post
