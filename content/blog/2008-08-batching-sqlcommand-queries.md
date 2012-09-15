As you know, i always like to reduce <a href="/blog/2008/07/batching/">unnecessary roundtrips</a>.  With my <a href="/blog/2008/07/the-request-response-service-layer/">Request/Response service layer</a> and my <a href="/blog/2008/06/the-query-batcher/">QueryBatcher for NHibernate</a>, it's trivially easy to do so. But what if you're in a situation where you can't use NHibernate and are stuck with low-level SqlCommands? It's actually not hard to enable batching those (select) queries either.  (note: for insert/update/delete Commands, there is <a href="http://www.ayende.com/Blog/archive/2006/09/14/7275.aspx">a better way</a>).

People with a lot of straight-up ADO.NET experience probably already know this, but you can simply combine your select statements into one SqlCommand.  When you execute that command, you get the results to each of the queries that was in the command.  So far, nothing new here.  I'm currently using a Data Access Layer at work which creates SqlCommand objects for select queries that you can build up through an API.  Obviously, i'd like a way to use each query in whatever way is best for the specific scenario i'm working on.  I basically want to be able to execute the SqlCommand objects as a stand-alone query, or in a batch with other queries, without having to modify the code that creates the SqlCommand objects.  

So i wrote a SelectCommandCombiner class which allows you to combine multiple SqlCommands into one SqlCommand, while making sure that none of the parameters conflict with each other.  Here's the code:

<script src="https://gist.github.com/3676387.js?file=s1.cs"></script>

Now execute the command returned from the CreateCombinedCommand method and you'll get all of the results in one roundtrip. There are a couple of ways to deal with the results... you could simply use a DataReader to loop through all the rows of all the result sets:

<script src="https://gist.github.com/3676387.js?file=s2.cs"></script>

Or you could use a SqlDataAdapter to fill a DataSet:

<script src="https://gist.github.com/3676387.js?file=s3.cs"></script>

In a future post, i'll try to make this as easy to use as the <a href="/blog/2008/06/the-query-batcher/">QueryBatcher for NHibernate</a>.