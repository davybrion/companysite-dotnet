Update: As of NHibernate 3.1, this is no longer an issue.

One of the applications we’re currently working on wasn’t responding anymore on the development server. The logfile showed the following message:

*System.InvalidOperationException: Timeout expired. The timeout period elapsed prior to obtaining a connection from the pool. This may have occurred because all pooled connections were in use and max pool size was reached.*

We’ve been using NHibernate in almost all of our projects for about 2 years now and we’ve never had this issue before. The thing is though, we never used System.Transactions’ TransactionScope class before and always used NHibernate’s transactions directly. In this project, we’re using TransactionScope for the first time (mostly because we need distributed transactions occasionally) so I immediately suspected that we were either doing something wrong with it, or that there was a bug somewhere.

After a while I managed to reproduce the problem with the following simple test fixture:

<script src="https://gist.github.com/3693313.js?file=s1.cs"></script> 

If you assign counter a value that is greater than your ‘Max Pool Size’ value of your connection string, then you will get the following exception once you’re going through the loop for the ‘Max Pool Size’ + 1 time:

*System.InvalidOperationException : Timeout expired. The timeout period elapsed prior to obtaining a connection from the pool. This may have occurred because all pooled connections were in use and max pool size was reached.*

Bingo!

In normal situations, you would obviously call the Complete method of the TransactionScope instance before it is disposed to commit the current database transaction. But in a situation where you can’t call the Complete method (as in: when an exception has occurred and you want to rollback the transaction), your database connection will actually leak until you’ve used up all of the connections in your connection pool. 

So the question is: How can we use a TransactionScope with NHibernate without leaking connections when we throw an exception (which should result in an automatic rollback of the current transaction)? 

I was always under the impression that merely opening an NHibernate session within a transaction scope was actually OK and that the session’s connection would make use of the current ambient transaction (the one created by the outer TransactionScope). Pretty much every session-related action you perform in NHibernate results in calling the CheckAndUpdateSessionStatus method of the SessionImpl class, which in turn calls the EnlistInAmbientTransactionIfNeeded method. That (as you can probably guess) will automatically enlist the current NHibernate session in the ambient transaction, which is the one that was created by the TransactionScope. So, as I understand it, it should indeed be sufficient to open an NHibernate session within a TransactionScope to have every action performed by that session being enlisted in the current ambient transaction. And it actually behaves as transactional as you’d expect it to be. Your transaction is properly committed if you call the Complete method of the TransactionScope and it is indeed rolled back if you do not call the Complete method. The only problem (and it’s a major one obviously) is that you’re connection will leak when the transaction is rolled back.

Interestingly enough, if we change the test code to this:

<script src="https://gist.github.com/3693313.js?file=s2.cs"></script>

Then the problem goes away. Transactions are correctly rolled back, and none of the connections leak anymore. It’s too bad that we still need to use NHibernate’s transactions only for the sake of being able to rollback in case of failure without leaking the connection, because again, when you’re not using an NHibernate transaction but are within a TransactionScope, your Session’s connection will indeed be enlisted in the transaction being governed by the TransactionScope. So there certainly appears to be some kind of bug in NHibernate when it comes to cleaning up the connection of a session that has been enlisted in a TransactionScope’s transaction which is rolled back instead of committed.

Now, since I frequently see people on the intarweb showing examples of using an NHibernate session together with a TransactionScope yet without using an NHibernate transaction, I'd might as well post the way that I believe is the safest, and that shouldn’t leak any connections.

<script src="https://gist.github.com/3693313.js?file=s3.cs"></script>

If you need to rollback, simply throw an exception within the using block of the NHibernate transaction (before the transaction.Commit() call obviously) and everything takes care of itself. </p>