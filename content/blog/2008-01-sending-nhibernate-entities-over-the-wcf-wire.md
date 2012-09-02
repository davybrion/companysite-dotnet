Note: i no longer recommend this solution for reasons that i've talked about in more detail <a href="http://davybrion.com/blog/2010/05/why-you-shouldnt-expose-your-entities-through-your-services/">here</a>.

Last year, Tim Scott posted <a href="http://lunaverse.wordpress.com/2007/05/09/remoting-using-wcf-and-nhibernate/"> this very good article on how to distribute NHiberate entities through WCF services</a>. In it, he mentions this:

<blockquote>We should mention that this application uses NHibernate 1.02. As of this writing, NHibernate has released version 1.2. We tested the application with NHibernate 1.2 before changing to the NetDataContractSerializer, and verified that it exhibits the same problem. We have not verified that the solution described here will work with NHibnerate 1.2, although we expect it will.</blockquote>

I created a small sample that uses his solution with NHibernate 1.2 and my Northwind example and in this post, we'll walk through the sample.  But first, a little bit of background information.  By default, WCF services use the DataContractSerializer to serialize/deserialize types. But when you're using NHibernate, you're most likely using some of the persistent collection types and proxies. Some people just want to use the same types serverside and clientside. For instance, retrieving a Customer object with his Orders collection through a service, manipulating one of the Order objects clientside, perhaps remove an order from the collection, send the object graph back to the server, attach it to an nhibernate session, persist the whole thing and be done with it. I'm still somewhat undecided as to whether or not this is a good way of doing things but that's beside the point of this post. Default WCF services do not make this possible but you can make it work rather easily.  WCF includes the NetDataContractSerializer which differs from the normal DataContractSerializer in that it includes CLR type information in the serialized data. This makes the above scenario possible.  You do lose all interoperability with other platforms, and your clients need the same types you use server side.

First of all, this is the service contract:

<div style="font-family:Consolas;font-size:10pt;color:black;background:white;">
<p style="margin:0;">&nbsp;&nbsp;&nbsp; [<span style="color:#2b91af;">ServiceContract</span>]</p>
<p style="margin:0;">&nbsp;&nbsp;&nbsp; <span style="color:blue;">public</span> <span style="color:blue;">interface</span> <span style="color:#2b91af;">ICustomerService</span></p>
<p style="margin:0;">&nbsp;&nbsp;&nbsp; {</p>
<p style="margin:0;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; [<span style="color:#2b91af;">UseNetDataContractSerializer</span>]</p>
<p style="margin:0;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; [<span style="color:#2b91af;">OperationContract</span>]</p>
<p style="margin:0;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span style="color:#2b91af;">IList</span>&lt;<span style="color:#2b91af;">Customer</span>&gt; GetCustomersWithTheirOrders();</p>
<p style="margin:0;">&nbsp;</p>
<p style="margin:0;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; [<span style="color:#2b91af;">UseNetDataContractSerializer</span>]</p>
<p style="margin:0;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; [<span style="color:#2b91af;">OperationContract</span>]</p>
<p style="margin:0;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span style="color:blue;">void</span> PersistCustomer(<span style="color:#2b91af;">Customer</span> customer);</p>
<p style="margin:0;">&nbsp;&nbsp;&nbsp; }</p>
</div>

The [UseNetDataContractSerializer] is not a standard WCF attribute, but is a part of Tim Scott's solution. You can find the implementation of the attribute in his post, or in my code which you can download at the bottom of this post. It basically comes down to this: the serialization behavior of operations decorated with this attribute is modified to use the NetDataContractSerializer instead of the default DataContractSerializer.

The implementation of the service contract looks like this:

<div style="font-family:Consolas;font-size:10pt;color:black;background:white;">
<p style="margin:0;">&nbsp;&nbsp;&nbsp; <span style="color:blue;">public</span> <span style="color:blue;">class</span> <span style="color:#2b91af;">CustomerService</span> : <span style="color:#2b91af;">ICustomerService</span></p>
<p style="margin:0;">&nbsp;&nbsp;&nbsp; {</p>
<p style="margin:0;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span style="color:blue;">private</span> <span style="color:blue;">static</span> <span style="color:blue;">readonly</span> <span style="color:#2b91af;">ISessionFactory</span> _sessionFactory;</p>
<p style="margin:0;">&nbsp;</p>
<p style="margin:0;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span style="color:blue;">static</span> CustomerService()</p>
<p style="margin:0;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; {</p>
<p style="margin:0;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span style="color:blue;">try</span></p>
<p style="margin:0;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; {</p>
<p style="margin:0;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span style="color:#2b91af;">Configuration</span> configuration = <span style="color:blue;">new</span> <span style="color:#2b91af;">Configuration</span>().AddAssembly(<span style="color:#a31515;">"Northwind.Domain"</span>);</p>
<p style="margin:0;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; _sessionFactory = configuration.BuildSessionFactory();</p>
<p style="margin:0;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; }</p>
<p style="margin:0;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span style="color:blue;">catch</span> (<span style="color:#2b91af;">Exception</span> e)</p>
<p style="margin:0;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; {</p>
<p style="margin:0;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span style="color:#2b91af;">Console</span>.Write(e);</p>
<p style="margin:0;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span style="color:blue;">throw</span>;</p>
<p style="margin:0;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; }</p>
<p style="margin:0;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; }</p>
<p style="margin:0;">&nbsp;</p>
<p style="margin:0;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span style="color:blue;">public</span> <span style="color:#2b91af;">IList</span>&lt;<span style="color:#2b91af;">Customer</span>&gt; GetCustomersWithTheirOrders()</p>
<p style="margin:0;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; {</p>
<p style="margin:0;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span style="color:blue;">using</span> (<span style="color:#2b91af;">ISession</span> session = _sessionFactory.OpenSession())</p>
<p style="margin:0;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; {</p>
<p style="margin:0;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span style="color:blue;">return</span> session.CreateCriteria(<span style="color:blue;">typeof</span>(<span style="color:#2b91af;">Customer</span>))</p>
<p style="margin:0;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; .SetFetchMode(<span style="color:#a31515;">"Orders"</span>, <span style="color:#2b91af;">FetchMode</span>.Join)</p>
<p style="margin:0;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; .SetResultTransformer(<span style="color:#2b91af;">CriteriaUtil</span>.DistinctRootEntity)</p>
<p style="margin:0;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; .List&lt;<span style="color:#2b91af;">Customer</span>&gt;();</p>
<p style="margin:0;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; }</p>
<p style="margin:0;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; }</p>
<p style="margin:0;">&nbsp;</p>
<p style="margin:0;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span style="color:blue;">public</span> <span style="color:blue;">void</span> PersistCustomer(<span style="color:#2b91af;">Customer</span> customer)</p>
<p style="margin:0;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; {</p>
<p style="margin:0;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span style="color:blue;">using</span> (<span style="color:#2b91af;">ISession</span> session = _sessionFactory.OpenSession())</p>
<p style="margin:0;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; {</p>
<p style="margin:0;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span style="color:blue;">using</span> (<span style="color:#2b91af;">ITransaction</span> transaction = session.BeginTransaction())</p>
<p style="margin:0;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; {</p>
<p style="margin:0;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; session.SaveOrUpdate(customer);</p>
<p style="margin:0;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; session.Flush();</p>
<p style="margin:0;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; transaction.Commit();</p>
<p style="margin:0;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; }</p>
<p style="margin:0;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; }</p>
<p style="margin:0;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; }</p>
<p style="margin:0;">&nbsp;&nbsp;&nbsp; }</p>
</div>

First of all, this is just an example, that's why the ISessionFactory is created within the service implementation, in a real system i wouldn't do this.  Anyway, the GetCustomersWithTheirOrders method returns a list of all Customers, with their Orders.  An Order contains references to an Employee and a Shipper.  The Employee and Shipper will not be retrieved from the database, but NHibernate will initialize them with proxy objects to enable lazy-loading.  Obviously, the lazy-loading won't work once you're outside of the scope of the NHibernate session, but it's important to note that there will be proxies in our object graph.

At first i had decorated my entity classes with the [DataContract] and [DataMember] attributes but that really messed up the deserialization of the proxies. Now my Entity classes are only decorated with the [Serializable] attribute. NetDataContractSerializer should work in both cases, but i only got it working properly when they were [Serializable].

Right, so now we have the service contract and the implementation, it's time to host it. My solution contains an example of a console host as well as a service hosted in IIS. For this post, i'll just go over the console host and console client. You can find the IIS example (which is practically identical anyway) in the downloadable solution.

So, in the console host project, i have the following in my app.config file:

<div style="font-family:Consolas;font-size:10pt;color:black;background:white;">
<p style="margin:0;"><span style="color:blue;">&nbsp; &lt;</span><span style="color:#a31515;">system.serviceModel</span><span style="color:blue;">&gt;</span></p>
<p style="margin:0;"><span style="color:blue;">&nbsp; &nbsp; &lt;</span><span style="color:#a31515;">services</span><span style="color:blue;">&gt;</span></p>
<p style="margin:0;"><span style="color:blue;">&nbsp; &nbsp; &nbsp; &lt;</span><span style="color:#a31515;">service</span><span style="color:blue;"> </span><span style="color:red;">name</span><span style="color:blue;">=</span>"<span style="color:blue;">ServiceImplementation.CustomerService</span>"<span style="color:blue;"> </span><span style="color:red;">behaviorConfiguration</span><span style="color:blue;">=</span>"<span style="color:blue;">customerServiceBehavior</span>"<span style="color:blue;">&gt;</span></p>
<p style="margin:0;"><span style="color:blue;">&nbsp; &nbsp; &nbsp; &nbsp; &lt;</span><span style="color:#a31515;">endpoint</span><span style="color:blue;"> </span><span style="color:red;">contract</span><span style="color:blue;">=</span>"<span style="color:blue;">ServiceInterface.ICustomerService</span>"<span style="color:blue;"> </span><span style="color:red;">binding</span><span style="color:blue;">=</span>"<span style="color:blue;">wsHttpBinding</span>"<span style="color:blue;"> </span></p>
<p style="margin:0;"><span style="color:blue;">&nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; </span><span style="color:red;">bindingConfiguration</span><span style="color:blue;">=</span>"<span style="color:blue;">customerServiceBinding</span>"<span style="color:blue;"> /&gt;</span></p>
<p style="margin:0;"><span style="color:blue;">&nbsp; &nbsp; &nbsp; &nbsp; &lt;</span><span style="color:#a31515;">endpoint</span><span style="color:blue;"> </span><span style="color:red;">contract</span><span style="color:blue;">=</span>"<span style="color:blue;">IMetadataExchange</span>"<span style="color:blue;"> </span><span style="color:red;">binding</span><span style="color:blue;">=</span>"<span style="color:blue;">mexHttpBinding</span>"<span style="color:blue;"> </span><span style="color:red;">address</span><span style="color:blue;">=</span>"<span style="color:blue;">mex</span>"<span style="color:blue;"> /&gt;</span></p>
<p style="margin:0;"><span style="color:blue;">&nbsp; &nbsp; &nbsp; &nbsp; &lt;</span><span style="color:#a31515;">host</span><span style="color:blue;">&gt;</span></p>
<p style="margin:0;"><span style="color:blue;">&nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &lt;</span><span style="color:#a31515;">baseAddresses</span><span style="color:blue;">&gt;</span></p>
<p style="margin:0;"><span style="color:blue;">&nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &lt;</span><span style="color:#a31515;">add</span><span style="color:blue;"> </span><span style="color:red;">baseAddress</span><span style="color:blue;">=</span>"<span style="color:blue;">http://localhost:8000/CustomerService</span>"<span style="color:blue;">/&gt;</span></p>
<p style="margin:0;"><span style="color:blue;">&nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &lt;/</span><span style="color:#a31515;">baseAddresses</span><span style="color:blue;">&gt;</span></p>
<p style="margin:0;"><span style="color:blue;">&nbsp; &nbsp; &nbsp; &nbsp; &lt;/</span><span style="color:#a31515;">host</span><span style="color:blue;">&gt;</span></p>
<p style="margin:0;"><span style="color:blue;">&nbsp; &nbsp; &nbsp; &lt;/</span><span style="color:#a31515;">service</span><span style="color:blue;">&gt;</span></p>
<p style="margin:0;"><span style="color:blue;">&nbsp; &nbsp; &lt;/</span><span style="color:#a31515;">services</span><span style="color:blue;">&gt;</span></p>
<p style="margin:0;">&nbsp;</p>
<p style="margin:0;"><span style="color:blue;">&nbsp; &nbsp; &lt;</span><span style="color:#a31515;">bindings</span><span style="color:blue;">&gt;</span></p>
<p style="margin:0;"><span style="color:blue;">&nbsp; &nbsp; &nbsp; &lt;</span><span style="color:#a31515;">wsHttpBinding</span><span style="color:blue;">&gt;</span></p>
<p style="margin:0;"><span style="color:blue;">&nbsp; &nbsp; &nbsp; &nbsp; &lt;</span><span style="color:#a31515;">binding</span><span style="color:blue;"> </span><span style="color:red;">name</span><span style="color:blue;">=</span>"<span style="color:blue;">customerServiceBinding</span>"<span style="color:blue;"> </span><span style="color:red;">maxReceivedMessageSize</span><span style="color:blue;">=</span>"<span style="color:blue;">2147483647</span>"<span style="color:blue;"> /&gt;</span></p>
<p style="margin:0;"><span style="color:blue;">&nbsp; &nbsp; &nbsp; &lt;/</span><span style="color:#a31515;">wsHttpBinding</span><span style="color:blue;">&gt;</span></p>
<p style="margin:0;"><span style="color:blue;">&nbsp; &nbsp; &lt;/</span><span style="color:#a31515;">bindings</span><span style="color:blue;">&gt;</span></p>
<p style="margin:0;">&nbsp;</p>
<p style="margin:0;"><span style="color:blue;">&nbsp; &nbsp; &lt;</span><span style="color:#a31515;">behaviors</span><span style="color:blue;">&gt;</span></p>
<p style="margin:0;"><span style="color:blue;">&nbsp; &nbsp; &nbsp; &lt;</span><span style="color:#a31515;">serviceBehaviors</span><span style="color:blue;">&gt;</span></p>
<p style="margin:0;"><span style="color:blue;">&nbsp; &nbsp; &nbsp; &nbsp; &lt;</span><span style="color:#a31515;">behavior</span><span style="color:blue;"> </span><span style="color:red;">name</span><span style="color:blue;">=</span>"<span style="color:blue;">customerServiceBehavior</span>"<span style="color:blue;"> &gt;</span></p>
<p style="margin:0;"><span style="color:blue;">&nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &lt;</span><span style="color:#a31515;">serviceMetadata</span><span style="color:blue;"> </span><span style="color:red;">httpGetEnabled</span><span style="color:blue;">=</span>"<span style="color:blue;">true</span>"<span style="color:blue;"> </span><span style="color:red;">httpGetUrl</span><span style="color:blue;">=</span>""<span style="color:blue;">/&gt;</span></p>
<p style="margin:0;"><span style="color:blue;">&nbsp; &nbsp; &nbsp; &nbsp; &lt;/</span><span style="color:#a31515;">behavior</span><span style="color:blue;">&gt;</span></p>
<p style="margin:0;"><span style="color:blue;">&nbsp; &nbsp; &nbsp; &lt;/</span><span style="color:#a31515;">serviceBehaviors</span><span style="color:blue;">&gt;</span></p>
<p style="margin:0;"><span style="color:blue;">&nbsp; &nbsp; &lt;/</span><span style="color:#a31515;">behaviors</span><span style="color:blue;">&gt;</span></p>
<p style="margin:0;">&nbsp;</p>
<p style="margin:0;"><span style="color:blue;">&nbsp; &lt;/</span><span style="color:#a31515;">system.serviceModel</span><span style="color:blue;">&gt;</span></p>
</div>

The service is then started like this:

<div style="font-family:Consolas;font-size:10pt;color:black;background:white;">
<p style="margin:0;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span style="color:blue;">private</span> <span style="color:blue;">static</span> <span style="color:blue;">void</span> Main(<span style="color:blue;">string</span>[] args)</p>
<p style="margin:0;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; {</p>
<p style="margin:0;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span style="color:blue;">using</span> (<span style="color:#2b91af;">ServiceHost</span> host = <span style="color:blue;">new</span> <span style="color:#2b91af;">ServiceHost</span>(<span style="color:blue;">typeof</span>(<span style="color:#2b91af;">CustomerService</span>)))</p>
<p style="margin:0;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; {</p>
<p style="margin:0;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; host.Open();</p>
<p style="margin:0;">&nbsp;</p>
<p style="margin:0;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span style="color:#2b91af;">Console</span>.WriteLine(<span style="color:#a31515;">"press ENTER to quit"</span>);</p>
<p style="margin:0;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span style="color:#2b91af;">Console</span>.ReadLine();</p>
<p style="margin:0;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; }</p>
<p style="margin:0;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; }</p>
</div>

As you can see, nothing special here... it's pretty much your typical self-hosting WCF example.  Except that the maximum message size has been increased since we'll be sending large object graphs over the wire.

The client configuration looks like this:

<div style="font-family:Consolas;font-size:10pt;color:black;background:white;">
<p style="margin:0;"><span style="color:blue;">&nbsp; &lt;</span><span style="color:#a31515;">system.serviceModel</span><span style="color:blue;">&gt;</span></p>
<p style="margin:0;"><span style="color:blue;">&nbsp; &nbsp; &lt;</span><span style="color:#a31515;">bindings</span><span style="color:blue;">&gt;</span></p>
<p style="margin:0;"><span style="color:blue;">&nbsp; &nbsp; &nbsp; &lt;</span><span style="color:#a31515;">wsHttpBinding</span><span style="color:blue;">&gt;</span></p>
<p style="margin:0;"><span style="color:blue;">&nbsp; &nbsp; &nbsp; &nbsp; &lt;</span><span style="color:#a31515;">binding</span><span style="color:blue;"> </span><span style="color:red;">name</span><span style="color:blue;">=</span>"<span style="color:blue;">customerServiceBinding</span>"<span style="color:blue;"> </span><span style="color:red;">maxReceivedMessageSize</span><span style="color:blue;">=</span>"<span style="color:blue;">2147483647</span>"<span style="color:blue;"> /&gt;</span></p>
<p style="margin:0;"><span style="color:blue;">&nbsp; &nbsp; &nbsp; &lt;/</span><span style="color:#a31515;">wsHttpBinding</span><span style="color:blue;">&gt;</span></p>
<p style="margin:0;"><span style="color:blue;">&nbsp; &nbsp; &lt;/</span><span style="color:#a31515;">bindings</span><span style="color:blue;">&gt;</span></p>
<p style="margin:0;">&nbsp;</p>
<p style="margin:0;"><span style="color:blue;">&nbsp; &nbsp; &lt;</span><span style="color:#a31515;">client</span><span style="color:blue;">&gt;</span></p>
<p style="margin:0;"><span style="color:blue;">&nbsp; &nbsp; &nbsp; &lt;</span><span style="color:#a31515;">endpoint</span><span style="color:blue;"> </span><span style="color:red;">address</span><span style="color:blue;">=</span>"<span style="color:blue;">http://localhost:8000/CustomerService</span>"<span style="color:blue;"> </span><span style="color:red;">binding</span><span style="color:blue;">=</span>"<span style="color:blue;">wsHttpBinding</span>"<span style="color:blue;"> </span><span style="color:red;">name</span><span style="color:blue;">=</span>"<span style="color:blue;">CustomerServiceEndPoint</span>"</p>
<p style="margin:0;"><span style="color:blue;">&nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; </span><span style="color:red;">bindingConfiguration</span><span style="color:blue;">=</span>"<span style="color:blue;">customerServiceBinding</span>"<span style="color:blue;"> </span><span style="color:red;">contract</span><span style="color:blue;">=</span>"<span style="color:blue;">ServiceInterface.ICustomerService</span>"<span style="color:blue;"> /&gt;</span></p>
<p style="margin:0;"><span style="color:blue;">&nbsp; &nbsp; &lt;/</span><span style="color:#a31515;">client</span><span style="color:blue;">&gt;</span></p>
<p style="margin:0;">&nbsp;</p>
<p style="margin:0;"><span style="color:blue;">&nbsp; &lt;/</span><span style="color:#a31515;">system.serviceModel</span><span style="color:blue;">&gt;</span></p>
</div>

And the client uses the service like this:

<div style="font-family:Consolas;font-size:10pt;color:black;background:white;">
<p style="margin:0;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span style="color:blue;">private</span> <span style="color:blue;">static</span> <span style="color:blue;">void</span> Main(<span style="color:blue;">string</span>[] args)</p>
<p style="margin:0;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; {</p>
<p style="margin:0;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span style="color:#2b91af;">ChannelFactory</span>&lt;<span style="color:#2b91af;">ICustomerService</span>&gt; factory = <span style="color:blue;">new</span> <span style="color:#2b91af;">ChannelFactory</span>&lt;<span style="color:#2b91af;">ICustomerService</span>&gt;(<span style="color:#a31515;">"CustomerServiceEndPoint"</span>);</p>
<p style="margin:0;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span style="color:#2b91af;">ICustomerService</span> proxy = factory.CreateChannel();</p>
<p style="margin:0;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span style="color:#2b91af;">IList</span>&lt;<span style="color:#2b91af;">Customer</span>&gt; customers = proxy.GetCustomersWithTheirOrders();</p>
<p style="margin:0;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; }</p>
</div>

I didn't write a test for this to prove it works, but if you step through it and you explore the returned object graph using the debugger, you'll see that everything is of the correct type... even the proxies make it through correctly. Manipulating the graph and using the PersistCustomer method also works, but it's not in this example anymore because i didn't wanna pollute my database every time i ran it to test it.

I did have problems when i was hosting the service both in the console and through IIS at the same time... some requests would then fail to deserialize the returned graph properly. If you don't run both hosts at the same time, it works.  I have no idea why this caused issues, but after wasting a few hours trying to figure it out, i just gave up.  You can reproduce it by running the ServiceConsoleHost, ServiceClient and ServiceWebClient projects in the solution.  Sometimes that just works as well, so you may have to try a few times to get it to fail. So if anyone can shed some light on that issue, i'd be most interested in hearing about it :)

You can download the solution <a href="http://davybrion.com/NHibernateAndWcfExample.zip">here</a>.  Note that this is a Visual Studio 2008 solution...
