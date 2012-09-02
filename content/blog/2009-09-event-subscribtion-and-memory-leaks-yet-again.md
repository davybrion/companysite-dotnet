Just read the following <a href="http://weblogs.asp.net/fmarguerie/archive/2009/09/09/forcing-event-unsubscription.aspx">post</a> by Fabrice Marguerie about forcing event unsubscription to avoid the typical memory leaks that occur frequently with event handlers.  Unfortunately, the solution in his post will in most cases be useless.

First of all, consider the following 2 classes:

<div>
[csharp]
        public class Publisher
        {
            public event EventHandler&lt;EventArgs&gt; MyEvent = delegate { };
 
            public void RaiseEvent()
            {
                MyEvent(this, EventArgs.Empty);
            }
        }
 
        public class Subscriber
        {
            private static int counter;
 
            private int currentInstanceCount;
 
            public Subscriber(Publisher publisher)
            {
                currentInstanceCount = ++counter;
                publisher.MyEvent += producer_MyEvent;
            }
 
            void producer_MyEvent(object sender, EventArgs e)
            {
                Console.WriteLine(string.Format(&quot;Instance {0} is handling event...&quot;, currentInstanceCount));
            }
        }
[/csharp]
</div>

Pretty simple... the Publisher publishes the MyEvent event, and the Subscriber subscribes to the event when an instance is created.  As you should already know, <strong>whenever the publisher of an event has a longer lifecycle than its subscibers, subscribers must explicitly unsubscribe from the event to prevent instances of the subscribers to remain in memory instead of being garbage collected</strong>.

The following simple example illustrates the problem:

<div>
[csharp]
            var publisher = new Publisher();
 
            var subscriber1 = new Subscriber(publisher);
            var subscriber2 = new Subscriber(publisher);
 
            Console.WriteLine(&quot;triggering event for the first time... both subscribers should respond&quot;);
            publisher.RaiseEvent();
 
            subscriber1 = null;
            GC.Collect();
 
            Console.WriteLine(&quot;triggering event for the second time... hopefully, the first subscriber has been collected&quot;);
            publisher.RaiseEvent();
 
            subscriber2 = null;
            GC.Collect();
 
            Console.WriteLine(&quot;triggering event for the third time... hopefully, both subscribers have been collected&quot;);
            publisher.RaiseEvent();
[/csharp]
</div>

This will produce the following console output:

triggering event for the first time... both subscribers should respond <br/>
Instance 1 is handling event... <br/>
Instance 2 is handling event... <br/>
triggering event for the second time... hopefully, the first subscriber has been collected <br/>
Instance 1 is handling event... <br/>
Instance 2 is handling event... <br/>
triggering event for the third time... hopefully, both subscribers have been collected <br/>
Instance 1 is handling event... <br/>
Instance 2 is handling event... <br/>

Obviously, both subscriber instances remained in memory even though we no longer had references to those instances and they should've been collected by the garbage collector.  For those of you who don't know what's going on: an event handler will internally keep a list of references to all the objects that subscribed to the event.  This means that it is only normal that the Publisher is actually keeping all of the Subscriber instances 'live' because it keeps references to it.  That means that the garbage collector will never collect these instances <strong>until the Producer is eligible for garbage collection</strong>.

Now, Fabrice's proposed solution is to change the Producer to this:

<div>
[csharp]
        public class Publisher : IDisposable
        {
            public event EventHandler&lt;EventArgs&gt; MyEvent = delegate { };
 
            public void RaiseEvent()
            {
                MyEvent(this, EventArgs.Empty);
            }
 
            public void Dispose()
            {
                MyEvent = null;
            }
        }
[/csharp]
</div>

Again, memory leaks through event subscription only occur when the publisher of an event lives longer than the subscribers of an event.  Obviously, this solution will hardly help in the situation where the memory actually leaks, because the publisher will not be disposed of until it is no longer needed.  Which means that the Producer will still keep all instances from no-longer-needed subscribers in memory.

The only way to avoid this problem is to explicitly unsubscribe each subscriber from the event.  Which means that you should implement IDisposable on the Subscriber:

<div>
[csharp]
        public class Subscriber : IDisposable
        {
            private static int counter;
 
            private int currentInstanceCount;
            private Publisher publisher;
 
            public Subscriber(Publisher publisher)
            {
                currentInstanceCount = ++counter;
                this.publisher = publisher;
                publisher.MyEvent += producer_MyEvent;
            }
 
            void producer_MyEvent(object sender, EventArgs e)
            {
                Console.WriteLine(string.Format(&quot;Instance {0} is handling event...&quot;, currentInstanceCount));
            }
 
            public void Dispose()
            {
                publisher.MyEvent -= producer_MyEvent;
            }
        }
[/csharp]
</div>

That's not enough though... you need to explicitly dispose these instances as well.  So the example code now looks like this:

<div>
[csharp]
            var publisher = new Publisher();
 
            var subscriber1 = new Subscriber(publisher);
            var subscriber2 = new Subscriber(publisher);
 
            Console.WriteLine(&quot;triggering event for the first time... both subscribers should respond&quot;);
            publisher.RaiseEvent();
 
            subscriber1.Dispose();
            subscriber1 = null;
            GC.Collect();
 
            Console.WriteLine(&quot;triggering event for the second time... hopefully, the first subscriber has been collected&quot;);
            publisher.RaiseEvent();
 
            subscriber2.Dispose();
            subscriber2 = null;
            GC.Collect();
 
            Console.WriteLine(&quot;triggering event for the third time... hopefully, both subscribers have been collected&quot;);
            publisher.RaiseEvent();
[/csharp]
</div>

And the output of this code is now correct:

triggering event for the first time... both subscribers should respond <br/>
Instance 1 is handling event... <br/>
Instance 2 is handling event... <br/>
triggering event for the second time... hopefully, the first subscriber has been collected <br/>
Instance 2 is handling event... <br/>
triggering event for the third time... hopefully, both subscribers have been collected <br/>

To summarize:

<ul>
	<li><p>If a producer will stay alive longer than the subscriber, then it will keep the subscribers in memory until the publisher gets garbage collected</p></li>
	<li><p>Your subscribers should explicitly unsubscribe from events from publishers, unless you know that the lifetime of both the subscriber or the publisher is either identical, or that the publisher has a shorter lifetime than the subscriber.  You should preferably unsubscribe from these events in a Dispose method and your subscriber should implement IDisposable.</p></li>
	<li><p>Always call the Dispose method on subscribers that implement IDisposable. Otherwise, you'll still leak instances of the subscribers.</p></li>
</ul>