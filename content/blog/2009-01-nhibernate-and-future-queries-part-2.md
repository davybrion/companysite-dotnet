In my last post i <a href="http://davybrion.com/blog/2009/01/nhibernate-and-future-queries/">showed you</a> how we can now use the Future feature in NHibernate.  The only downside to the feature is that we only have the Future method in ICriteria, which returns a generic IEnumerable.  Which doesn't really lead to nice code when dealing with queries that return a scalar value, or just a single row in general.  So i decided to extend the feature a little bit.  

I introduced the IFutureValue interface, which only defines a Value property:

<div>
[csharp]
    public interface IFutureValue&lt;T&gt;
    {
        T Value { get; }
    }
[/csharp]
</div>

Then i added the FutureValue method on ICriteria, which returns an IFutureValue instance which behaves exactly like the IEnumerable that is returned by the Future method.  If you access the Value property of the IFutureValue instance, it will either execute all of the currently queued Future queries in a single roundtrip, or it will simply return the result if the queries were already executed.

Here's some useless sample code to show off the feature:

<div>
[csharp]
            using (ISession session = sessionFactory.OpenSession())
            {
                IFutureValue&lt;int&gt; categoryCount = session.CreateCriteria(typeof(ProductCategory))
                    .SetProjection(Projections.RowCount())
                    .FutureValue&lt;int&gt;();
 
                IFutureValue&lt;Supplier&gt; mySupplier = session.CreateCriteria(typeof(Supplier))
                    .Add(Restrictions.Eq(&quot;Id&quot;, supplierId))
                    .FutureValue&lt;Supplier&gt;();
 
                IEnumerable&lt;Product&gt; allProducts = session.CreateCriteria(typeof(Product))
                    .Future&lt;Product&gt;();
 
                // the next line causes the 3 queries to be executed
                int count = categoryCount.Value;
                Supplier retrievedSupplier = mySupplier.Value;
 
                foreach (var product in allProducts)
                {
                    // yada yada yada... we're doing something important
                }
[/csharp]
</div>

This is available starting with revision 4000, or in the official NHibernate 2.1 release.