As you may have read by now, it's a good idea to <a href="http://ayende.com/Blog/archive/2009/03/20/nhibernate-avoid-identity-generator-when-possible.aspx">avoid identity-style identifier strategies</a> with ORM's.  One of the better alternatives that i kinda like is the guid.comb strategy.  Using regular guids as a primary key value leads to fragmented indexes (due to the randomness of the guid's value) which leads to bad performance.  This is a problem that the guid.comb strategy can solve quite easily for you.

If you want to learn how the guid.comb strategy really works, be sure to check out <a href="http://www.informit.com/articles/article.aspx?p=25862">Jimmy Nilsson's article on it</a>. Basically, this strategy generates sequential guids which solves the fragmented index issue.  You can generate these sequential guids in your database, but the downside of that is that your ORM would still need to insert each record seperately and fetch the generated primary key value each time.  NHibernate includes the guid.comb strategy which will generate the sequential guids before actually inserting the records in your database.

This obviously has some great benefits: 
<ul>
	<li>you don't have to hit the database immediately whenever a record needs to be inserted</li>
	<li>you don't need to retrieve a generated primary key value when a record was inserted</li>
	<li>you can batch your insert statements</li>
</ul>

Let's see how we can use this with NHibernate.  First of all, you need to map the identifier of your entity like this:

<script src="https://gist.github.com/3684514.js?file=s1.xml"></script>

And that's actually all you have to do.  You don't have to assign the primary key values or anything like that.  You don't need to worry about them at all.  

Take a look at the following test:

<script src="https://gist.github.com/3684514.js?file=s2.cs"></script>

Interesting, no? The entities have an ID value after they have been 'saved' by NHibernate.  But they haven't actually been saved to the database yet though.  NHibernate always tries to wait as long as possible to hit the database, and in this case it only needs to hit the database when the transaction is committed.  If you've enabled <a href="/blog/2008/10/batching-nhibernates-dm-statements/">batching of DML statements</a>, you could severly reduce the number of times you need to hit the database in this scenario.

And in case you're wondering, the generated guids look like this:

81cdb935-d371-4285-9dcb-9bdb0122f25f<br/>
a44baf99-58e9-4ad7-9a59-9bdb0122f25f<br/>
a88300c2-6d64-4ae3-a55b-9bdb0122f25f<br/>
032c7884-da2f-4568-b505-9bdb0122f25f<br/>
....<br/>
70d7713c-b38d-4341-953d-9bdb0122f25f<br/>

Notice the last part of the guids... this is what prevents the index fragmentation.

Obviously, this particular test is not a realistic scenario but i'm sure you understand how much of an improvement this identifier strategy could provide throughout an entire application.  The only downside (IMO) is that guid's aren't really human readable so if that is important to you, you should probably look into other identifier strategies.  The HiLo strategy would be particularly interesting in that case, but we'll cover that in a later post ;)