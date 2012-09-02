The usual example:

<code>
<div style="font-family: Consolas; font-size: 10pt; color: black; background: white;">
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; dispatcher.Add(<span style="color: blue;">new</span> <span style="color: #2b91af;">GetProductCategoriesRequest</span>(), <span style="color: blue;">new</span> <span style="color: #2b91af;">GetSuppliersRequest</span>());</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; View.ProductCategories = dispatcher.Get&lt;<span style="color: #2b91af;">GetProductCategoriesResponse</span>&gt;().ProductCategories;</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; View.Suppliers = dispatcher.Get&lt;<span style="color: #2b91af;">GetSuppliersResponse</span>&gt;().Suppliers;</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; View.DataBind();</p>
</div>
</code>

Thanks to my <a href="http://davybrion.com/blog/2008/07/the-request-response-service-layer/">request/response service layer</a>, those are two service requests that will be performed in one service call.  On the server side, these two requests are dealt with separately.  But they merely retrieve data from the database.  Wouldn't it be cool if those 2 queries were performed in one database roundtrip instead of two?  Wouldn't it be cool if we could batch the queries that will be performed by read-only service requests into a single database roundtrip? It sure as hell would be. Implementing this was one of those irresistible challenges you just can't say no to, so from now on we can actually do this.  I'm not gonna get into the actual implementation of how I got it working (check the code in the library for that if you're interested), but i will show you how you can do this in your application.

First of all, if you have requests that merely return data, inherit from ReadOnlyRequest instead of the usual Request type:

<code>
<div style="font-family: Consolas; font-size: 10pt; color: black; background: white;">
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; [<span style="color: #2b91af;">Serializable</span>]</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; <span style="color: blue;">public</span> <span style="color: blue;">class</span> <span style="color: #2b91af;">GetProductCategoriesRequest</span> : <span style="color: #2b91af;">ReadOnlyRequest</span> {}</p>
<p style="margin: 0px;">&nbsp;</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; [<span style="color: #2b91af;">Serializable</span>]</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; <span style="color: blue;">public</span> <span style="color: blue;">class</span> <span style="color: #2b91af;">GetSuppliersRequest</span> : <span style="color: #2b91af;">ReadOnlyRequest</span> {}</p>
</div>
</code>

The handlers for these requests should inherit from the ReadOnlyRequestHandler base class instead of the usual RequestHandler class.  Btw, my ReadOnlyRequestHandler class inherits from my UoWRequestHandler class, which requires a IUnitOfWork instance to be passed to the constructor.  So we need to put an IUnitOfWork parameter into the constructor along with our other dependencies if we want the IOC container (or the request processor) to pass us a valid IUnitOfWork instance.

<code>
<div style="font-family: Consolas; font-size: 10pt; color: black; background: white;">
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; <span style="color: blue;">public</span> <span style="color: blue;">class</span> <span style="color: #2b91af;">GetProductCategoriesHandler</span> : <span style="color: #2b91af;">ReadOnlyRequestHandler</span>&lt;<span style="color: #2b91af;">GetProductCategoriesRequest</span>&gt;</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; {</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span style="color: blue;">private</span> <span style="color: blue;">readonly</span> <span style="color: #2b91af;">IRepository</span>&lt;<span style="color: #2b91af;">ProductCategory</span>&gt; repository;</p>
<p style="margin: 0px;">&nbsp;</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span style="color: blue;">public</span> GetProductCategoriesHandler(<span style="color: #2b91af;">IUnitOfWork</span> unitOfWork, <span style="color: #2b91af;">IRepository</span>&lt;<span style="color: #2b91af;">ProductCategory</span>&gt; repository) </p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; : <span style="color: blue;">base</span>(unitOfWork)</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; {</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span style="color: blue;">this</span>.repository = repository;</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; }</p>
<p style="margin: 0px;">&nbsp;</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span style="color: blue;">public</span> <span style="color: blue;">override</span> <span style="color: blue;">void</span> AddQueries(<span style="color: #2b91af;">IQueryBatcher</span> queryBatcher, <span style="color: #2b91af;">GetProductCategoriesRequest</span> request)</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; {</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; queryBatcher.AddCriteria(<span style="color: #a31515;">"categories"</span>, repository.CriteriaForAll());</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; }</p>
<p style="margin: 0px;">&nbsp;</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span style="color: blue;">public</span> <span style="color: blue;">override</span> <span style="color: #2b91af;">Response</span> GetResults(<span style="color: #2b91af;">IQueryBatcher</span> queryBatcher, <span style="color: #2b91af;">GetProductCategoriesRequest</span> request)</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; {</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span style="color: blue;">var</span> categories = queryBatcher.GetEnumerableResult&lt;<span style="color: #2b91af;">ProductCategory</span>&gt;(<span style="color: #a31515;">"categories"</span>);</p>
<p style="margin: 0px;">&nbsp;</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span style="color: blue;">return</span> <span style="color: blue;">new</span> <span style="color: #2b91af;">GetProductCategoriesResponse</span> { ProductCategories = categories.ToDTOs() };</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; }</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; }</p>
</div>
</code>

The ReadOnlyRequestHandler class defines two abstract methods: the AddQueries and GetResults method.  Both are passed an instance of IQueryBatcher and the actual request object so you can use the request parameters if there are any.

In the AddQueries method, you simply add the queries this request needs to perform to the batcher (with a string key value of course for later retrieval), and in the GetResults method you can get the results (with the key values used in AddQueries) back from the batcher and you can construct your Response object.  The GetResults method will be called after each ReadOnlyRequestHandler for the current request batch has added the queries to be performed to the batcher.  The queries are then all executed, and each ReadOnlyRequestHandler will have its GetResults method called so each handler can construct the proper response.

And that's it basically... The calling code doesn't even change. 

Now there are obviously some limitations. If you mix ReadOnlyRequests with other Requests, then each ReadOnlyRequest will be handled as if it were a regular Request and they will each get their very own IQueryBatcher instance.  I don't think there's any strategy to automagically determine which ReadOnlyRequests can be batched together while still correctly executing the other Requests since those requests might influence the results of ReadOnlyRequests that are present further in the batch.

Anyways, you can find an updated version of the library <a href="http://davybrion.com/blog/stuff/">here</a>.  Check it out, it's some pretty slick shit, if i do say so myself ;)

Note: i only added this stuff today, so the previous build (080729) doesn't have this yet, in case you only downloaded that today, and the stats show that some of you actually did download it ;)