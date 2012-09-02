All of our projects that use NHibernate (which is all of them except those where the customer explicitly doesn't want us to use it or where it wouldn't make sense to use it) use the same <a href="http://davybrion.com/blog/2008/06/data-access-with-nhibernate/">Repository implementation</a>.  After the <a href="http://davybrion.com/blog/2009/01/nhibernate-and-future-queries/">Future</a> and <a href="http://davybrion.com/blog/2009/01/nhibernate-and-future-queries-part-2/">FutureValue</a> queries were added to NHibernate, i modified the implementation of that Repository class.  

Two of the FindAll methods now look like this:

<div>
[csharp]
        public virtual IEnumerable&lt;T&gt; FindAll()
        {
            return Session.CreateCriteria&lt;T&gt;().Future&lt;T&gt;();
        }
 
        public virtual IEnumerable&lt;T&gt; FindAll(DetachedCriteria criteria)
        {
            return criteria.GetExecutableCriteria(Session).Future&lt;T&gt;();
        }
[/csharp]
</div>

The only thing i changed in those methods is calling the Future method, instead of the List method.  That's it.  All of our specific Find-methods (those that execute specific queries) pass through the FindAll(DetachedCriteria criteria) method so they all benefit from this change.  

That means that all of our queries are suddenly batched transparently whenever possible, without impacting any of the calling code.  And that is pretty nice if you ask me.  Batching queries can offer a substantial performance benefit, and we didn't even have to change any of the calling code to achieve it.  

Obviously, this only works for the queries that return IEnumerables (in our case, that's every query that doesn't return a single value).  I also added a few more methods to enable query batching for queries that return a single entity, or a scalar value (i kept the original methods in this code snippet as well so you can see the difference):

<div>
[csharp]
        public virtual T FindOne(DetachedCriteria criteria)
        {
            return criteria.GetExecutableCriteria(Session).UniqueResult&lt;T&gt;();
        }
 
        public virtual IFutureValue&lt;T&gt; FindFutureOne(DetachedCriteria criteria)
        {
            return criteria.GetExecutableCriteria(Session).FutureValue&lt;T&gt;();
        }
 
        public virtual K GetScalar&lt;K&gt;(DetachedCriteria criteria)
        {
            return (K)criteria.GetExecutableCriteria(Session).UniqueResult();
        }
 
        public virtual IFutureValue&lt;K&gt; GetFutureScalar&lt;K&gt;(DetachedCriteria criteria)
        {
            return criteria.GetExecutableCriteria(Session).FutureValue&lt;K&gt;();
        }
 
        public virtual int Count(DetachedCriteria criteria)
        {
            return Convert.ToInt32(QueryCount(criteria).GetExecutableCriteria(Session).UniqueResult());
        }
 
        public virtual IFutureValue&lt;int&gt; FutureCount(DetachedCriteria criteria)
        {
            return QueryCount(criteria).GetExecutableCriteria(Session).FutureValue&lt;int&gt;();
        }
[/csharp]
</div>

So let's recap.  Queries that return IEnumerables are all batched transparently whenever it's possible to do so.  No calling code had to be modified to get this benefit.  Queries that return single values (an entity instance or a scalar value) that still use the 'old' FindOne, GetScalar and Count methods obviously couldn't benefit from the transparent batching without breaking backwards compatibility, but the new methods that were introduced do enable transparent batching for these queries from now on.

Does all of this sound too good to be true? I'd be skeptic too if i were you but i made these changes a few months ago actually and we have been using this stuff on a couple of projects with zero problems.  

Obviously, you need NHibernate 2.1 Alpha 1 (or later) for this or the current trunk, both of which i would recommend over NH 2.0 at this point.