As <a href="/blog/2008/08/batching-sqlcommand-queries/">promised</a>, this post describes a way to make batching of select queries through SqlCommands as easy to use as my <a href="/blog/2008/06/the-query-batcher/">QueryBatcher for NHibernate</a>.  

So naturally, i came up with the SelectCommandBatcher class (i know, i really need to come up with better names).  It's very similar in usage to the QueryBatcher for NHibernate.  Obviously, there is one important difference: NHibernate takes care of transforming the database result into entity objects automagically.  For the SelectCommandBatcher, you either have to provide some functionality that takes care of mapping the database results to objects, or you can access the results through DataTables (yikes!).

Let's just get straight to the code, shall we? For the first couple of examples, i'll focus specifically on how you can retrieve your results so each example only uses one command.  At the end of the examples, there is of course one example that illustrates how you can add multiple SqlCommands and fetch the results in one roundtrip (which is after all the purpose of the SelectCommandBatcher class).

Suppose you have some code that is able to transform the content of a DataTable (or a DataRow) to a List of entities (or one entity):

<script src="https://gist.github.com/3676737.js?file=s1.cs"></script>

Obviously, this simply returns a new Product for each DataRow in the DataTable.  If this were real code, you'd have to put the data of the DataRow into the Product instance.  Feel free to consider me lazy :P

With the SelectCommandBatcher, you can now do this:

<script src="https://gist.github.com/3676737.js?file=s2.cs"></script>

Notice how the TransformTableToListOfProducts method is passed to the GetEnumerableResult method as a parameter.  How does it work? It's pretty simple really: The GetEnumerableResult method's definition looks like this:

<script src="https://gist.github.com/3676737.js?file=s3.cs"></script>

Basically, this method requires you to provide a delegate which accepts a DataTable as its sole incoming parameter, and returns an IEnumerable of T with T being any Type your code transforms the result to. 

What if your Data Access Layer only has methods to transform individual rows to entities? No problem:

<script src="https://gist.github.com/3676737.js?file=s4.cs"></script>

This code uses an overload of the GetEnumerableResult method that looks like this:

<script src="https://gist.github.com/3676737.js?file=s5.cs"></script>

You just need to pass a delegate which takes a DataRow as its sole incoming parameter, and returns an instance of T with T being any Type your code transforms the result to.  The delegate that gets passed in will be called for every DataRow in the DataTable which contains the results of that specific select command.

Or if your query only returns a single row, you can do this:

<script src="https://gist.github.com/3676737.js?file=s6.cs"></script>

Scalar values you say? How about this:

<script src="https://gist.github.com/3676737.js?file=s7.cs"></script>

Or if you're old-school (sorry, couldn't resist) and just want the DataTable, you can use this:

<script src="https://gist.github.com/3676737.js?file=s8.cs"></script>

And of course, here's how you can execute multiple select queries in one roundtrip:

<script src="https://gist.github.com/3676737.js?file=s9.cs"></script>

The queries are executed as soon as you try to retrieve the first result.

That's all pretty easy to use right? So how does it work? Here's the code:

<script src="https://gist.github.com/3676737.js?file=s10.cs"></script>
