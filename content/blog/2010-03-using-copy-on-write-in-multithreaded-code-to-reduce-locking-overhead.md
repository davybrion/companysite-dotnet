I recently <a href="http://davybrion.com/blog/2010/02/wanna-review-my-code/" target="_blank">posted some code</a> that i asked you to review.&#160; When i posted it, the code had never even executed (that’s right, not even through a test) and i only <em>thought</em> it would do what i needed it to do.&#160; I consider the actual implementation non-obvious (at least for those who don’t know the copy-on-write approach to avoid traditional locking) so i just wanted to hear some reactions to the code from people who didn’t knew the context.&#160; I promised to do a follow-up post to discuss the code in its entirety so here it is.

First, i’ll show the whole class again:

<script src="https://gist.github.com/3693200.js?file=s1.cs"></script>

Basically, the purpose of this class is to hold a set of ISessionFactory instances, each of which belongs to a particular tenant in a multi-tenant application.&#160; Tenants can be added on the fly (without restarting the application) and when an ISessionFactory doesn’t exist yet for a particular tenant, it must be created when the first request for an ISession for that tenant comes in.&#160; Obviously, access to the sessionFactories dictionary must be thread-safe since multiple threads will be reading from the dictionary as well as occasionally writing to it.

I considered 3 options to make sure access to the dictionary would be thread-safe:

- Traditional locking (through the lock statement or the Monitor class)
- Using the <a href="http://msdn.microsoft.com/en-us/library/system.threading.readerwriterlockslim.aspx" target="_blank">ReadWriterLockSlim</a> class
- Using the copy-on-write pattern

Traditional locking was quickly scratched from the list because that would require me to lock for every read of the dictionary as well as every write.&#160; Now, pretty much every single request requires an NHibernate session which means that pretty much every single request results in a lookup in the sessionFactories dictionary.&#160; If i need to lock for every read, this significantly hurts overall throughput of the system.&#160; 

The ReadWriterLockSlim might be a good solution here… after all, the short description of this class in MSDN says this:

<blockquote>Represents a lock that is used to manage access to a resource, allowing multiple threads for reading or exclusive access for writing.</blockquote>  

Sounds like what i need, right?&#160; But the thing is, i’ve never used the ReadWriterLockSlim class before and it hasn’t really gained my trust yet.&#160; I know that’s a terrible excuse for not using it, but here me out.&#160; While the ReadWriterLockSlim likely reduces locking overhead over traditional locking substantially, there still has to be <em>some</em> overhead for read operations, even if it is small.&#160; In most situations, that small overhead wouldn’t bother me but in this case, that little overhead would be added to pretty much <em>every single request</em> in the system.&#160; Now, writing to a dictionary implies that a new tenant has been added to the system.&#160; In the context of this system, that’s not even gonna happen on a daily basis.&#160; Hell, once a week is probably a best-case estimation and even that is highly optimistic.&#160; So i really don’t want any kind of overhead on read operations when the write operation is only going to happen very occasionally.

That leaves the copy-on-write pattern.&#160; I’ve used it <a href="http://davybrion.com/blog/2009/02/challenge-do-you-truly-understand-this-code/" target="_blank">before</a> with success (though at the time, i didn’t know it was a known pattern) so this approach has already gained my trust.&#160; It basically implies that we don’t do <em>any</em> locking on the read operations, but whenever a write operation occurs we copy the original set of objects, perform the write on the newly copied set and then set the reference of the original set to the newly created and modified instance.&#160; During this whole time, every <em>single</em> read is safe.&#160; Successive reads within the same logical operation however aren’t, so the following code would not be thread-safe:

<script src="https://gist.github.com/3693200.js?file=s2.cs"></script>

Because there’s no locking on the reads, the code within the if-block could fail because the sessionFactories reference could be pointing to a new dictionary which no longer contains the element for that key.&#160; 

Of course, if you have frequent writes, the overhead of copying the set of objects every time you need to add/remove one might be bigger than you want, so this isn’t a pattern that you should use whenever you need to protect access to a shared resource. For this situation however, i think it’s ideal… though i’d obviously like to hear about better solutions. 

Now, let’s take a closer look at the pieces of code that perform the write operations.&#160; First, adding a new ISessionFactory to the dictionary:

<script src="https://gist.github.com/3693200.js?file=s3.cs"></script>

As you can see, the entire operation is put between a lock on the writeLock object instance.&#160; The downside of this is that creating an ISessionFactory instance is an expensive operation, which means the lock will be held for a long time (could easily be one or more <em>seconds</em>).&#160; Then again, i don’t anticipate this happening frequently so it’s not that big of an issue… especially since reads aren’t being blocked by this anyway.&#160; This approach also prevents the creation of 2 ISessionFactory instances for the same tenant.&#160; Well, unless i missed a bug here :p

Now, once the ISessionFactory instance is created, we create a new Dictionary based on the contents of the old one and then we add the new ISessionFactory instance to it.&#160; After that, we replace the sessionFactories references with the new dictionary and from that point on, every read will use the new dictionary instance.&#160; During this entire operation, no read operation was impacted negatively.&#160; 

Lets take a look at the other write operation, removing an ISessionFactory instance from the dictionary:

<script src="https://gist.github.com/3693200.js?file=s4.cs"></script>

The first if-check, which happens outside of the lock is a bug that i missed but that was pointed out in the comments of the original post.&#160; If CreateSessionFactoryForCurrentTenant and RemoveSessionFactoryForTenant would execute concurrently for the same tenant, it’s possible that the ISessionFactory instance of that tenant is never removed from the dictionary (and also never disposed of…) since the check happens outside of the lock and could be executed before the ISessionFactory of the tenant was added to the dictionary.&#160; In that case, the ISessionFactory instance would stay in the dictionary as long as the application stays up.&#160; This is definitely a race condition that you want to avoid in every other situation though in this case, the odds that we’re simultaneously adding and removing the same tenant are slim to none.&#160; Nevertheless, i don’t want to be accused of promoting race conditions so we’ll make the change anyway.

<script src="https://gist.github.com/3693200.js?file=s5.cs"></script> 

Now, as you can see we once again create a new dictionary based on the previous one, then remove the ISessionFactory instance for the current tenant and then we overwrite the sessionFactories instance once again. </p>  <p>Finally, there’s the read operation that i specifically didn’t want suffering from locking overhead:

<script src="https://gist.github.com/3693200.js?file=s6.cs"></script>

The only time this code will block is when a new ISessionFactory for the current tenant needs to be created.&#160; Luckily, that only happens once for each tenant.&#160; As i mentioned earlier in the post, using this pattern doesn’t guarantee that successive reads within the same logical operation are thread safe, so there is a bug in here.&#160; If a tenant already has an ISessionFactory instance, it’s possible that the RemoveSessionFactoryForTenant method has been executed between the if-check and accessing the ISessionFactory based on the tenantId.&#160; In that particular scenario, the ISessionFactory instance is no longer in the dictionary which will cause this code to throw an exception. That’s a bug that i don’t feel like fixing though… Once a tenant has been removed, they are no longer a paying customer.&#160; If they are no longer paying for the software, there is no reason whatsoever why i should care about any possible exceptions they could get while running the software :)

Seriously though, if the RemoveSessionFactoryForTenant method is called, users of that tenant won’t even have access to the system anymore so it’s really a non-issue.

Anyways, i think i’ve covered the implementation in more detail than you probably cared for.&#160; So, any thoughts? Are there still issues that i haven’t thought of? Is there another approach that you would use for this specific scenario?