I’ve spent about 3 days trying to get something working that should’ve just worked. I basically wanted some .NET code to use a distributed transaction to update some data in a database, and then publish a message on the service bus. I want to do this in a distributed transaction because if something goes wrong, I want to roll back both transactions (the database change and the published message). Normally, this should just work if you have MS DTC configured correctly. On my machine, I enabled Network DTC Access, and allowed outbound transaction communication. On the database server, Network DTC Access was already enabled and both outbound and inbound communication was allowed. 

Now the thing is, I'd either expect DTC to fail outright or to just work. But it shouldn’t fail in one situation, and work in another. On my machine, it failed in the following situation (which I'll further refer to as Situation A):

- open a transaction scope
- open an nhibernate session
- hit the db
- publish a message through nservicebus
- close the nhibernate session
- complete and close the transaction scope

Step 4 and 5 could be switched around but it didn’t make a difference. In Situation A, I always got a TransactionManagerCommunicationException with the following message:

*Network access for Distributed Transaction Manager (MSDTC) has been disabled. Please enable DTC for network access in the security configuration for MSDTC using the Component Services Administrative tool.*

Everyone who’s worked with MSDTC before probably knows that exception since it usually takes some fiddling with the settings to make things work. The thing is, I was pretty sure that my settings, as well as the ones on the database server were correct. Unfortunately, DTCPing didn’t confirm that since that too failed.

However, I also tried the following sequence of events (Situation B):

- open a transaction scope
- open an nhibernate session
- publish a message
- hit the db
- close the nhibernate session
- complete and close the transaction scope

And guess what. That actually worked. With full DTC transaction semantics. The DTC statistics on the server confirmed that it was indeed using a DTC transaction, and if I made the code fail with an exception both the database action and the published message were correctly rolled back.

So the question is: why on earth does it only work when I publish a message before I hit the db?

During my investigation I noticed that in Situation A, the internal transaction that the transaction scope was using was a SqlDelegatedTransaction. Which, if I'm not mistaken is an LTM transaction. When trying to send a message to a message queue, the transaction manager tries to promote the current transaction to an OletxCommittableTransaction since the OleTx transaction protocol is required when using MSMQ (it doesn’t support LTM transactions). For some reason, promoting the SqlDelegatedTransaction to a full DTC (OleTx) transaction fails on my machine.

In Situation B, the internal transaction is promoted to an OletxCommittableTransaction as soon as you try to send the message to a message queue. Once it’s time to hit the DB, NHibernate nicely works together with the OletxCommittableTransaction and everything just works. 

Now, I have no idea on earth why promotion of a SqlDelegatedTransaction fails, but after a long number of attempts and experiments to get it working correctly, I sorta gave up and figured I'd have to resort to a hack. What I basically needed was for the transaction scope’s internal transaction to automatically be promoted to an OletxCommittableTransaction <em>before</em> I'd hit the database and without having to publish a dummy message at the beginning of the transaction.

I found one way of doing this which, while being a huge hack, is still relatively clean I think. I wrote the following class:

<script src="https://gist.github.com/3693225.js?file=s1.cs"></script>

Then, right after opening the transaction scope and before doing anything else, I do this:

<script src="https://gist.github.com/3693225.js?file=s2.cs"></script>

This basically tells the System.Transactions infrastructure that we’re adding our own Resource Manager to the current transaction. And because it’s a durable Resource Manager, it now automatically promotes the internal transaction to an OletxCommittableTransaction and everything just works. While our Resource Manager participates in the 2-phase-commit process, it doesn’t actually do anything. It’s sole purpose is to force the creation of an OletxCommittableTransaction.

Like I said, it’s a hack but it’s still relatively clean. I still have no idea why I needed to resort to this hack though… If anyone can shed some light on this, I'd highly appreciate it :)

Also, if you ever want to learn more about transactions in .NET or distributed transactions in particular, you really need to check out <a href="http://www.codeproject.com/KB/WCF/NETTx.aspx" target="_blank">this article</a>. Without it, I probably wouldn’t have figured out what to do.