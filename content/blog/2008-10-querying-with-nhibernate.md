A lot of people are rather skeptical when it comes to executing non-trivial queries with NHibernate. In this post, i want to explore some of the features that NHibernate offers to execute those kind of queries in an easy manner.

Now, the difference between easy, non-trivial and complex queries is different for everyone. So in the following example, the query that needs to be executed is not at all complex, but it isn't your typically way too simplistic example either. It does show some often occurring requirements for queries, but at the same time it's still small enough to grasp easily.

Suppose we have the following 4 tables:

<a href="http://davybrion.com/blog/wp-content/uploads/2008/10/querying_example_tables.png"><img src="http://davybrion.com/blog/wp-content/uploads/2008/10/querying_example_tables.png" alt="" title="querying_example_tables" width="499" height="371" class="aligncenter size-full wp-image-481" /></a>

Now, suppose we have the following business requirement: if we discontinue a product, we want to inform all of the customers who've ever bought that product. 

NHibernate offers a few options of retrieving the customers that once bought a given product. You could use the lazy loading capabilities to walk the object graph and keep the customers you need.  This approach would justify a punch in the face though.  That's just abusing lazy loading to achieve lazy coding, which is just wrong.  The correct way to fetch the data is to query for it in an efficient manner.

Suppose that we would typically write the following SQL query to fetch the required data:

<code>
<pre>
select
	customer.CustomerId,
	customer.CompanyName,
	customer.ContactName,
	customer.ContactTitle,
	customer.Address,
	customer.City,
	customer.Region,
	customer.PostalCode,
	customer.Country,
	customer.Phone,
	customer.Fax
from
	dbo.Customers customer
where
	customer.CustomerId in
		(select distinct CustomerId from Orders
		 where OrderId in (select OrderId from [Order Details] where ProductId = 24))</pre>
</code>

For the purpose of this example, let's just assume that the ProductId that we need is 24. Now, i'm far from a SQL guru so i don't know if this approach (using subqueries) is the best way to fetch this data.  We'll explore another possibility later on.  But for now, let's try to get NHibernate to generate a query like the one i just showed you.

First of all, let's focus on the following subquery:
select OrderId from [Order Details] where ProductId = 24

With NHibernate, we'd get the same thing like this:

<code>
<div style="font-family: Consolas; font-size: 10pt; color: black; background: white;">
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span style="color: blue;">var</span> orderIdsCriteria = <span style="color: #2b91af;">DetachedCriteria</span>.For&lt;<span style="color: #2b91af;">OrderLine</span>&gt;()</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; .SetProjection(<span style="color: #2b91af;">Projections</span>.Distinct(<span style="color: #2b91af;">Projections</span>.Property(<span style="color: #a31515;">"Order.Id"</span>)))</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; .Add(<span style="color: #2b91af;">Restrictions</span>.Eq(<span style="color: #a31515;">"Product.Id"</span>, productId));</p>
</div>
</code>

This basically tells NHibernate to build a query which fetches each Orders' Id property for every Order that has an OrderLine which contains the given Product's Id.  Keep in mind that this doesn't actually fetch the Order Id's yet.

Now that we already have that part, let's focus on the next subquery:
select distinct CustomerId from Orders
where OrderId in (select OrderId from [Order Details] where ProductId = 24)

With NHibernate, we'd get the same thing like this:

<code>
<div style="font-family: Consolas; font-size: 10pt; color: black; background: white;">
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span style="color: blue;">var</span> customerIdsFromOrdersForProductCriteria = <span style="color: #2b91af;">DetachedCriteria</span>.For&lt;<span style="color: #2b91af;">Order</span>&gt;()</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; .SetProjection(<span style="color: #2b91af;">Projections</span>.Distinct(<span style="color: #2b91af;">Projections</span>.Property(<span style="color: #a31515;">"Customer.Id"</span>)))</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; .Add(<span style="color: #2b91af;">Subqueries</span>.PropertyIn(<span style="color: #a31515;">"Id"</span>, orderIdsCriteria));</p>
</div>
</code>

This builds a query which returns the Customer Id for each Customer that ever ordered the given product.  Notice how we reuse the previous subquery in this Criteria.

Now we need to build a query that fetches the full Customer entities, but only for the Customers whose Id is in the resultset of the previous query:

<code>
<div style="font-family: Consolas; font-size: 10pt; color: black; background: white;">
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span style="color: blue;">var</span> customersThatBoughtProductCriteria = <span style="color: #2b91af;">DetachedCriteria</span>.For&lt;<span style="color: #2b91af;">Customer</span>&gt;()</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; .Add(<span style="color: #2b91af;">Subqueries</span>.PropertyIn(<span style="color: #a31515;">"Id"</span>, customerIdsFromOrdersForProductCriteria));</p>
</div>
</code>

That's pretty easy, right? This is the query that NHibernate sends to the database to fetch the data:

<code>
<pre>
SELECT 
   this_.CustomerId as CustomerId0_0_, 
   this_.CompanyName as CompanyN2_0_0_, 
   this_.ContactName as ContactN3_0_0_, 
   this_.ContactTitle as ContactT4_0_0_, 
   this_.Address as Address0_0_, 
   this_.City as City0_0_, 
   this_.Region as Region0_0_, 
   this_.PostalCode as PostalCode0_0_, 
   this_.Country as Country0_0_, 
   this_.Phone as Phone0_0_, 
   this_.Fax as Fax0_0_ 
FROM dbo.Customers this_ 
WHERE 
   this_.CustomerId in 
      (SELECT distinct this_0_.CustomerId as y0_ FROM dbo.Orders this_0_ 
       WHERE this_0_.OrderId in 
           (SELECT distinct this_0_0_.OrderId as y0_ FROM dbo.[Order Details] this_0_0_ WHERE  
            this_0_0_.ProductId = @p0));
</pre>
</code>

Apart from the aliases that were added, this looks exactly the same as the query i wrote manually.  
An extra benefit that i think is pretty important is that each part of the query is actually reusable. If you built an API that could give you each part of the entire query that you needed, then you could easily reuse each part whenever you needed it.  Duplication in queries is just as bad as duplication in code IMHO.

Suppose you'd want to limit the amount of subqueries and use a join instead of the lowest level subquery.  If we'd write the query ourselves, it would look something like this:

<code>
<pre>
select
	customer.CustomerId,
	customer.CompanyName,
	customer.ContactName,
	customer.ContactTitle,
	customer.Address,
	customer.City,
	customer.Region,
	customer.PostalCode,
	customer.Country,
	customer.Phone,
	customer.Fax
from
	dbo.Customers customer
where
	customer.CustomerId in 
		(select o.customerId
		 from Orders o inner join [Order Details] line on line.OrderId = o.OrderId
		 where line.ProductId = 24)
</pre>
</code>

First, let's try to write the following query with NHibernate's Criteria API:
select o.customerId 
from Orders o inner join [Order Details] line on line.OrderId = o.OrderId
where line.ProductId = 24

Since our Order class has an OrderLines collection that is mapped to the [Order Details] table, we can generate that part of the query like this:

<code>
<div style="font-family: Consolas; font-size: 10pt; color: black; background: white;">
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span style="color: blue;">var</span> customerIdsFromOrdersForProductCriteria = <span style="color: #2b91af;">DetachedCriteria</span>.For&lt;<span style="color: #2b91af;">Order</span>&gt;()</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; .SetProjection(<span style="color: #2b91af;">Projections</span>.Distinct(<span style="color: #2b91af;">Projections</span>.Property(<span style="color: #a31515;">"Customer.Id"</span>)))</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; .CreateCriteria(<span style="color: #a31515;">"OrderLines"</span>, <span style="color: #2b91af;">JoinType</span>.InnerJoin)</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; .Add(<span style="color: #2b91af;">Restrictions</span>.Eq(<span style="color: #a31515;">"Product.Id"</span>, productId));</p>
</div>
</code>

The final part remains the same:

<code>
<div style="font-family: Consolas; font-size: 10pt; color: black; background: white;">
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span style="color: blue;">var</span> customersThatBoughtProductCriteria = <span style="color: #2b91af;">DetachedCriteria</span>.For&lt;<span style="color: #2b91af;">Customer</span>&gt;()</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; .Add(<span style="color: #2b91af;">Subqueries</span>.PropertyIn(<span style="color: #a31515;">"Id"</span>, customerIdsFromOrdersForProductCriteria));</p>
</div>
</code>

And the query that NHibernate generates looks like this:

<code>
<pre>
SELECT 
   this_.CustomerId as CustomerId0_0_, 
   this_.CompanyName as CompanyN2_0_0_, 
   this_.ContactName as ContactN3_0_0_, 
   this_.ContactTitle as ContactT4_0_0_, 
   this_.Address as Address0_0_, 
   this_.City as City0_0_, 
   this_.Region as Region0_0_, 
   this_.PostalCode as PostalCode0_0_, 
   this_.Country as Country0_0_, 
   this_.Phone as Phone0_0_, 
   this_.Fax as Fax0_0_ 
FROM 
   dbo.Customers this_ 
WHERE 
   this_.CustomerId in 
      (SELECT distinct this_0_.CustomerId as y0_ 
       FROM dbo.Orders this_0_ inner join dbo.[Order Details] orderline1_ on this_0_.OrderId = 
       orderline1_.OrderId WHERE orderline1_.ProductId = @p0); 
</pre>
</code>

Again, pretty easy right?

The Criteria API's Projection features, combined with Subqueries and combining Criteria into larger Criteria offers you a lot of possibilities when it comes to querying.  This post only showed a very small part of what's available, but hopefully it's enough to point some people in the right direction. Now, NHibernate's criteria API is pretty powerful, but the learning curve is indeed somewhat steep. It does take a while to get used to it, and i certainly don't know everything there is to know about it either. But it's definitely worth investing some time into learning how to use it well. 