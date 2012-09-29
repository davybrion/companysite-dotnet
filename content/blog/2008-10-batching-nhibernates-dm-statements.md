An oft-forgotten feature of NHibernate is that of batching DML statements.  If you need to create, update or delete a bunch of objects you can get NHibernate to send those statements in batches instead of one by one.  Let's give this a closer look.

I have an 'entity' with the following mapping:

<script src="https://gist.github.com/3684012.js?file=s1.xml"></script>

Nothing special here, just a Guid Id field and a string Description field. 

First, let's see how much time it takes to create 10000 records of this without using the batching feature.  I use the following method to create a bunch of dummy objects:

<script src="https://gist.github.com/3684012.js?file=s2.cs"></script>

Then, the code to persist these objects:

<script src="https://gist.github.com/3684012.js?file=s3.cs"></script>

Without enabling the batching, this code took 23 seconds to run on my cheap MacBook.  Now let's enable the batching in the hibernate.cfg.xml file:

<script src="https://gist.github.com/3684012.js?file=s4.xml"></script>

A batch size of 5 is still very small, but for this test it means that it only has to do 2000 trips to the database instead of the original 10000.  The code above now runs in 5.5 seconds.  Setting the batch size to 100 made it run in 1.8 seconds.  Going from 23 to 1.8 seconds with a small configuration change is a pretty nice improvement with very little effort.  Obviously, these aren't real benchmarks so your results may vary but I think it does show that you can easily get some performance benefits from it.

You can get performance benefits like this whenever you need to create/update/delete a bunch of records simply by enabling this setting.  Keep in mind that this batching of statements doesn't apply to select queries... for that you need to use NHibernate's MultiCriteria or MultiQuery features :)

Another thing to keep in mind is that for this test I used the 'assigned' Id generator... which means that the developer is responsible for providing the Id value for new objects.  One of the consequences of this is that NHibernate does not have to go to the database to retrieve the Id values like it would have to do if you were using (for instance) Identity Id values.  If you were using the Identity Id generator, this configuration setting would have no effect whatsoever for inserts, although the benefits would still apply to update and delete statements.

Note that this approach is good for regular applications, but it's still not good enough if you need to process very large data sets (like import processes and things of that nature). Obviously, an ORM isn't well suited for those purposes, but we will examine another NHibernate feature in a future post which makes it possible to use NHibernate in such bulk operations with a pretty low performance overhead.