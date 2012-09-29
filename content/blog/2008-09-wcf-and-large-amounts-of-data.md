If you're using my Request/Response service layer, or any other WCF service that might require sending large amounts of data over the wire, you quickly bump into some limits that WCF enforces by default.  Among the billions of configuration options for WCF, there are luckily some options that allow you to easily send large amounts of data from a service to a client.

I typically use the following options:

For my binding configuration I usually set the maxReceivedMessageSize, maxStringContentLength and the maxArrayLength properties to their maximum values:

<script src="https://gist.github.com/3684001.js?file=s1.xml"></script>

This example shows these settings for the netTcpBinding... I've also used them with the wsHttpBinding. Not sure how well it works with other bindings though.

I also set the maxItemsInObjectGraph setting of the DataContractSerializer to make sure I don't hit the default limit if I have to send a large object graph over the wire:

<script src="https://gist.github.com/3684001.js?file=s2.xml"></script>

You have to apply these settings both server and client side to get it working properly and you need to refer to these settings in your service and endpoint settings:

<script src="https://gist.github.com/3684001.js?file=s3.xml"></script>

<script src="https://gist.github.com/3684001.js?file=s4.xml"></script>

Now, I don't recommend sending such large amounts of data through WCF services... but in the case of using my Request/Response service layer, the amount of data you're sending over the wire pretty much depends on which kind of requests (and how many of them) you're batching so I think it's better to make sure that at least the configuration allows for it.  So obviously, it's best to keep an eye on the size of your messages to make sure you're not doing anything crazy.  Being able to send your entire database over the wire doesn't mean it's a good idea to actually do so ;)