Suppose you have the following table structure:

<a href='http://davybrion.com/blog/wp-content/uploads/2008/06/products_categories_suppliers.png'><img src="http://davybrion.com/blog/wp-content/uploads/2008/06/products_categories_suppliers.png" alt="" title="products_categories_suppliers" width="500" height="195" class="aligncenter size-full wp-image-137" /></a>

And we have the following classes:

<a href='http://davybrion.com/blog/wp-content/uploads/2008/06/products_class_structure.png'><img src="http://davybrion.com/blog/wp-content/uploads/2008/06/products_class_structure.png" alt="" title="products_class_structure" width="500" height="331" class="aligncenter size-full wp-image-138" /></a>

What is the most efficient way to retrieve all Product instances, fully populated (with complete Supplier and ProductCategory references)?  There are a couple of ways to retrieve this data, depending on what kind of data access techniques you're using.  These are the goals we should aim to achieve:

<ul>
	<li>We only want <strong>one</strong> roundtrip to the database</li>
	<li>No joins... Depending on the amount of data in each of the tables, a 3-table join could easily lead to a query which is way too expensive for what we're trying to achieve</li>
	<li>We don't want to write boring code to construct the object graph. The object graph should be built up automagically</li>
</ul>

Ok... let's see.  Since we only want one trip to the database, that rules out using lazy loading (which would be a terrible idea in this case anyway).  And we don't want to join either, so our best bet is probably to fetch all the categories, all the suppliers, and all the products with 3 simple queries.  Obviously, we'd have to batch those 3 queries so the 3 results are retrieved with only one roundtrip. And then we need to figure out a way to join the results together in an easy to use object graph.

Luckily for me i'm using NHibernate which makes all of this incredibly easy. First, we create the 3 queries:

<code>

<div style="font-family: Consolas; font-size: 10pt; color: black; background: white;">
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span style="color: #2b91af;">IMultiCriteria</span> multiCriteria = Session.CreateMultiCriteria();</p>
<p style="margin: 0px;">&nbsp;</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; multiCriteria.Add(Session.CreateCriteria(<span style="color: blue;">typeof</span>(<span style="color: #2b91af;">ProductCategory</span>)));</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; multiCriteria.Add(Session.CreateCriteria(<span style="color: blue;">typeof</span>(<span style="color: #2b91af;">Supplier</span>)));</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; multiCriteria.Add(Session.CreateCriteria(<span style="color: blue;">typeof</span>(<span style="color: #2b91af;">Product</span>)));</p>
</div>

</code>

A Criteria instance is kinda like a programmatic query. If you create a Criteria that is only based on the type of an object, you basically create a query which returns each record from the table that is associated with that type without any other conditions.  In the code above, we create 3 queries this way to retrieve all the instances of the ProductCategory, Supplier and Product types.  At this point, the queries have only been created, they haven't been executed yet.

To retrieve the results of these 3 queries with one database call, we simply do this:

<code>

<div style="font-family: Consolas; font-size: 10pt; color: black; background: white;">
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span style="color: #2b91af;">IList</span> results = multiCriteria.List();</p>
</div>

</code>

When this line is executed, we can see the following happening on the database (through SQL Server Profiler):

<a href='http://davybrion.com/blog/wp-content/uploads/2008/06/sql_server_trace.png'><img src="http://davybrion.com/blog/wp-content/uploads/2008/06/sql_server_trace.png" alt="" title="sql_server_trace" width="500" height="312" class="aligncenter size-full wp-image-139" /></a>

Alright, so we've accomplished the goal of using only one database roundtrip without using joins.  Now, we want to use the data we retrieved without having to write boring code to make sure each product refers to the correct ProductCategory and Supplier references.  Actually, we don't really have to do anything for that.  The list of products was the third Criteria instance that we added, so it is also the third item in the result list.

<code>

<div style="font-family: Consolas; font-size: 10pt; color: black; background: white;">
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span style="color: #2b91af;">IEnumerable</span>&lt;<span style="color: #2b91af;">Product</span>&gt; products = ((<span style="color: #2b91af;">IList</span>)results[2]).Cast&lt;<span style="color: #2b91af;">Product</span>&gt;();</p>
</div>

</code>

Ok, so now we have a list of Products... Now how do we make sure that each product points to the correct ProductCategory and Supplier instances? Fortunately, we don't even have to worry about that.  When we access a Product's Category or Supplier properties, NHibernate first checks its current session's <a href="http://martinfowler.com/eaaCatalog/identityMap.html">identity map</a> to see if those objects are already present in the session's first-level cache.  Because we retrieved the ProductCategories and the Suppliers, all of them are already present.  So if we would write the following code, no more extra queries would be executed:

<code>

<div style="font-family: Consolas; font-size: 10pt; color: black; background: white;">
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span style="color: blue;">foreach</span> (<span style="color: #2b91af;">Product</span> product <span style="color: blue;">in</span> products)</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; {</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span style="color: blue;">string</span> dummy = product.Name + <span style="color: #a31515;">" - "</span> +</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; product.Supplier.CompanyName + <span style="color: #a31515;">" - "</span> +</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; product.Category.Name;</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; }</p>
</div>

<code> 

This code is pretty much pointless, but it does show that we can simply access whatever data we need, without having to write boring code, and do it in an efficient way as well.

Now, you might be thinking "who cares if you retrieve the data in one roundtrip instead of three?".  Well, you should care about that stuff... what if you used this technique throughout your application (where applicable of course)?  Even if the data you need to retrieve is not related, this is still a potentially large performance improvement if you simply try to batch statements wherever you can.  The more you reduce the number of network roundtrips, the fewer time your application spends waiting on data to cross the wire.  And as your number of concurrent users increases, you really want to minimize the places in your code base where the application is just waiting on something.
