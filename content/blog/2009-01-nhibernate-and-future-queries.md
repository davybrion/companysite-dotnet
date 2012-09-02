As some of you already know, i'm a big fan of avoiding excessive roundtrips by batching queries and/or service calls.  For NHibernate, i wrote the <a href="http://davybrion.com/blog/2008/06/the-query-batcher/">QueryBatcher</a> class which makes this pretty easy to do.  Ayende recently added a much easier approach for this to NHibernate.  

Take a look at the following code:

<div>
[csharp]
            using (ISession session = sessionFactory.OpenSession())
            {
                // this executes the first query
                var categories = session.CreateCriteria(typeof(ProductCategory)).List();               
                // this executes the second query
                var suppliers = session.CreateCriteria(typeof(Supplier)).List();
 
                foreach (var category in categories)
                {
                    // do something
                }
 
                foreach (var supplier in suppliers)
                {
                    // do something
                }
            }
[/csharp]
</div>

This is a really trivial example, but it should be more than sufficient.  It simply executes two very simple queries and loops through the results to do something with each returned entity.  The problem, obviously, is that this hits the database twice while there really is no good reason for doing so.

With the new Future feature we can rewrite that code like this:

<div>
[csharp]
            using (ISession session = sessionFactory.OpenSession())
            {
                // this creates the first query
                var categories = session.CreateCriteria(typeof(ProductCategory)).Future&lt;ProductCategory&gt;();
                // this creates the second query
                var suppliers = session.CreateCriteria(typeof(Supplier)).Future&lt;Supplier&gt;();
 
                // this causes both queries to be sent in ONE roundtrip
                foreach (var category in categories)
                {
                    // do something
                }
 
                // this doesn't do anything because the suppliers have already been loaded
                foreach (var supplier in suppliers)
                {
                    // do something
                }
            }
[/csharp]
</div>

Apart from the comments, did you spot the difference? Instead of calling ICriteria's List method (which causes the query to be executed immediately), we call ICriteria's Future method.  This returns an IEnumerable of the type you provided to the Future method.  And this is where it gets interesting.  Instead of executing the queries immediately, the queries are added to an instance of NHibernate's already existing MultiCriteria class.  Only once you enumerate through one of the retrieved IEnumerables will all the (queued) Future queries be executed, in a single roundtrip.  Once they are executed, their result is final (as in: enumerating through the IEnumerable will not cause the query to be executed again).

The example used here is obviously very trivial, but you can use this with any ICriteria so you can very easily start batching your complex queries as well.  The kind of query doesn't really matter, as long as it's an ICriteria instance.

This feature will be available in NHibernate 2.1, or if you're using the trunk you can use it starting with revision 3999.