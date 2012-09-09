I'm definitely not a fan of building your own Data Access Layer (DAL), since there are plenty of powerful and mature options already available.  However, we have 2 customers at work who simply don't let us use any existing libraries/tools as a DAL and want us to just use straight ADO.NET.  I don't want to get into their reasons for this, but the reality of the situation is that whenever we have to develop projects for them, we need to use a custom built DAL.  I've never seen a custom built DAL that i found acceptable, let alone one that i actually wanted to use.

A lot of people typically go the code generation route when faced with this situation, which is exactly what we have done in the past.  Been there, done that, hated it with a passion for various reasons.  One of my coworkers recently started a new project for one of these customers, and he started implementing a new DAL.  I had to review this, and while it had some good ideas there was a large amount of repetitive and error-prone code that still needed to be written by developers for every table.  So i set out to come up with something better.  If we did have to use a custom DAL for these customers, i wanted to make sure that it would at least avoid having us write repetitive, error-prone code for every table that we needed to use.  Oh, and without having to resort to code generation.  Since we are all NHibernate users (when customers don't have a problem with us using it, that is) i wanted something that was somewhat similar in ease-of-use though it could obviously never match its feature set, power and maturity.

I spent about 24 working hours (in total) on this, and i believe i came up with something that is acceptable for most simple forms-over-data applications. This DAL allows you to write your entity classes as POCO's, offers 'out-of-the-box' CRUD functionality for every mapped table, and has lazy loading for reference properties (so you don't need to pollute your entity classes with foreign key properties).  There is also a simple session-level cache, and there is some functionality to ease the pain of using simple, custom queries (with that i mean: every query that is not a select all or select by id and that doesn't join other tables).  

Compared to a real ORM, it is missing a lot: there is no Unit Of Work implementation, no automated change tracking of entities, no dirty checks, no collection support, no advanced querying possibilities, no statement batching, no serious caching functionality, no transitive persistence, and a whole host of features that something like NHibernate gives you for free.   Each and every one of those features comes with a great cost of complexity and development time to get 'right' so it truly doesn't make a lot of sense to do all of this yourself.

In this series, we're going to go over the entire implementation of this DAL and throughout the series i will point out its shortcomings and try to explain the complexity that would be required to make it truly powerful.  The purpose of this series is basically to:
<ul>
	<li>Show you that you really don't need to resort to code generation to build your own custom DAL</li>
	<li>Show you what kind of complexity is involved with the implementation of a good DAL</li>
	<li>Convince you that you typically are better off with simply using something that is already available as a mature, powerful and proven solution</li>
</ul>

These are the posts that this series consists of:
<ul>
	<li><a href="http://davybrion.com/blog/2009/08/build-your-own-data-access-layer-mapping-classes-to-tables/">Mapping Classes To Tables</a></li>
	<li><a href="http://davybrion.com/blog/2009/08/build-your-own-data-access-layer-out-of-the-box-crud-functionality/">Out Of The Box CRUD Functionality</a></li>
	<li><a href="http://davybrion.com/blog/2009/08/build-your-own-data-access-layer-hydrating-entities/">Hydrating Entities</a></li>
	<li><a href="http://davybrion.com/blog/2009/08/build-your-own-data-access-layer-session-level-cache/">Session Level Cache</a></li>
	<li><a href="http://davybrion.com/blog/2009/08/build-your-own-data-access-layer-lazy-loading/">Lazy Loading</a></li>
	<li><a href="http://davybrion.com/blog/2009/08/build-your-own-data-access-layer-executing-custom-queries/">Executing Custom Queries</a></li>
	<li><a href="http://davybrion.com/blog/2009/08/build-your-own-data-access-layer-bringing-it-all-together/">Bringing It All Together</a></li>
	<li><a href="http://davybrion.com/blog/2009/08/build-your-own-data-access-layer-conclusions/">Conclusions</a></li>
	<li><a href="http://davybrion.com/blog/2009/10/build-your-own-data-access-layer-enabling-bulk-inserts/">Enabling Bulk Inserts</a></li>
</ul>

The code of this series can be found <a href="https://github.com/davybrion/BuildYourOwnDal">here</a>