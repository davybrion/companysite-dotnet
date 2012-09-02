Ayende has released his NHibernate Profiler into private beta and i was lucky enough to be able to play around with it.

There's no installer, so it's just xcopy deployment.  I don't really like installers so the xcopy deployment is actually a plus in my book.  I just put the files into an NH-Prof folder and that was that.  Using it in a project currently requires adding a reference to one of the profiler's assemblies and then adding the following line of code in your application's startup routine:

<code>
<div style="font-family: Consolas; font-size: 10pt; color: black; background: white;">
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; HibernatingRhinos.NHibernate.Profiler.Appender.<span style="color: #2b91af;">NHibernateProfiler</span>.Initialize();</p>
</div>
</code>

After that, you simply start the client and run your application.  As soon as NHibernate does something, you can see the results immediately in the profiler:

<a href="http://davybrion.com/blog/wp-content/uploads/2008/12/nhprof1.png"><img src="http://davybrion.com/blog/wp-content/uploads/2008/12/nhprof1.png" alt="" title="nhprof1" width="500" height="353" class="aligncenter size-full wp-image-646" /></a>

My test application immediately sends 2 queries in a MultiCriteria batch which is correctly picked up by the profiler as one statement.  It also immediately alerts me that i executed those queries outside of a transaction. Pretty nice.

The profiler also can give you more information on the number of entities that were loaded in a session, and it can also show you which entities where loaded.  But it looks like there's still a bug there:

<a href="http://davybrion.com/blog/wp-content/uploads/2008/12/nhprof2.png"><img src="http://davybrion.com/blog/wp-content/uploads/2008/12/nhprof2.png" alt="" title="nhprof2" width="500" height="353" class="aligncenter size-full wp-image-647" /></a>

As you can see from the SessionFactory statistics, we've loaded 37 entities.  Now, the SessionFactory statistics keeps statistics for all Sessions it created, but it also clearly shows that there has only been one used Session so far.  The profiler however says that there were no entities loaded in this session:

<a href="http://davybrion.com/blog/wp-content/uploads/2008/12/nhprof3.png"><img src="http://davybrion.com/blog/wp-content/uploads/2008/12/nhprof3.png" alt="" title="nhprof3" width="500" height="353" class="aligncenter size-full wp-image-648" /></a>

In my second Session in the application, i use one query to retrieve the entities of 3 different classes.  You can easily get NHibernate to print the SQL statements it generates, but they are not formatted so they're not always very readable.  This profiler makes the generated statements a lot easier to read:

<a href="http://davybrion.com/blog/wp-content/uploads/2008/12/nhprof4.png"><img src="http://davybrion.com/blog/wp-content/uploads/2008/12/nhprof4.png" alt="" title="nhprof4" width="500" height="353" class="aligncenter size-full wp-image-649" /></a>

But now that i've executed my second Session, i can suddenly see the entity details of the first Session:

<a href="http://davybrion.com/blog/wp-content/uploads/2008/12/nhprof5.png"><img src="http://davybrion.com/blog/wp-content/uploads/2008/12/nhprof5.png" alt="" title="nhprof5" width="500" height="353" class="aligncenter size-full wp-image-650" /></a>

<br/>

<a href="http://davybrion.com/blog/wp-content/uploads/2008/12/nhprof6.png"><img src="http://davybrion.com/blog/wp-content/uploads/2008/12/nhprof6.png" alt="" title="nhprof6" width="500" height="353" class="aligncenter size-full wp-image-651" /></a>

The SessionFactory's statistics show that we've already loaded 151 entities, and 37 of those were actually loaded in the first session.  Those 37 entities are shown in the details of the second session however and i have no idea which entities where loaded in the second Session.  Hopefully it's an easy bug to fix because this is a piece of functionality that looks very useful for troubleshooting.

Another thing i like a lot is how the profiler properly replaces parameter values in the formatted queries:

<a href="http://davybrion.com/blog/wp-content/uploads/2008/12/nhprof7.png"><img src="http://davybrion.com/blog/wp-content/uploads/2008/12/nhprof7.png" alt="" title="nhprof7" width="500" height="353" class="aligncenter size-full wp-image-652" /></a>

Now you can easily just copy/paste the formatted query and execute it in whatever tool you prefer to see the actual output of queries.

Now, as you could see, there was a query where i used joins to combine the result of 3 different tables.  If i would rewrite the code so i don't use the joins and simply rely on NHibernate's lazy loading to fetch the related data, the profiler would show me something like this:

<a href="http://davybrion.com/blog/wp-content/uploads/2008/12/nhprof8.png"><img src="http://davybrion.com/blog/wp-content/uploads/2008/12/nhprof8.png" alt="" title="nhprof8" width="500" height="265" class="aligncenter size-full wp-image-655" /></a>

As you can see, the profiler detects the notorious SELECT N+1 problem which is a very common mistake that a lot of people make with ORMs, or generated DALs for that matter.  But wait, it gets better:

<a href="http://davybrion.com/blog/wp-content/uploads/2008/12/nhprof9.png"><img src="http://davybrion.com/blog/wp-content/uploads/2008/12/nhprof9.png" alt="" title="nhprof9" width="500" height="284" class="aligncenter size-full wp-image-656" /></a>

It also notifies you when you send too many queries in a Session! Very neat stuff.

I've only used the NHibernate Profiler on a small test application and i'm already pretty impressed. In the next couple of days i'm going to experiment with it on a real application and then i'll post some more feedback on it.

Update: the bug i mentioned is already fixed in a newer build :)