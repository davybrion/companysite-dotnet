Events in .NET are very useful. But if you're not careful, they might prevent objects from being removed from memory by the Garbage Collector (GC).  Let's call an object that publishes an event a Publisher and an object that subscribes to an event a Subscriber. If a Subscriber subscribes to a Publisher's event, the Publisher will indirectly hold a reference to the Subscriber (because of how events in .NET are implemented).  If the Publisher is an object that will stay around for a long time, this could prevent the Subscriber from being removed from memory when it's no longer used.

Suppose we have the following Publisher class:

<script src="https://gist.github.com/3676624.js?file=s1.cs"></script>

And then we have a BadSubscriber class:

<script src="https://gist.github.com/3676624.js?file=s2.cs"></script>

Notice how the BadSubscriber subscribes to the Publisher's event, but never unsubscribes from it.

Then we have the GoodSubscriber:

<script src="https://gist.github.com/3676624.js?file=s3.cs"></script>

The GoodSubscriber is very similar to the BadSubscriber, but it unsubscribes from the Publisher's event when it is disposed.  Note that the implementation of the IDisposable interface is hardly ideal, but for the purpose of this demo it's sufficient.

The following code illustrates the problem:

<script src="https://gist.github.com/3676624.js?file=s4.cs"></script>

The output of that code is the following:

the publisher notified the bad subscriber of an event<br/>
the publisher notified the good subscriber of an event<br/>
good publisher is gone<br/>
bad publisher is alive<br/>
the publisher notified the bad subscriber of an event<br/>

Notice how the GoodSubscriber is removed from memory by the GC and how the BadSubscriber is still in memory even though we no longer have a reference to it and the GC has performed a collection.

So keep in mind that every event handler you subscribe to an event should be properly unsubscribed from the event when your instance is no longer needed. Not doing so might lead to instances not being removed if the event's publisher has a longer lifetime than your object will have.  I'd actually suggest you'd always properly unsubscribe from any event you subscribe to.  Even if you know that the Publisher won't have a longer lifetime than your instance will have, who's to say some future change in the code won't extend the lifetime of the Publisher over that of the Subscriber? When that happens, you might end up with a bunch of instances that will remain in memory as long as the Publisher remains in memory.
