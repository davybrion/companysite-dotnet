If you're using my Request/Response service layer, or any other WCF service that might require sending large amounts of data over the wire, you quickly bump into some limits that WCF enforces by default.  Among the billions of configuration options for WCF, there are luckily some options that allow you to easily send large amounts of data from a service to a client.

I typically use the following options:

For my binding configuration i usually set the maxReceivedMessageSize, maxStringContentLength and the maxArrayLength properties to their maximum values:

<code>
<div style="font-family: Consolas; font-size: 10pt; color: black; background: white;">
<p style="margin: 0px;"><span style="color: blue;">&nbsp; &nbsp; &lt;</span><span style="color: #a31515;">bindings</span><span style="color: blue;">&gt;</span></p>
<p style="margin: 0px;"><span style="color: blue;">&nbsp; &nbsp; &nbsp; &lt;</span><span style="color: #a31515;">netTcpBinding</span><span style="color: blue;">&gt;</span></p>
<p style="margin: 0px;"><span style="color: blue;">&nbsp; &nbsp; &nbsp; &nbsp; &lt;</span><span style="color: #a31515;">binding</span><span style="color: blue;"> </span><span style="color: red;">name</span><span style="color: blue;">=</span>"<span style="color: blue;">MyTcpBinding</span>"<span style="color: blue;"> </span><span style="color: red;">maxReceivedMessageSize</span><span style="color: blue;">=</span>"<span style="color: blue;">2147483647</span>"<span style="color: blue;"> </span><span style="color: red;">receiveTimeout</span><span style="color: blue;">=</span>"<span style="color: blue;">00:30</span>"<span style="color: blue;"> </span><span style="color: red;">sendTimeout</span><span style="color: blue;">=</span>"<span style="color: blue;">00:30</span>"<span style="color: blue;">&gt;</span></p>
<p style="margin: 0px;"><span style="color: blue;">&nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &lt;</span><span style="color: #a31515;">readerQuotas</span><span style="color: blue;"> </span><span style="color: red;">maxStringContentLength</span><span style="color: blue;">=</span>"<span style="color: blue;">8192</span>"<span style="color: blue;"> </span><span style="color: red;">maxArrayLength</span><span style="color: blue;">=</span>"<span style="color: blue;">20971520</span>"<span style="color: blue;"> /&gt;</span></p>
<p style="margin: 0px;"><span style="color: blue;">&nbsp; &nbsp; &nbsp; &nbsp; &lt;/</span><span style="color: #a31515;">binding</span><span style="color: blue;">&gt;</span></p>
<p style="margin: 0px;"><span style="color: blue;">&nbsp; &nbsp; &nbsp; &lt;/</span><span style="color: #a31515;">netTcpBinding</span><span style="color: blue;">&gt;</span></p>
<p style="margin: 0px;"><span style="color: blue;">&nbsp; &nbsp; &lt;/</span><span style="color: #a31515;">bindings</span><span style="color: blue;">&gt;</span></p>
</div>
</code>

This example shows these settings for the netTcpBinding... i've also used them with the wsHttpBinding. Not sure how well it works with other bindings though.

I also set the maxItemsInObjectGraph setting of the DataContractSerializer to make sure i don't hit the default limit if i have to send a large object graph over the wire:

<code>
<div style="font-family: Consolas; font-size: 10pt; color: black; background: white;">
<p style="margin: 0px;"><span style="color: blue;">&nbsp; &nbsp; &lt;</span><span style="color: #a31515;">behaviors</span><span style="color: blue;">&gt;</span></p>
<p style="margin: 0px;"><span style="color: blue;">&nbsp; &nbsp; &nbsp; &lt;</span><span style="color: #a31515;">serviceBehaviors</span><span style="color: blue;">&gt;</span></p>
<p style="margin: 0px;"><span style="color: blue;">&nbsp; &nbsp; &nbsp; &nbsp; &lt;</span><span style="color: #a31515;">behavior</span><span style="color: blue;"> </span><span style="color: red;">name</span><span style="color: blue;">=</span>"<span style="color: blue;">MyBehavior</span>"<span style="color: blue;">&gt;</span></p>
<p style="margin: 0px;"><span style="color: blue;">&nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &lt;</span><span style="color: #a31515;">dataContractSerializer</span><span style="color: blue;"> </span><span style="color: red;">maxItemsInObjectGraph</span><span style="color: blue;">=</span>"<span style="color: blue;">2147483647</span>"<span style="color: blue;">/&gt;</span></p>
<p style="margin: 0px;"><span style="color: blue;">&nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &lt;</span><span style="color: #a31515;">serviceMetadata</span><span style="color: blue;"> /&gt;</span></p>
<p style="margin: 0px;"><span style="color: blue;">&nbsp; &nbsp; &nbsp; &nbsp; &lt;/</span><span style="color: #a31515;">behavior</span><span style="color: blue;">&gt;</span></p>
<p style="margin: 0px;"><span style="color: blue;">&nbsp; &nbsp; &nbsp; &lt;/</span><span style="color: #a31515;">serviceBehaviors</span><span style="color: blue;">&gt;</span></p>
<p style="margin: 0px;"><span style="color: blue;">&nbsp; &nbsp; &lt;/</span><span style="color: #a31515;">behaviors</span><span style="color: blue;">&gt;</span></p>
</div>
</code>

You have to apply these settings both server and client side to get it working properly and you need to refer to these settings in your service and endpoint settings:

<code>
<div style="font-family: Consolas; font-size: 10pt; color: black; background: white;">
<p style="margin: 0px;"><span style="color: blue;">&nbsp; &nbsp; &nbsp; &lt;</span><span style="color: #a31515;">service</span><span style="color: blue;"> </span><span style="color: red;">name</span><span style="color: blue;">=</span>"<span style="color: blue;">Brion.Library.ServerSide.WCF.WcfRequestProcessor</span>"<span style="color: blue;"> </span><span style="color: red;">behaviorConfiguration</span><span style="color: blue;">=</span>"<span style="color: blue;">MyBehavior</span>"<span style="color: blue;">&gt;</span></p>
<p style="margin: 0px;"><span style="color: blue;">&nbsp; &nbsp; &nbsp; &nbsp; &lt;</span><span style="color: #a31515;">host</span><span style="color: blue;">&gt;</span></p>
<p style="margin: 0px;"><span style="color: blue;">&nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &lt;</span><span style="color: #a31515;">baseAddresses</span><span style="color: blue;">&gt;</span></p>
<p style="margin: 0px;"><span style="color: blue;">&nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &lt;</span><span style="color: #a31515;">add</span><span style="color: blue;"> </span><span style="color: red;">baseAddress</span><span style="color: blue;">=</span>"<span style="color: blue;">net.tcp://localhost/RequestProcessor</span>"<span style="color: blue;">/&gt;</span></p>
<p style="margin: 0px;"><span style="color: blue;">&nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &lt;/</span><span style="color: #a31515;">baseAddresses</span><span style="color: blue;">&gt;</span></p>
<p style="margin: 0px;"><span style="color: blue;">&nbsp; &nbsp; &nbsp; &nbsp; &lt;/</span><span style="color: #a31515;">host</span><span style="color: blue;">&gt;</span></p>
<p style="margin: 0px;">&nbsp;</p>
<p style="margin: 0px;"><span style="color: blue;">&nbsp; &nbsp; &nbsp; &nbsp; &lt;</span><span style="color: #a31515;">endpoint</span><span style="color: blue;"> </span><span style="color: red;">contract</span><span style="color: blue;">=</span>"<span style="color: blue;">Brion.Library.Common.WCF.IWcfRequestProcessor</span>"<span style="color: blue;"> </span><span style="color: red;">binding</span><span style="color: blue;">=</span>"<span style="color: blue;">netTcpBinding</span>"</p>
<p style="margin: 0px;"><span style="color: blue;">&nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; </span><span style="color: red;">bindingConfiguration</span><span style="color: blue;">=</span>"<span style="color: blue;">MyTcpBinding</span>"<span style="color: blue;"> /&gt;</span></p>
<p style="margin: 0px;">&nbsp;</p>
<p style="margin: 0px;"><span style="color: blue;">&nbsp; &nbsp; &nbsp; &lt;/</span><span style="color: #a31515;">service</span><span style="color: blue;">&gt;</span></p>
</div>
</code>

<code>
<div style="font-family: Consolas; font-size: 10pt; color: black; background: white;">
<p style="margin: 0px;"><span style="color: blue;">&nbsp; &nbsp; &lt;</span><span style="color: #a31515;">client</span><span style="color: blue;">&gt;</span></p>
<p style="margin: 0px;"><span style="color: blue;">&nbsp; &nbsp; &nbsp; &lt;</span><span style="color: #a31515;">endpoint</span><span style="color: blue;"> </span><span style="color: red;">address</span><span style="color: blue;">=</span>"<span style="color: blue;">net.tcp://localhost/RequestProcessor</span>"<span style="color: blue;"> </span><span style="color: red;">binding</span><span style="color: blue;">=</span>"<span style="color: blue;">netTcpBinding</span>"</p>
<p style="margin: 0px;"><span style="color: blue;">&nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; </span><span style="color: red;">name</span><span style="color: blue;">=</span>"<span style="color: blue;">IRequestProcessor</span>"<span style="color: blue;"> </span><span style="color: red;">bindingConfiguration</span><span style="color: blue;">=</span>"<span style="color: blue;">MyTcpBinding</span>"<span style="color: blue;"> </span><span style="color: red;">behaviorConfiguration</span><span style="color: blue;">=</span>"<span style="color: blue;">MyBehavior</span>"</p>
<p style="margin: 0px;"><span style="color: blue;">&nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; </span><span style="color: red;">contract</span><span style="color: blue;">=</span>"<span style="color: blue;">Brion.Library.Common.WCF.IWcfRequestProcessor</span>"<span style="color: blue;"> /&gt;</span></p>
<p style="margin: 0px;"><span style="color: blue;">&nbsp; &nbsp; &lt;/</span><span style="color: #a31515;">client</span><span style="color: blue;">&gt;</span></p>
</div>
</code>

Now, i don't recommend sending such large amounts of data through WCF services... but in the case of using my Request/Response service layer, the amount of data you're sending over the wire pretty much depends on which kind of requests (and how many of them) you're batching so i think it's better to make sure that at least the configuration allows for it.  So obviously, it's best to keep an eye on the size of your messages to make sure you're not doing anything crazy.  Being able to send your entire database over the wire doesn't mean it's a good idea to actually do so ;)