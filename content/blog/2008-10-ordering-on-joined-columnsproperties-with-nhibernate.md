I recently had to look into a minor issue one of the team members had with NHibernate.  Since i couldn't quickly find the solution online, i'm posting it here for future reference.

He had a criteria where he wanted to fetch all of the entities of a specific type, and have it joined with one of its associations.  Basically something like this:

<code>
<div style="font-family: Consolas; font-size: 10pt; color: black; background: white;">
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span style="color: blue;">var</span> query = <span style="color: #2b91af;">DetachedCriteria</span>.For&lt;<span style="color: #2b91af;">Product</span>&gt;()</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; .SetFetchMode(<span style="color: #a31515;">"Category"</span>, <span style="color: #2b91af;">FetchMode</span>.Join);</p>
</div>
</code>

Then he needed to apply ordering on one of the joined association's properties.  I said "no problem, it can do that" and i changed the code to this:

<code>
<div style="font-family: Consolas; font-size: 10pt; color: black; background: white;">
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span style="color: blue;">var</span> query = <span style="color: #2b91af;">DetachedCriteria</span>.For&lt;<span style="color: #2b91af;">Product</span>&gt;()</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; .SetFetchMode(<span style="color: #a31515;">"Category"</span>, <span style="color: #2b91af;">FetchMode</span>.Join)</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; .AddOrder(<span style="color: #2b91af;">Order</span>.Asc(<span style="color: #a31515;">"Category.Name"</span>));</p>
</div>
</code>

Executing that criteria gave the following error:

NHibernate.QueryException: could not resolve property: Category.Name of: Northwind.Domain.Entities.Product

Which is weird, because Product has a property called Category, which in turns has a property called Name. That should just work, right?  Apparently not... SetFetchMode merely instructs NHibernate how to fetch an association, but its usage does not mean you can just start adding extra options in the criteria for the entity's associations. If you want to do that, you need to add a subcriteria for the association to the first criteria (which you can then consider a parent criteria):

<code>
<div style="font-family: Consolas; font-size: 10pt; color: black; background: white;">
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span style="color: blue;">var</span> query = <span style="color: #2b91af;">DetachedCriteria</span>.For&lt;<span style="color: #2b91af;">Product</span>&gt;()</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; .CreateCriteria(<span style="color: #a31515;">"Category"</span>, <span style="color: #2b91af;">JoinType</span>.InnerJoin);</p>
</div>
</code>

In this version, we've added a second criteria to the first criteria.  Which means we can do everything with the second criteria (which is set to the given association) that we could normally do with a criteria.  So now, applying a sort Order to an association's property can be done like this:

<code>
<div style="font-family: Consolas; font-size: 10pt; color: black; background: white;">
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span style="color: blue;">var</span> query = <span style="color: #2b91af;">DetachedCriteria</span>.For&lt;<span style="color: #2b91af;">Product</span>&gt;()</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; .CreateCriteria(<span style="color: #a31515;">"Category"</span>, <span style="color: #2b91af;">JoinType</span>.InnerJoin)</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; .AddOrder(<span style="color: #2b91af;">Order</span>.Asc(<span style="color: #a31515;">"Name"</span>));</p>
</div>
</code>

Or, if you want to be more explicit in the usage of your property names (to avoid confusion with the Name property of the Product entity for instance), you could assign an alias to the second criteria, and then use that alias in the AddOrder call:

<code>
<div style="font-family: Consolas; font-size: 10pt; color: black; background: white;">
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span style="color: blue;">var</span> query = <span style="color: #2b91af;">DetachedCriteria</span>.For&lt;<span style="color: #2b91af;">Product</span>&gt;()</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; .CreateCriteria(<span style="color: #a31515;">"Category"</span>, <span style="color: #a31515;">"ProductCategory"</span>, <span style="color: #2b91af;">JoinType</span>.InnerJoin)</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; .AddOrder(<span style="color: #2b91af;">Order</span>.Asc(<span style="color: #a31515;">"ProductCategory.Name"</span>));</p>
</div>
</code>