Calling expensive synchronous methods in succession can sometimes waste a lot of time.  If those synchronous methods spend most of their time waiting on external resources, you're probably better off executing those methods at the same time and waiting for all the results to come back instead of executing a method, waiting for its result, executing the next method, waiting for that ones result, etc...

take this code for instance:

<div>
[csharp]
            List&lt;UserPrincipal&gt; users = activeDirectoryFacade.GetAllUsers();
            List&lt;GroupPrincipal&gt; groups = activeDirectoryFacade.GetAllGroups();
[/csharp]
</div>

Both calls can take a while, and they spend practically all of their time waiting on an external resource. Unfortunately, in this case neither method has an asynchronous implementation. You've probably seen that a lot of .NET classes offer asynchronous implementations of some methods along with their synchronous implementations. If there's an Xxx method, the asynchronous methods will be named BeginXxx and EndXxx.  This can be very useful at times, but if no asynchronous method is available for the task you need to do, how do you effectively perform that task asynchronously?

It's not that hard... so let's give it a shot. First of all, we need to define the methods that we want to execute:

<div>
[csharp]
            Func&lt;List&lt;UserPrincipal&gt;&gt; fetchUsers = () =&gt; activeDirectoryFacade.GetAllUsers();
            Func&lt;List&lt;GroupPrincipal&gt;&gt; fetchGroups = () =&gt; activeDirectoryFacade.GetAllGroups();
[/csharp]
</div>

The GetAllUsers method is now defined as a function that returns a List of UserPrincipals. The GetAllGroups method is now defined as a function that returns a List of GroupPrincipals.  And now we can simply call them asynchronously like this:

<div>
[csharp]
            IAsyncResult fetchUserAsyncResult = fetchUsers.BeginInvoke(null, null);
            IAsyncResult fetchGroupsAsyncrResult = fetchGroups.BeginInvoke(null, null);
[/csharp]
</div>

The BeginInvoke method invokes the original method, but instead of waiting for the original method to finish, it immediately returns with an IAsyncResult instance which we'll use later on. The first parameter of the BeginInvoke method is an AsyncCallback instance, the second one can be any object that you want to pass along to the AsyncCallback.  The AsyncCallback is just another method that will be called when the original method has completed its task.  In this case, we don't use a callback so we simply pass null.

So now both original methods are executing, and our code needs to wait for the result.  We can either call the EndInvoke method on the Func instances, which will block until the original method has completed, or we can explicitely wait using a waithandle offered by the returned IAsyncResult:

<div>
[csharp]
            fetchUserAsyncResult.AsyncWaitHandle.WaitOne();
            fetchGroupsAsyncrResult.AsyncWaitHandle.WaitOne();
 
            List&lt;UserPrincipal&gt; userPrincipals = fetchUsers.EndInvoke(fetchUserAsyncResult);
            List&lt;GroupPrincipal&gt; groupPrincipals = fetchGroups.EndInvoke(fetchGroupsAsyncrResult);
[/csharp]
</div>

In this case, calling the WaitOne method of the waithandle blocks the current thread until the original method has finished its task. Keep in mind that while you wait on the first method, the second one might still be executing as well.  Waiting on the first method does not influence the execution of the second method at all.

So in the above piece of code, we explicitly wait for both methods to finish.  And then we call the EndInvoke method and pass the IAsyncResult to it, which will complete the entire operation and returns the return value of the original method.  We could've just as easily skipped calling the WaitOne method on the waithandles, and the call to EndInvoke would've blocked until the original method had finished as well.  However, i think the calls to WaitOne make the intent of the code more clear which makes the code easier to understand.

Anyways, as you can see, it's trivially easy to perform synchronous methods in an asynchronous manner.  This can sometimes offer tremendous advantages for performance, but you shouldn't go around using this all the time.  It only offers a benefit if you have a slow method somewhere and you can actually do some other work as well while the slow method is executing.
