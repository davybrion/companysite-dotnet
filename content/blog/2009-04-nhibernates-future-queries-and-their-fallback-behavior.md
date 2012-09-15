I've blogged about NHibernate's Future queries a <a href="/blog/2009/01/nhibernate-and-future-queries/">couple</a> <a href="/blog/2009/01/nhibernate-and-future-queries-part-2/">of</a> <a href="/blog/2009/04/transparent-query-batching-through-your-repository/">times</a> already.  But as you know, NHibernate aims to offer you a way to write your code completely independent of the actual database you're using.  So what happens if you run your code, which is using the Future and FutureValue features, on a database that doesn't support batched queries?  Previously, this would fail with a NotSupportedException being thrown.

As of today, (revision 4177 if you want to be specific) this is no longer the case.  If you use the Future or FutureValue methods of either ICriteria or IQuery, and the database doesn't support batching queries, NHibernate will fall back to simply executing the queries immediately, as the following tests show:

<script src="https://gist.github.com/3684484.js?file=s1.cs"></script>

There are more tests obviously, but you get the point.  The interesting part about these tests is how i disabled query batching support.  I only have Sql Server and MySQL running on this machine, and they both support query batching.  I didn't really feel like installing a database that doesn't support it, so i just took advantage of NHibernate's extensibility.  Since most of us run the NHibernate tests on Sql Server, i inherited from the Sql Server Driver and made sure that it would report to NHibernate that it didn't support query batching:

<script src="https://gist.github.com/3684484.js?file=s2.cs"></script>

Easy huh? Then i just inherited from the TestCase class we have in the NHibernate.Tests project which offers a virtual method where you can modify the NHibernate configuration for the current fixture:

<script src="https://gist.github.com/3684484.js?file=s3.cs"></script>

Now NHibernate thinks that query batching isn't supported, yet the above tests still work.  Mission accomplished :)