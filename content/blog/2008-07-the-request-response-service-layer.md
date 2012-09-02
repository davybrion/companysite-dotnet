Update: i also have a complete <a href="http://davybrion.com/blog/2009/11/requestresponse-service-layer-series/">series of posts</a> on this which you may find interesting.

<h2>Introduction</h2>

When you're trying to design a service layer API, there are two things you always need to be very careful of. The first is making sure the API isn't chatty. A service layer is usually deployed on another physical machine than the client layer, so you want clients to be able to do everything they need to do with a minimum of roundtrips, because those are after all rather expensive.  The second thing you want to avoid is the implicit coupling between the client of the service and the service itself.  In your quest to avoid the chatty interface, you might start grouping several service operations together in more coarse-grained operations to avoid the chatty communication that might otherwise occur.  This can be good from a performance point of view, but it usually introduces a certain implicit coupling between the service and the client, because a specific grouping of operations might be beneficial to one client, but might be completely inappropriate to another type of client of the service.

I really had difficulties coming up with an approach that offered nice coarse-grained service interfaces while at the same time making sure the interfaces weren't too 'driven' by a client's specific requirements.  So i basically wanted a way to keep my service operations as specific as they could be (very fine-grained), while avoiding the pitfall of chatty communication by batching calls to the service layer together whenever it makes sense <strong>from the client's point of view</strong>.  I eventually ended up with an approach that i've already documented <a href="http://davybrion.com/blog/2008/06/batching-wcf-calls/">here</a> and <a href="http://davybrion.com/blog/2008/07/batching-wcf-calls-take-2/">here</a>.  Read those posts first before continuing with this post because the rest of this post builds upon the content of the previous posts.

The idea is to basically consider each <em>service operation</em> as a <em>request</em> which can have a <em>response</em>. For each request that you define, you need to provide a handler which does whatever it needs to do to handle the request and returns a response, or an empty response if no response is needed for a specific request.  You then only need one service method which receives incoming requests, delegates them to the proper handlers, and returns each response in the order of the incoming requests. On a side note: this approach probably doesn't sit well with many SOA people, but then again, i couldn't care less about SOA. I just want good software no matter what acronym i can use to describe it.

Anyway, i like this approach so much that i wanted to make it available to whoever is interested in it.  You can find it in my open source library which you can find <a href="http://davybrion.com/blog/stuff/">here</a>. I won't go into the details of the implementation, because those have mostly been covered already in my previous posts on this subject.  The rest of this post focuses solely on how you can use this approach in your projects. 

<h2>Defining Requests and Responses</h2>

The following two base classes need to be used to define request and response types:

<code>
<div style="font-family: Consolas; font-size: 10pt; color: black; background: white;">
<p style="margin: 0px;"><span style="color: blue;">namespace</span> Brion.Library.Common.Messaging</p>
<p style="margin: 0px;">{</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; [<span style="color: #2b91af;">Serializable</span>]</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; <span style="color: blue;">public</span> <span style="color: blue;">abstract</span> <span style="color: blue;">class</span> <span style="color: #2b91af;">Request</span> {}</p>
<p style="margin: 0px;">&nbsp;</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; [<span style="color: #2b91af;">Serializable</span>]</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; <span style="color: blue;">public</span> <span style="color: blue;">abstract</span> <span style="color: blue;">class</span> <span style="color: #2b91af;">Response</span> { }</p>
<p style="margin: 0px;">&nbsp;</p>
<p style="margin: 0px;">}</p>
</div>
</code>

Suppose you have a service operation that retrieves a list of products based on some parameters the user could provide in the UI. You would define the request like this:

<code>
<div style="font-family: Consolas; font-size: 10pt; color: black; background: white;">
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; [<span style="color: #2b91af;">Serializable</span>]</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; <span style="color: blue;">public</span> <span style="color: blue;">class</span> <span style="color: #2b91af;">GetProductOverviewsRequest</span> : <span style="color: #2b91af;">Request</span></p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; {</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span style="color: blue;">public</span> <span style="color: blue;">string</span> NamePattern { <span style="color: blue;">get</span>; <span style="color: blue;">set</span>; }</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span style="color: blue;">public</span> <span style="color: blue;">int</span>? CategoryId { <span style="color: blue;">get</span>; <span style="color: blue;">set</span>; }</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span style="color: blue;">public</span> <span style="color: blue;">int</span>? SupplierId { <span style="color: blue;">get</span>; <span style="color: blue;">set</span>; }</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; }</p>
</div>
</code>

And the response would look like this:

<code>
<div style="font-family: Consolas; font-size: 10pt; color: black; background: white;">
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; [<span style="color: #2b91af;">Serializable</span>]</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; <span style="color: blue;">public</span> <span style="color: blue;">class</span> <span style="color: #2b91af;">GetProductOverviewsResponse</span> : <span style="color: #2b91af;">Response</span></p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; {</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span style="color: blue;">public</span> <span style="color: #2b91af;">ProductOverviewDTO</span>[] Products { <span style="color: blue;">get</span>; <span style="color: blue;">set</span>; }</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; }</p>
</div>
</code>

Keep in mind that these types always need to be marked with the Serializable attribute and that these types need to be in an assembly that you can share between both the client and the service.

<h2>Handling Requests</h2>

Incoming requests are handled by a class which implements the IRequestProcessor interface:

<code>
<div style="font-family: Consolas; font-size: 10pt; color: black; background: white;">
<p style="margin: 0px;"><span style="color: blue;">namespace</span> Brion.Library.Common.Messaging</p>
<p style="margin: 0px;">{</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; <span style="color: blue;">public</span> <span style="color: blue;">interface</span> <span style="color: #2b91af;">IRequestProcessor</span> : <span style="color: #2b91af;">IDisposable</span></p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; {</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span style="color: #2b91af;">Response</span>[] Process(<span style="color: blue;">params</span> <span style="color: #2b91af;">Request</span>[] requests);</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; }</p>
<p style="margin: 0px;">}</p>
</div>
</code>

For each request that is received, the request processor will create a request handler which must be of the type IRequestHandler&lt;TRequest&gt; where TRequest is the type of the request instance. The request object is then passed along to the handler's Handle method, which will return a proper Response object.

The simplest way to create a request handler is to inherit from the RequestHandler&lt;TRequest&gt; class, like this:

<code>
<div style="font-family: Consolas; font-size: 10pt; color: black; background: white;">
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; <span style="color: blue;">public</span> <span style="color: blue;">class</span> <span style="color: #2b91af;">GetProductOverviewsHandler</span> : </p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; Brion.Library.ServerSide.Messaging.<span style="color: #2b91af;">RequestHandler</span>&lt;<span style="color: #2b91af;">GetProductOverviewsRequest</span>&gt;</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; {</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span style="color: blue;">public</span> <span style="color: #2b91af;">IProductRepository</span> ProductRepository { <span style="color: blue;">get</span>; <span style="color: blue;">private</span> <span style="color: blue;">set</span>; }</p>
<p style="margin: 0px;">&nbsp;</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span style="color: blue;">public</span> GetProductOverviewsHandler(<span style="color: #2b91af;">IProductRepository</span> productRepository) </p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; {</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; ProductRepository = productRepository;</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; }</p>
<p style="margin: 0px;">&nbsp;</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span style="color: blue;">public</span> <span style="color: blue;">override</span> <span style="color: #2b91af;">Response</span> Handle(<span style="color: #2b91af;">GetProductOverviewsRequest</span> request)</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; {</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span style="color: blue;">var</span> products = ProductRepository.FindAll(request.NamePattern, request.SupplierId, </p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; request.CategoryId);</p>
<p style="margin: 0px;">&nbsp;</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span style="color: blue;">return</span> <span style="color: blue;">new</span> <span style="color: #2b91af;">GetProductOverviewsResponse</span> { Products = products.ToOverviewDTOs() };</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; }</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; }</p>
</div>
</code>

The request processor uses the Castle Windsor Inversion Of Control container to resolve and create the instances of the request handler types. This allows you to use <a href="http://davybrion.com/blog/2007/07/introduction-to-dependency-injection/">dependency injection</a> for your request handlers. In the example posted above, you can see that the constructor of the handler takes an IProductRepository instance. Because the Windsor container also knows about the IProductRepository component in my application, it can correctly instantiate the GetProductOverviewsHandler instance with a valid IProductRepository instance.  Keep in mind that this is optional though. You don't have to use dependency injection for your request handlers if you don't want to. 

If you do want to use it, your application will have to use the same Windsor container instance as this library does. To make this possible, the library contains the following class:

<code>
<div style="font-family: Consolas; font-size: 10pt; color: black; background: white;">
<p style="margin: 0px;"><span style="color: blue;">namespace</span> Brion.Library.Common</p>
<p style="margin: 0px;">{</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; <span style="color: blue;">public</span> <span style="color: blue;">static</span> <span style="color: blue;">class</span> <span style="color: #2b91af;">IoC</span></p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; {</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span style="color: blue;">private</span> <span style="color: blue;">static</span> <span style="color: #2b91af;">IWindsorContainer</span> container = <span style="color: blue;">new</span> <span style="color: #2b91af;">WindsorContainer</span>();</p>
<p style="margin: 0px;">&nbsp;</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span style="color: gray;">///</span><span style="color: green;"> </span><span style="color: gray;">&lt;summary&gt;</span></p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span style="color: gray;">///</span><span style="color: green;"> Gets/Sets the current Windsor container. Applications can either use</span></p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span style="color: gray;">///</span><span style="color: green;"> the reference to the container that this property provides, or they</span></p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span style="color: gray;">///</span><span style="color: green;"> can set their own reference through the setter. </span></p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span style="color: gray;">///</span><span style="color: green;"> </span><span style="color: gray;">&lt;/summary&gt;</span></p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span style="color: blue;">public</span> <span style="color: blue;">static</span> <span style="color: #2b91af;">IWindsorContainer</span> Container { <span style="color: blue;">get</span>; <span style="color: blue;">set</span>; }</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; }</p>
<p style="margin: 0px;">}</p>
</div>
</code>

By default, an empty container is created which you can access (and thus, configure) through the IoC.Container property.  Or if you want the library to use your own Windsor container instance you can simply set the instance and then the library will use your container.  For the moment, it is not possible to use another container (like StructureMap or Unity) yet, but that possibility might be added if people want it, or if someone submits a patch :)

Obviously, you need to register your request handlers somewhere in your application code, preferably before you start hosting the service layer.  You can do that like this:

<code>
<div style="font-family: Consolas; font-size: 10pt; color: black; background: white;">
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; Brion.Library.ServerSide.<span style="color: #2b91af;">ComponentRegistration</span></p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; .RegisterRequestHandlersFrom(<span style="color: #2b91af;">Assembly</span>.GetExecutingAssembly());</p>
</div>
</code>

That basically registers each valid request handler (each type that inherits from the RequestHandler class provided by the library) that is present in the given assembly.  You can also very easily provide your own custom base request handler, in case you want to provide some extra stuff.  You'd simply need to inherit from the library's RequestHandler class, add whatever it is you need, and let your request handlers inherit from your new class instead of directly inheriting from the library's RequestHandler.

<h2>Hosting The Service Layer</h2>

There are a lot of options for hosting the service layer. Most people will probably host it as a WCF service so we'll cover that scenario here. The service contract looks like this:

<code>
<div style="font-family: Consolas; font-size: 10pt; color: black; background: white;">
<p style="margin: 0px;"><span style="color: blue;">namespace</span> Brion.Library.Common.WCF</p>
<p style="margin: 0px;">{</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; [<span style="color: #2b91af;">ServiceContract</span>]</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; <span style="color: blue;">public</span> <span style="color: blue;">interface</span> <span style="color: #2b91af;">IWcfRequestProcessor</span></p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; {</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; [<span style="color: #2b91af;">OperationContract</span>]</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; [<span style="color: #2b91af;">ServiceKnownType</span>(<span style="color: #a31515;">"GetKnownTypes"</span>, <span style="color: blue;">typeof</span>(<span style="color: #2b91af;">KnownTypeProvider</span>))]</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span style="color: #2b91af;">Response</span>[] Process(<span style="color: blue;">params</span> <span style="color: #2b91af;">Request</span>[] requests);</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; }</p>
<p style="margin: 0px;">}</p>
</div>
</code>

This is practically identical to the IRequestProcessor interface shown earlier in the article. The difference is that IRequestProcessor uses no WCF attributes.  The actual implementation of IRequestProcessor is completely decoupled from WCF as well so you can use this approach without having to use WCF if you want to.  The implementation of the IWcfRequestProcessor simply forwards each call to the real request processor.

Notice the usage of the <a href="http://davybrion.com/blog/2008/07/the-known-type-provider/">KnownTypeProvider class</a>. This makes sure that each of your Request or Response derived types will be properly recognized as a KnownType.  All you have to do is register them, like this:

<code>
<div style="font-family: Consolas; font-size: 10pt; color: black; background: white;">
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span style="color: #2b91af;">KnownTypeProvider</span>.RegisterDerivedTypesOf&lt;<span style="color: #2b91af;">Request</span>&gt;(sharedAssembly);</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span style="color: #2b91af;">KnownTypeProvider</span>.RegisterDerivedTypesOf&lt;<span style="color: #2b91af;">Response</span>&gt;(sharedAssembly);</p>
</div>
</code> 

This registers each Request or Response derived type from the sharedAssembly reference (which is a reference to the shared assembly containing the request and response types) with the KnownTypeProvider.  You need to do this before you start hosting the service.  It's best to combine this task with the registration of your request handlers.

The actual hosting of the service is typical WCF and is entirely up to you.  Here's a simple example of self-hosting the service in a console app:

<code>
<div style="font-family: Consolas; font-size: 10pt; color: black; background: white;">
<p style="margin: 0px;"><span style="color: blue;">&nbsp; &nbsp; &lt;</span><span style="color: #a31515;">services</span><span style="color: blue;">&gt;</span></p>
<p style="margin: 0px;"><span style="color: blue;">&nbsp; &nbsp; &nbsp; &lt;</span><span style="color: #a31515;">service</span><span style="color: blue;"> </span><span style="color: red;">name</span><span style="color: blue;">=</span>"<span style="color: blue;">Brion.Library.ServerSide.WCF.WcfRequestProcessor</span>"<span style="color: blue;"> </span><span style="color: red;">behaviorConfiguration</span><span style="color: blue;">=</span>"<span style="color: blue;">MyBehavior</span>"<span style="color: blue;">&gt;</span></p>
<p style="margin: 0px;"><span style="color: blue;">&nbsp; &nbsp; &nbsp; &nbsp; &lt;</span><span style="color: #a31515;">endpoint</span><span style="color: blue;"> </span><span style="color: red;">contract</span><span style="color: blue;">=</span>"<span style="color: blue;">Brion.Library.Common.WCF.IWcfRequestProcessor</span>"<span style="color: blue;"> </span><span style="color: red;">binding</span><span style="color: blue;">=</span>"<span style="color: blue;">netNamedPipeBinding</span>"<span style="color: blue;"> </span></p>
<p style="margin: 0px;"><span style="color: blue;">&nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; </span><span style="color: red;">bindingConfiguration</span><span style="color: blue;">=</span>"<span style="color: blue;">MyNamedPipeBinding</span>"<span style="color: blue;"> </span><span style="color: red;">address</span><span style="color: blue;">=</span>"<span style="color: blue;">net.pipe://localhost/RequestProcessor</span>"<span style="color: blue;">/&gt;</span></p>
<p style="margin: 0px;"><span style="color: blue;">&nbsp; &nbsp; &nbsp; &lt;/</span><span style="color: #a31515;">service</span><span style="color: blue;">&gt;</span></p>
<p style="margin: 0px;"><span style="color: blue;">&nbsp; &nbsp; &lt;/</span><span style="color: #a31515;">services</span><span style="color: blue;">&gt;</span></p>
</div>
</code>

<code>
<div style="font-family: Consolas; font-size: 10pt; color: black; background: white;">
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span style="color: blue;">using</span> (<span style="color: blue;">var</span> host = <span style="color: blue;">new</span> <span style="color: #2b91af;">ServiceHost</span>(<span style="color: blue;">typeof</span>(<span style="color: #2b91af;">WcfRequestProcessor</span>)))</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; {</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; host.Open();</p>
<p style="margin: 0px;">&nbsp;</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span style="color: #2b91af;">Console</span>.WriteLine(<span style="color: #a31515;">"press ENTER to quit"</span>);</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span style="color: #2b91af;">Console</span>.ReadLine();</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; }</p>
</div>
</code>

You'll most likely prefer an alternative way of hosting the service, but again, that is entirely up to you and is typical WCF stuff.  All you need to know is that the service contract is the Brion.Library.Common.WCF.IWcfRequestProcessor interface, and the actual implementation of the service is the Brion.Library.ServerSide.WCF.WcfRequestProcessor class.

<h2>Communicating With The Service Layer</h2>

In your client layer, you can choose between using a direct proxy type for the IWcfRequestProcessor service, or you can use the Dispatcher class.  The Dispatcher class is somewhat of a wrapper around the direct proxy to make it easier to use and to get a nicer syntax.  The Dispatcher class implements the IDispatcher interface:

<code>
<div style="font-family: Consolas; font-size: 10pt; color: black; background: white;">
<p style="margin: 0px;"><span style="color: blue;">namespace</span> Brion.Library.ClientSide.Messaging</p>
<p style="margin: 0px;">{</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; <span style="color: blue;">public</span> <span style="color: blue;">interface</span> <span style="color: #2b91af;">IDispatcher</span> : <span style="color: #2b91af;">IDisposable</span></p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; {</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span style="color: #2b91af;">IEnumerable</span>&lt;<span style="color: #2b91af;">Request</span>&gt; Requests { <span style="color: blue;">get</span>; }</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span style="color: #2b91af;">IEnumerable</span>&lt;<span style="color: #2b91af;">Response</span>&gt; Responses { <span style="color: blue;">get</span>; }</p>
<p style="margin: 0px;">&nbsp;</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span style="color: blue;">void</span> Add(<span style="color: #2b91af;">Request</span> request);</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span style="color: blue;">void</span> Add(<span style="color: blue;">params</span> <span style="color: #2b91af;">Request</span>[] requestsToAdd);</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span style="color: blue;">void</span> Add(<span style="color: blue;">string</span> key, <span style="color: #2b91af;">Request</span> request);</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; TResponse Get&lt;TResponse&gt;() <span style="color: blue;">where</span> TResponse : <span style="color: #2b91af;">Response</span>;</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; TResponse Get&lt;TResponse&gt;(<span style="color: blue;">string</span> key) <span style="color: blue;">where</span> TResponse : <span style="color: #2b91af;">Response</span>;</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; TResponse Get&lt;TResponse&gt;(<span style="color: #2b91af;">Request</span> request) <span style="color: blue;">where</span> TResponse : <span style="color: #2b91af;">Response</span>;</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span style="color: blue;">void</span> Clear();</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; }</p>
<p style="margin: 0px;">}</p>
</div>
</code>

There are 3 implementations of this interface.  The first is the Dispatcher class itself, which has a dependency on an IRequestProcessor instance.  The second is the WcfDispatcher class which inherits from the Dispatcher class and will supply its base class with a RequestProcessorProxy instance to satisfy the IRequestProcessor dependency.  The third implementation is the DispatcherStub class, which is only useful for your tests.  If you want to use dependency injection through the container, you can register the correct components at client start-up time like this:

<code>
<div style="font-family: Consolas; font-size: 10pt; color: black; background: white;">
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; Brion.Library.ClientSide.<span style="color: #2b91af;">ComponentRegistration</span>.RegisterDispatcherForWcfUsage();</p>
</div>
</code>

The container will then have the correct configuration to provide you with IDispatcher instances that will correctly use the RequestProcessorProxy underneath. If you don't want to use the container client-side, you can simply instantiate instances of the WcfDispatcher class.

So, how do you use the dispatcher? It's pretty easy really.  Suppose you want to dispatch two requests to the service, you could do so like this:

<code>
<div style="font-family: Consolas; font-size: 10pt; color: black; background: white;">
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; dispatcher.Add(<span style="color: blue;">new</span> <span style="color: #2b91af;">GetProductCategoriesRequest</span>(), <span style="color: blue;">new</span> <span style="color: #2b91af;">GetSuppliersRequest</span>());</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; View.ProductCategories = dispatcher.Get&lt;<span style="color: #2b91af;">GetProductCategoriesResponse</span>&gt;().ProductCategories;</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; View.Suppliers = dispatcher.Get&lt;<span style="color: #2b91af;">GetSuppliersResponse</span>&gt;().Suppliers;</p>
</div>
</code>

The requests won't be dispatched to the service until you actually call one of the Get method overloads for the first time.  So you can add as much requests as you like, they won't be dispatched until you try to retrieve a response. And when those requests are dispatched, it is done in only one roundtrip.

If you just want to dispatch a single request, you could do so like this:

<code>
<div style="font-family: Consolas; font-size: 10pt; color: black; background: white;">
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span style="color: blue;">var</span> request = <span style="color: blue;">new</span> <span style="color: #2b91af;">GetProductOverviewsRequest</span> </p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; { NamePattern = name, CategoryId = productCategoryId, SupplierId = supplierId };</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span style="color: blue;">var</span> response = dispatcher.Get&lt;<span style="color: #2b91af;">GetProductOverviewsResponse</span>&gt;(request);</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; View.Products = response.Products;</p>
</div>
</code>

Keep in mind that you do need to define the service endpoint in your client-side application's configuration file. You also need to register the KnownTypes client-side before you start using the Dispatcher or the service proxy in the manner discussed earlier.

<h2>Stubbing The Service Layer During Testing</h2>

If you write tests for your client-side code, you'll be glad to hear that i've included a DispatcherStub class which makes it easy to prepare responses to return and to inspect requests that were sent from the code you're testing.  The approach i outlined in this post doesn't really lend itself to easy mocking, so the DispatcherStub class makes it all much easier.

First of all, make sure your client code always uses references of the IDispatcher type instead of directly using the Dispatcher or WcfDispatcher types.  Then in your tests, inject DispatcherStub instances instead of real Dispatchers.  The DispatcherStub class provides a few extra methods which you can use in your tests:

<code>
<div style="font-family: Consolas; font-size: 10pt; color: black; background: white;">
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; [<span style="color: #2b91af;">Test</span>]</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span style="color: blue;">public</span> <span style="color: blue;">void</span> RetrievesCategoriesAndSuppliersOnLoad()</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; {</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span style="color: blue;">var</span> categoriesToReturn = <span style="color: blue;">new</span> <span style="color: #2b91af;">ProductCategoryDTO</span>[0];</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span style="color: blue;">var</span> suppliersToReturn = <span style="color: blue;">new</span> <span style="color: #2b91af;">SupplierDTO</span>[0];</p>
<p style="margin: 0px;">&nbsp;</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; dispatcher.SetResponsesToReturn(</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span style="color: blue;">new</span> <span style="color: #2b91af;">GetProductCategoriesResponse</span> { ProductCategories = categoriesToReturn },</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span style="color: blue;">new</span> <span style="color: #2b91af;">GetSuppliersResponse</span> { Suppliers = suppliersToReturn });</p>
<p style="margin: 0px;">&nbsp;</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; view.Expect(v =&gt; v.ProductCategories = categoriesToReturn);</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; view.Expect(v =&gt; v.Suppliers = suppliersToReturn);</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; view.Expect(v =&gt; v.DataBind());</p>
<p style="margin: 0px;">&nbsp;</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; mocks.ReplayAll();</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; CreateController().Load();</p>
<p style="margin: 0px;">&nbsp;</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; view.VerifyAllExpectations();</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span style="color: #2b91af;">Assert</span>.IsNotNull(dispatcher.GetRequest&lt;<span style="color: #2b91af;">GetProductCategoriesRequest</span>&gt;());</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span style="color: #2b91af;">Assert</span>.IsNotNull(dispatcher.GetRequest&lt;<span style="color: #2b91af;">GetSuppliersRequest</span>&gt;());</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; }</p>
</div>
</code>

The SetResponsesToReturn and GetRequest methods make it much easier to stub the service layer than traditional mocking would.

Something you'll often want to test is that requests have been sent with the correct parameters:

<code>
<div style="font-family: Consolas; font-size: 10pt; color: black; background: white;">
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span style="color: blue;">var</span> request = dispatcher.GetRequest&lt;<span style="color: #2b91af;">GetProductOverviewsRequest</span>&gt;();</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span style="color: #2b91af;">Assert</span>.AreEqual(productPattern, request.NamePattern);</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span style="color: #2b91af;">Assert</span>.AreEqual(categoryId, request.CategoryId);</p>

<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span style="color: #2b91af;">Assert</span>.AreEqual(supplierId, request.SupplierId);</p>
</div>
</code>

<h2>Time to wrap it up</h2>
Again, i really like this approach. Unfortunately i haven't had the chance to use this in a real project yet, but based on my experiments i'm already very happy with it and am looking forward to use this in a real project. If you'd like to use this approach, download the library <a href="http://davybrion.com/blog/stuff/">here</a>, play around with it, try it out, try to break it, and let me know what needs to be fixed/changed/added.  Or send patches ;)