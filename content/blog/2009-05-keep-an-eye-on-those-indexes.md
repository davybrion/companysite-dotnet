We have a multi-tenant application, where each tenant has its own database. We recently were informed about a particular performance problem that one tenant (which we'll refer to as Tenant A) was experiencing in every screen where data of a certain type needed to be shown.  None of the other tenants experienced this problem though.

We tracked down the query that was causing the bad performance and ran it on the database of Tenant B.  Tenant B actually had a lot more data in the main table that was used in the query and the query executed immediately whereas it took about 25 seconds to complete for Tenant A.   So the query runs fast on another database that actually has more data... at this point i was convinced that it had to be related to indexes.

Turns out that someone recently ran an import process to import a bunch of data in Tenant A's database. I know very little about databases, but one thing i've seen time and time again (with both Oracle and SQL Server) is that you really need to make sure that your indexes are in good shape after any process that performs a lot of inserts (or removals).   A couple of years ago, i had a very intensive nightly import process for a particular project that used an Oracle database.  As time went on, the application's queries became painfully (unacceptably even) slow.  I managed to restore the performance of those queries by simply instructing Oracle to recalculate all of the statistics of the indexes of tables that were affected heavily during the nightly import.

With that in mind, we simply rebuilt the indexes for Tenant A's database, and the same query that took 25 seconds completed almost instantly from then on.  Now, we did had a weekly job running on that database server to keep the indexes in a healthy shape but that job didn't really do a good umm... job of it, apparently.  

Lessons learned: make sure that you:
<ul>
	<li>Have a proper maintenance job set up which keeps your indexes healthy and schedule it to run regularly</li>
	<li>Run that job manually if you need to perform a manual import process</li>
	<li>Execute that job in an automated fashion whenever an intensive automated import process has completed</li>
</ul>

Oh, and consult with your DBA's or at least people who know what they're doing when it comes to your particular database on how to keep those indexes healthy.  In this case, we rebuilt them.  In other cases it's sufficient to recalculate the statistics... i'm not sure which way is the best but you should at least keep an eye on this possible problem :) 