I'm currently reading <a href="http://www.amazon.com/Release-Production-Ready-Software-Pragmatic-Programmers/dp/0978739213/ref=pd_bbs_sr_1?ie=UTF8&s=books&qid=1210668155&sr=8-1">Release It</a> and in it, the author really stresses the importance of not having any blocking-calls in your code. The reason is fairly simple, if something goes wrong in a blocking call, the thread executing it might hang forever.  If you're servicing requests, you surely don't want all of your request-handling threads to hang forever because before you know it, all of the threads will be blocked and no requests will be dealt with.

You may remember my post about <a href="http://davybrion.com/blog/2008/03/thread-safe-repositories/">thread-safe repositories</a> from a while ago. In the code from that post, i use the lock keyword to make sure instances of the class are thread-safe. The problem with the lock keyword is that it uses the Monitor.Enter method, which blocks indefinitely until it can acquire a lock on the object you pass to it.  And since you should avoid blocking calls, you really should use Monitor.TryEnter instead, because it allows you to set a timeout. If the lock can't be acquired within the timeout period, the method simply returns false and you did not get a lock. This makes it possible to avoid blocking threads and deadlocks.

The problem is that the lock keyword makes everything pretty easy... it guarantees that the lock is released (by calling Monitor.Exit) when leaving the code block, whatever may have possible went wrong.  So if you want to use Monitor.TryEnter, you basically have to use a try/finally everytime.  Not only does that make the code more ugly, it's so boring and tedious that it's easy to screw up once in a while.  So i started googling for a different approach, and luckily, Ian Griffith has two <a href="http://www.interact-sw.co.uk/iangblog/2004/03/23/locking">excellent</a> <a href="http://www.interact-sw.co.uk/iangblog/2004/04/26/yetmoretimedlocking">posts</a> on the subject.  He basically uses a TimedLock struct in a using block... when you request the lock, you specify a timeout value, and when the lock can't be acquired within the given timeout period, it throws a LockTimeOutException.  Here's a (simplified) version of the TimedLock struct:

<div>
[csharp]
    public struct TimedLock : IDisposable
    {
        private readonly object target;
 
        private TimedLock(object o)
        {
            target = o;
        }
 
        public void Dispose()
        {
            Monitor.Exit(target);
        }
 
        public static TimedLock Lock(object o)
        {
            return Lock(o, TimeSpan.FromSeconds(5));
        }
 
        public static TimedLock Lock(object o, TimeSpan timeout)
        {
            return Lock(o, timeout.Milliseconds);
        }
 
        public static TimedLock Lock(object o, int milliSeconds)
        {
            var timedLock = new TimedLock(o);
 
            if (!Monitor.TryEnter(o, milliSeconds))
            {
                throw new LockTimeoutException();
            }
 
            return timedLock;
        }
    }
 
    public class LockTimeoutException : ApplicationException
    {
        public LockTimeoutException() : base(&quot;Timeout waiting for lock&quot;) {}
    }
[/csharp]
</div>

The version on his blog also has a way of detecting unreleased locks when you're in debug mode. 

So now you can basically change the following code:

<div>
[csharp]
        public virtual T Get(Id id)
        {
            lock(MonitorObject)
            {
                if (!Members.ContainsKey(id))
                {
                    return null;
                }
 
                return Members[id];
            }
        }
[/csharp]
</div>

To this:

<div>
[csharp]
        public virtual T Get(Id id)
        {
            using (TimedLock.Lock(MonitorObject, 250))
            {
                if (!Members.ContainsKey(id))
                {
                    return null;
                }
 
                return Members[id];
            }
        }
[/csharp]
</div>

So now it tries to acquire a lock on the MonitorObject instance with a timeout of 250 milliseconds. If it can't acquire the lock, something is wrong and the LockTimeOutException is thrown. Which is a lot better than blocking indefinitely :)
