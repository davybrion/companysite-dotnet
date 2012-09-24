In my previous <a href="/blog/2008/10/batching-nhibernates-dm-statements/">post</a>, I showed how you can configure NHibernate to batch create/update/delete statements and what kind of performance benefits you can get from it.  In this post, we're going to take this a bit further so we can actually use NHibernate in bulk data operations, an area where ORM's traditionally perform pretty badly.

First of all, let's get back to our test code from the last post:

<script src="https://gist.github.com/3684023.js?file=s1.cs"></script>

The only thing that changed since the previous post is the amount of objects that are created. In the previous post we only created 10000 objects, whereas now we'll be creating 500000 objects.

The batch size is configured like this:

<script src="https://gist.github.com/3684023.js?file=s2.xml"></script>

This means that NHibernate will send its DML statements in batches of 100 statements instead of sending all of them one by one.  The above code runs in 2 minutes and 24 seconds with a batch size of 100.  

However, if we use NHibernate's IStatelessionSession instead of a regular ISession, we can get some nice improvements.  First of all, here's the code to use the IStatelessSession:

<script src="https://gist.github.com/3684023.js?file=s3.cs"></script>

As you can see, apart from the usage of the IStatelessSession instead of the regular ISession, this is pretty much the same code.

With a batch-size of 100, this code creates and inserts the 500000 records in 1 minute and 26 seconds.  While not a spectacular improvement, it's definitely a nice improvement in duration.

The biggest difference however is in memory usage while the code is running. A regular NHibernate ISession keeps a lot of data in its first-level cache (this enables a lot of the NHibernate magical goodies).  The IStatelessSession however, does no such thing.  It does no caching whatsoever and it also doesn't fire all of the events that you could usually plug into.  This is strictly meant to be used for bulk data operations.

To give you an idea on the difference in memory usage, here are the memory statistics (captured by Process Explorer) after running the original code (with the ISession instance):

<a href="/postcontent/isession.png"><img src="/postcontent/isession.png" alt="" title="isession" width="398" height="385" class="alignnone size-full wp-image-551" /></a>

And here are the memory statistics after running the modified code (with the IStatelessSession instance):

<a href="/postcontent/istatelesssession.png"><img src="/postcontent/istatelesssession.png" alt="" title="istatelesssession" width="400" height="386" class="alignnone size-full wp-image-552" /></a>

Quite a difference for what is essentially the same operation.  We could even improve on this because the code in its current form keeps all of the object instances in its own collection, preventing them from being garbage collected after they have been inserted in the database.  But I think this already demonstrates the value in using the IStatelessSession if you need to perform bulk operations.

Obviously, this will never perform as well as a bulk data operation that directly uses low-level ADO.NET code.  But if you already have the NHibernate mappings and infrastructure set up, implementing those bulk operations could be cheaper while still being 'fast enough' for most situations.