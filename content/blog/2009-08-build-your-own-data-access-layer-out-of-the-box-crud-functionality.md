Note: This post is part of a series.  Be sure to read the introduction <a href="http://davybrion.com/blog/2009/08/build-your-own-data-access-layer-series/">here</a>.

One thing that i consider an absolute must-have in any data access layer is the ability to perform CRUD operations out-of-the-box without having to write any code to enable these operations.  Once your data access layer knows about your classes and your tables, CRUD operations should 'just work'.

As you've seen in the <a href="http://davybrion.com/blog/2009/08/build-your-own-data-access-layer-mapping-classes-to-tables/">previous post</a> of this series, the TableInfo class offers a couple of methods to automatically build the required SQL statements for CRUD actions.  With these statements, we can easily create SqlCommand instances for all CRUD operations.

First of all, i use the following helper method to easily add a SqlParameter to a SqlCommand:

<script src="https://gist.github.com/3684982.js?file=s1.cs"></script>

I also have the following abstract DatabaseAction class which has a few properties that are used by most of the CRUD actions:

<script src="https://gist.github.com/3684982.js?file=s2.cs"></script>

Did you notice the EntityHydrater and SessionLevelCache? I'm going to ignore those as much as possible for now, since they will be covered in depth in the following two posts in these series.  The important thing to note is that each derived DatabaseAction will have a reference to the MetaDataStore.

And now we can easily start implementing our CRUD actions.  Let's start with the GetByIdAction:

<script src="https://gist.github.com/3684982.js?file=s3.cs"></script>

Pretty simple stuff, right?  This will first check the session level cache to see if this instance has already been retrieved in the current session (i'll discuss the session in a later post) and if so, it will return that instance.  If it's not in the cache, it will create a SqlCommand and fill its CommandText property with a SQL string that is provided by the relevant TableInfo class.   After that, it passes the SqlCommand to the EntityHydrater so it can return an actual entity instance.

The details of EntityHydration will be fully explored in the next post of this series, so for now you only need to know that it can transform the results from the SqlCommand to an instance of TEntity.

It's always useful to get a collection of all instances of a certain entity class, so we also have this very simple FindAllAction:

<script src="https://gist.github.com/3684982.js?file=s4.cs"></script>

We also need an InsertAction:

<script src="https://gist.github.com/3684982.js?file=s5.cs"></script>

There's not a lot to this one either... The actual insert statement is once again retrieved through the TableInfo class, as are the parameter values (including their values for this specific entity).  You can go back to the previous post to look at the implementation of TableInfo's GetParametersForInsert method :)

Keep in mind that there is a limitation here that i only support SQL Server's Identity-style generators.  Again, if you want to support multiple identifier strategies like NHibernate does, you'll have to deal with a lot more complexity in the InsertAction class.

The UpdateAction is very similar:

<script src="https://gist.github.com/3684982.js?file=s6.cs"></script>

And finally, we have the DeleteAction:

<script src="https://gist.github.com/3684982.js?file=s7.cs"></script>

And that's all there is to it.  We now have some classes that will give us out-of-the-box CRUD functionality for all of the mapped entity classes.  Obviously, you will still need some way of actually accessing this functionality from your application code and you certainly don't want to instantiate and use these DatabaseAction classes directly.  All of that will be covered in the "Bringing It All Together" post, so stay tuned ;)