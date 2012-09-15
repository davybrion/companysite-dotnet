Note: This post is part of a series.  Be sure to read the introduction <a href="/blog/2009/08/build-your-own-data-access-layer-series/">here</a>.

You've already seen that this DAL offers you the ability to query each entity by primary key, or to retrieve a list of all instances of an entity type.  Obviously, this isn't even sufficient for the most trivial applications so we need a way to execute custom queries.  So i needed to provide something that would enable you to easily execute custom queries and get a list of entity instances, or a single result without having to muck around with transforming the results on your own.  

Due to the extremely simplistic nature of this DAL, i can't support much more than queries which either return a single value (being one entity instance, or a scalar value) or a list of entities.  As soon as you need to execute queries that return joined results, this DAL won't be able to deal with the results automatically.  I'll get back to this part later on in the post so for now, let's focus on queries whose result can be transformed to an entity automatically.

My first version of this DAL hardly had any support for this, and when using the DAL for custom queries, you basically had to deal with SqlCommands and their results manually.  Well, you could pass the SqlCommand to the EntityHydrater, but you still had to deal with a lot of ugly code.  I had asked <a href="http://ayende.com/blog/">Ayende</a> to do a private review before i started with this series, and he quickly pointed out that i needed something better for executing SQL queries and suggested something like (surprise, surprise) what NHibernate offers for SQL queries.  So yes, this solution is once again heavily inspired by NHibernate ;)

Let's go over the details... First of all, we have this simple IQuery interface:

<script src="https://gist.github.com/3685082.js?file=s1.cs"></script>

That is what you should be able to do with a query once you've created it.  Creating a query is possible through my session API (which will be covered in the next post):

<script src="https://gist.github.com/3685082.js?file=s2.cs"></script>

Through the regular CreateQuery method, you can provide the full SQL string and you have full control over the actual SQL.  The CreateQuery&lt;TEntity&gt; overload only requires you to provide the WHERE clause because it will generate a SELECT clause for the given entity which automatically retrieves all of the columns for this entity.  This also ensures that the result of CreateQuery&lt;TEntity&gt; can always be cleanly transformed to a list of entities or a single entity instance through the EntityHydrater.

This is the implementation of both CreateQuery methods:

<script src="https://gist.github.com/3685082.js?file=s3.cs"></script>

And here's the actual implementation of the Query class:

<script src="https://gist.github.com/3685082.js?file=s4.cs"></script>

Pretty simple, right? The ability to get a strong typed result is something that i find very important when using any DAL, and the Query class makes this very easy to do.  If you want to return a list of single value results, you can do that easily.  If you want to return a single scalar result, you can do that easily.  A single entity instance? No problem, the EntityHydrater takes care of that for us.  Same thing goes for a list of entities.  Custom delete or update statements? No problem, the ExecuteUpdate method can be used for that and will return the typical number of affected rows as reported by the database.

And now you can do things like this pretty easily:

<script src="https://gist.github.com/3685082.js?file=s5.cs"></script>

or

<script src="https://gist.github.com/3685082.js?file=s6.cs"></script>

or

<script src="https://gist.github.com/3685082.js?file=s7.cs"></script>

I'm sure you get the idea by now ;)

Note: when using the CreateQuery&lt;TEntity&gt; method, the SELECT clause that is generated automatically prefixes each selected column from the entity with the 'this' prefix.  This makes it easier to refer to the entity's properties in other clauses while you still have to ability to join on another table in the from clause, though you obviously can't add any columns to the select clause anymore. 

Now, this is all pretty good for queries where you only need to return scalar values or specific entities, but what if you want to return values from multiple tables in one query? This DAL doesn't have any support for that, but what you could do instead, is to create a view for your query and map an 'entity' to that view instead.  Then you could still get a typed result while querying the view, though the results wouldn't be able to be transformed into your 'real' entities.  But for filling grids or just to write typical overview queries, this might already be sufficient. 

As you can tell, there's not a lot of power or flexibility behind this approach.  But try to think of the complexity involved with trying to deal with results from multiple tables.  It makes <strong>everything</strong> a whole lot more complex.  At that time, you really need to consider if it's still worth writing your own DAL because the effort you'll spend on getting it right will be very significant.  