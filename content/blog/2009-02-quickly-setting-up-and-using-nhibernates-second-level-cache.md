The purpose of this post is just to quickly go over what you need to do to get NHibernate's 2nd Level Cache working in your application.  If you want to read how the 1st and 2nd Level Caches work, please read Gabriel Schenker's excellent and thorough <a href="http://blogs.hibernatingrhinos.com/nhibernate/archive/2008/11/09/first-and-second-level-caching-in-nhibernate.aspx">post about it</a>.

Anyways, the first thing you need to do, is to enable the 2nd level cache.  Add the following 2 properties to your hibernate.cfg.xml file:

<script src="https://gist.github.com/3684295.js?file=s1.xml"></script>

The first one (obviously) enables the 2nd level cache, while the second one enables query caching. That basically means that you can (optionally) cache the results of specific queries.  Note that this doesn't mean that the results of all queries will be cached, only the ones where you specify that the results can be cached.

Next, you need to choose a CacheProvider.  There are various options available, although i generally just use SysCache (which makes use of the ASP.NET Cache).

Once you've picked out a CacheProvider, you need to add a property for it to your hibernate.cfg.xml file as well:

<script src="https://gist.github.com/3684295.js?file=s2.xml"></script>

Let's first start with caching the results of a query.  Suppose we have the following query:

<script src="https://gist.github.com/3684295.js?file=s3.cs"></script>

If we want NHibernate to cache the results of this query, we can make that happen like this:

<script src="https://gist.github.com/3684295.js?file=s4.cs"></script>

When we execute this query, NHibernate will cache the results of this query.  It is very important to know that it won't actually cache all of the values of each row.  Instead, when the results of queries are cached, only the identifiers of the returned rows are cached.

So what happens when we execute the query the first time with categoryId containing the value 1? It sends the correct SQL statement to the database, creates all of the entities, but it only stores the identifiers of those entities in the cache.  The second time you execute this query with categoryId containing the value 1, it will retrieve the previously cached identifiers but then it will go to the database to fetch each row that corresponds with the cached identifiers.

Obviously, this is bad.  What good is caching if it's actually making us go to the database more often than without caching?  That is where entity caching comes in.  In this case, our query returns Product entities, but because the Product entity hasn't been configured for caching, only the identifiers are cached.  If we enable caching for Product entities, the resulting identifiers of the query will be cached, as well as the actual entities.  In this case, the second time this query is executed with a categoryId with value 1, we won't hit the database at all because both the resulting identifiers as well as the entities are stored in the cache.  

To enable caching on the entity level, add the following property right below the class definition in the Product.hbm.xml file:

<script src="https://gist.github.com/3684295.js?file=s5.xml"></script>

This tells NHibernate to store the data of Product entities in the 2nd level cache, and that any updates that we make to Product entities need to be synchronized in both the database and the cache.

That's pretty much all you need to do to get the 2nd Level Cache working.  But please keep in mind that there is a lot more to caching than what i showed in this post.  Reading Gabriel's post on caching is an absolute must IMO.  Caching is a powerful feature, but with great power comes great responsibility. Learn how to use it wisely :)