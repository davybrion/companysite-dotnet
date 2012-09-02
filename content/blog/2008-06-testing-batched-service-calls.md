Now that we can <a href="http://davybrion.com/blog/2008/06/batching-wcf-calls/">batch WCF calls</a>, and do so <a href="http://davybrion.com/blog/2008/06/the-service-call-batcher/">with readable code</a> we still need a way to test this.

Suppose that we have a page where we need to display a dropdown list of product categories and suppliers. This is the code in the controller of that screen for the Load event:

<code>
<div style="font-family: Consolas; font-size: 10pt; color: black; background: white;">
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span style="color: blue;">if</span> (!View.IsPostBack)</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; {</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span style="color: blue;">var</span> batcher = <span style="color: blue;">new</span> <span style="color: #2b91af;">ServiceCallBatcher</span>(service);</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; batcher.Add(<span style="color: #a31515;">"categories"</span>, <span style="color: blue;">new</span> <span style="color: #2b91af;">GetProductCategoriesRequest</span>());</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; batcher.Add(<span style="color: #a31515;">"suppliers"</span>, <span style="color: blue;">new</span> <span style="color: #2b91af;">GetSuppliersRequest</span>());</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; View.ProductCategories = batcher.Get&lt;<span style="color: #2b91af;">GetProductCategoriesResponse</span>&gt;(<span style="color: #a31515;">"categories"</span>).ProductCategories;</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; View.Suppliers = batcher.Get&lt;<span style="color: #2b91af;">GetSuppliersResponse</span>&gt;(<span style="color: #a31515;">"suppliers"</span>).Suppliers;</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; View.DataBind();</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; }</p>
</div>
</code>

When i'm testing my controllers, i like to use mocked service instances, and i set expectations on the methods that should be called and the parameters they receive.  With my batching technique, i don't really execute specific methods on the service because the batcher calls this method:

<code>
<div style="font-family: Consolas; font-size: 10pt; color: black; background: white;">
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span style="color: #2b91af;">Response</span>[] Process(<span style="color: blue;">params</span> <span style="color: #2b91af;">Request</span>[] requests); </p>
</div>
</code>

My Request classes are really simple, and to keep them simple they don't override the Equals method. This makes it hard to set expectations because the Request instances are created by the controller, and i can't set expectations on the Process method using Request instances that would equal the Request instances that were passed to the batcher by the controller. This makes it hard to test that the service is called correctly. If a Request type contains properties (which are really service method parameters) you really want to be able to test that those properties contain the correct values.  Also, you want to make sure that the correct Requests are sent to the service. But if they're handled by this very generic Process method, it makes it hard to verify correct usage during a test.

So how can we properly test the code listed above? I came up with the following approach. First we need a class that we can use to set up the Response instances to return from the service's Process method, and to capture the Request instances that are passed to the Process method:

<code>
<div style="font-family: Consolas; font-size: 10pt; color: black; background: white;">
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; <span style="color: blue;">public</span> <span style="color: blue;">class</span> <span style="color: #2b91af;">ServiceRequestResponseSpy</span></p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; {</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span style="color: blue;">private</span> <span style="color: #2b91af;">Request</span>[] receivedRequests;</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span style="color: blue;">private</span> <span style="color: #2b91af;">Response</span>[] responsesToReturn;</p>
<p style="margin: 0px;">&nbsp;</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span style="color: blue;">public</span> <span style="color: blue;">void</span> SetResponsesToReturn(<span style="color: blue;">params</span> <span style="color: #2b91af;">Response</span>[] responses)</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; {</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; responsesToReturn = responses;</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; }</p>
<p style="margin: 0px;">&nbsp;</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span style="color: blue;">public</span> T GetRequest&lt;T&gt;(<span style="color: blue;">int</span> index) <span style="color: blue;">where</span> T : <span style="color: #2b91af;">Request</span></p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; {</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span style="color: blue;">return</span> (T)receivedRequests[index];</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; }</p>
<p style="margin: 0px;">&nbsp;</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span style="color: blue;">public</span> <span style="color: #2b91af;">Response</span>[] GrabRequestsAndReturnGivenResponses(<span style="color: blue;">params</span> <span style="color: #2b91af;">Request</span>[] requests)</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; {</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; receivedRequests = requests;</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span style="color: blue;">return</span> responsesToReturn;</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; }</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; }</p>
</div>
</code>

Now we can write our test like this:

<code>
<div style="font-family: Consolas; font-size: 10pt; color: black; background: white;">
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; [<span style="color: #2b91af;">Test</span>]</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span style="color: blue;">public</span> <span style="color: blue;">void</span> RetrievesCategoriesAndSuppliersOnLoadIfNotPostBack()</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; {</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; view.Stub(v =&gt; v.IsPostBack).Return(<span style="color: blue;">false</span>);</p>
<p style="margin: 0px;">&nbsp;</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span style="color: blue;">var</span> categoriesToReturn = <span style="color: blue;">new</span> <span style="color: #2b91af;">ProductCategoryDTO</span>[0];</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span style="color: blue;">var</span> suppliersToReturn = <span style="color: blue;">new</span> <span style="color: #2b91af;">SupplierDTO</span>[0];</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span style="color: blue;">var</span> spy = <span style="color: blue;">new</span> <span style="color: #2b91af;">ServiceRequestResponseSpy</span>();</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; spy.SetResponsesToReturn(<span style="color: blue;">new</span> <span style="color: #2b91af;">GetProductCategoriesResponse</span>(categoriesToReturn), </p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span style="color: blue;">new</span> <span style="color: #2b91af;">GetSuppliersResponse</span>(suppliersToReturn));</p>
<p style="margin: 0px;">&nbsp;</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; service.Stub(s =&gt; s.Process(<span style="color: blue;">null</span>))</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; .IgnoreArguments()</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; .Do(<span style="color: blue;">new</span> <span style="color: #2b91af;">Func</span>&lt;<span style="color: #2b91af;">Request</span>[], <span style="color: #2b91af;">Response</span>[]&gt;(spy.GrabRequestsAndReturnGivenResponses));</p>
<p style="margin: 0px;">&nbsp;</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; view.Expect(v =&gt; v.ProductCategories = categoriesToReturn);</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; view.Expect(v =&gt; v.Suppliers = suppliersToReturn);</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; view.Expect(v =&gt; v.DataBind());</p>
<p style="margin: 0px;">&nbsp;</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; CreateController();</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; controller.Load();</p>
<p style="margin: 0px;">&nbsp;</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; view.VerifyAllExpectations();</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span style="color: #2b91af;">Assert</span>.IsNotNull(spy.GetRequest&lt;<span style="color: #2b91af;">GetProductCategoriesRequest</span>&gt;(0));</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span style="color: #2b91af;">Assert</span>.IsNotNull(spy.GetRequest&lt;<span style="color: #2b91af;">GetSuppliersRequest</span>&gt;(1));</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; }</p>
</div>
</code>

The important part of the code is this one:

<code>
<div style="font-family: Consolas; font-size: 10pt; color: black; background: white;">
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; service.Stub(s =&gt; s.Process(<span style="color: blue;">null</span>))</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; .IgnoreArguments()</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; .Do(<span style="color: blue;">new</span> <span style="color: #2b91af;">Func</span>&lt;<span style="color: #2b91af;">Request</span>[], <span style="color: #2b91af;">Response</span>[]&gt;(spy.GrabRequestsAndReturnGivenResponses));</p>
</div>
</code>

This basically tells Rhino Mocks to execute the spy's GrabRequestsAndReturnGivenRepsonses method whenever the mocked service instance's Process method is called, no matter what arguments are passed to it.

So we basically set up the mocked service to delegate the call to Process to the GrabRequestsAndReturnGivenRepsonses method, and then we set expectations on how the returned data should be passed to the view.  After that we execute the Controller's Load method, and we verify that the view's expectations were met, and we test that the spy captured Request instances of the expected types.
