In my last post I <a href="/blog/2009/01/nhibernate-and-future-queries/">showed you</a> how we can now use the Future feature in NHibernate.  The only downside to the feature is that we only have the Future method in ICriteria, which returns a generic IEnumerable.  Which doesn't really lead to nice code when dealing with queries that return a scalar value, or just a single row in general.  So I decided to extend the feature a little bit.  

I introduced the IFutureValue interface, which only defines a Value property:

<script src="https://gist.github.com/3684244.js?file=s1.cs"></script>

Then I added the FutureValue method on ICriteria, which returns an IFutureValue instance which behaves exactly like the IEnumerable that is returned by the Future method.  If you access the Value property of the IFutureValue instance, it will either execute all of the currently queued Future queries in a single roundtrip, or it will simply return the result if the queries were already executed.

Here's some useless sample code to show off the feature:

<script src="https://gist.github.com/3684244.js?file=s2.cs"></script>

This is available starting with revision 4000, or in the official NHibernate 2.1 release.