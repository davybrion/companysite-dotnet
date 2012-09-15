Note: i no longer recommend this solution for reasons that i've talked about in more detail [here](/blog/2010/05/why-you-shouldnt-expose-your-entities-through-your-services/).

Last year, Tim Scott posted this very good [article](http://lunaverse.wordpress.com/2007/05/09/remoting-using-wcf-and-nhibernate/) on how to distribute NHiberate entities through WCF services. In it, he mentions this:

> We should mention that this application uses NHibernate 1.02. As of this writing, NHibernate has released version 1.2. We tested the application with NHibernate 1.2 before changing to the NetDataContractSerializer, and verified that it exhibits the same problem. We have not verified that the solution described here will work with NHibnerate 1.2, although we expect it will.

I created a small sample that uses his solution with NHibernate 1.2 and my Northwind example and in this post, we'll walk through the sample.  But first, a little bit of background information.  By default, WCF services use the DataContractSerializer to serialize/deserialize types. But when you're using NHibernate, you're most likely using some of the persistent collection types and proxies. Some people just want to use the same types serverside and clientside. For instance, retrieving a Customer object with his Orders collection through a service, manipulating one of the Order objects clientside, perhaps remove an order from the collection, send the object graph back to the server, attach it to an nhibernate session, persist the whole thing and be done with it. I'm still somewhat undecided as to whether or not this is a good way of doing things but that's beside the point of this post. Default WCF services do not make this possible but you can make it work rather easily.  WCF includes the NetDataContractSerializer which differs from the normal DataContractSerializer in that it includes CLR type information in the serialized data. This makes the above scenario possible.  You do lose all interoperability with other platforms, and your clients need the same types you use server side.

First of all, this is the service contract:

<script src="https://gist.github.com/3611636.js?file=s1.cs"></script>

The [UseNetDataContractSerializer] is not a standard WCF attribute, but is a part of Tim Scott's solution. You can find the implementation of the attribute in his post, or in my code which you can download at the bottom of this post. It basically comes down to this: the serialization behavior of operations decorated with this attribute is modified to use the NetDataContractSerializer instead of the default DataContractSerializer.

The implementation of the service contract looks like this:

<script src="https://gist.github.com/3611636.js?file=s2.cs"></script>

First of all, this is just an example, that's why the ISessionFactory is created within the service implementation, in a real system i wouldn't do this.  Anyway, the GetCustomersWithTheirOrders method returns a list of all Customers, with their Orders.  An Order contains references to an Employee and a Shipper.  The Employee and Shipper will not be retrieved from the database, but NHibernate will initialize them with proxy objects to enable lazy-loading.  Obviously, the lazy-loading won't work once you're outside of the scope of the NHibernate session, but it's important to note that there will be proxies in our object graph.

At first i had decorated my entity classes with the [DataContract] and [DataMember] attributes but that really messed up the deserialization of the proxies. Now my Entity classes are only decorated with the [Serializable] attribute. NetDataContractSerializer should work in both cases, but i only got it working properly when they were [Serializable].

Right, so now we have the service contract and the implementation, it's time to host it. My solution contains an example of a console host as well as a service hosted in IIS. For this post, i'll just go over the console host and console client. You can find the IIS example (which is practically identical anyway) in the downloadable solution.

So, in the console host project, i have the following in my app.config file:

<script src="https://gist.github.com/3611636.js?file=s3.xml"></script>

The service is then started like this:

<script src="https://gist.github.com/3611636.js?file=s4.cs"></script>

As you can see, nothing special here... it's pretty much your typical self-hosting WCF example.  Except that the maximum message size has been increased since we'll be sending large object graphs over the wire.

The client configuration looks like this:

<script src="https://gist.github.com/3611636.js?file=s5.xml"></script>

And the client uses the service like this:

<script src="https://gist.github.com/3611636.js?file=s6.cs"></script>

I didn't write a test for this to prove it works, but if you step through it and you explore the returned object graph using the debugger, you'll see that everything is of the correct type... even the proxies make it through correctly. Manipulating the graph and using the PersistCustomer method also works, but it's not in this example anymore because i didn't wanna pollute my database every time i ran it to test it.

I did have problems when i was hosting the service both in the console and through IIS at the same time... some requests would then fail to deserialize the returned graph properly. If you don't run both hosts at the same time, it works.  I have no idea why this caused issues, but after wasting a few hours trying to figure it out, i just gave up.  You can reproduce it by running the ServiceConsoleHost, ServiceClient and ServiceWebClient projects in the solution.  Sometimes that just works as well, so you may have to try a few times to get it to fail. So if anyone can shed some light on that issue, i'd be most interested in hearing about it :)

You can download the solution <a href="http://davybrion.com/NHibernateAndWcfExample.zip">here</a>.  Note that this is a Visual Studio 2008 solution...
