We're experimenting at work with a bit of a different approach as to how we structure our views and how they will interact with each other.  We already know that our views will be reused in many different contexts so having them communicate with other views is something that needs to be done in a very loosely coupled manner.  I don't want any of the views to even know about the existence of other views, let alone having them know about specific instances of them.

But these views do have to interact with each other.  I didn't want to use typical events because that would require either a certain view to know about another view to be able to subscribe to its events, or you'd need some other component which knows which views need to be hooked to each other.  We really need maximum flexibility for what we have in mind with our views, so it only made sense to finally start using the <a href="http://martinfowler.com/eaaDev/EventAggregator.html">Event Aggregation pattern</a>.  The idea is that a view can basically publish events, without knowing who is subscribed to these events, and that suscribers will be notified whenever these events occur.  However, subscribers don't know anything about who is publishing the events.  Instead, both publishers and subscribers only know of the Event Aggregator.  A publisher tells the aggregator to publish an event to all subscribed listeners for that event.  Each subscribers simply tells the aggregator "i'd like to be notified whenever this event occurs, and i don't care where it comes from".  
Plenty of implementations of this pattern can be found online already, so i figured: why not add my own? :p

Fist of all, an event is nothing more than a class that inherits from this class:

<div>
[csharp]
    public abstract class Event {}
[/csharp]
</div>

Every event should inherit from this class, and add whatever necessary properties that are important for that particular type of event. 

If a class is interested in listening to a specific event, it needs to implement the following interface:

<div>
[csharp]
    public interface IListener { }
 
    public interface IListener&lt;TEvent&gt; : IListener
        where TEvent : Event
    {
        void Handle(TEvent receivedEvent);
    }
[/csharp]
</div>

If a class is interested in multiple events, it simply needs to implement the generic IListener interface for each type of event that it wants to handle.

Then we obviously need the Event Aggregator.  I wanted an aggregator that allowed listeners to either subscribe/unsubscribe to/from very specific events, or just subscribe/unsubscribe to/from whatever it supports.  So i have the following IEventAggregator interface:

<div>
[csharp]
    public interface IEventAggregator
    {
        void Publish&lt;TEvent&gt;(TEvent message) where TEvent : Event;
        void Publish&lt;TEvent&gt;() where TEvent : Event, new();
 
        void Subscribe(IListener listener);
        void Unsubscribe(IListener listener);
 
        void Subscribe&lt;TEvent&gt;(IListener&lt;TEvent&gt; listener) where TEvent : Event;
        void Unsubscribe&lt;TEvent&gt;(IListener&lt;TEvent&gt; listener) where TEvent : Event;
    }
[/csharp]
</div>

The Subscribe and Unsubscribe methods that simply take an IListener reference will either subscribe or unsubscribe the given listener to/from every event that it can handle.  In other words, for every generic IListener interface that it implements.  Yet you also have the ability to subscribe/unsubscribe from a specific event type.

And here's the implementation:

<div>
[csharp]
    public class EventAggregator : IEventAggregator
    {
        private readonly object listenerLock = new object();
        protected readonly Dictionary&lt;Type, List&lt;IListener&gt;&gt; listeners = new Dictionary&lt;Type, List&lt;IListener&gt;&gt;();
        private readonly IDispatcher dispatcher;
 
        public EventAggregator(IDispatcher dispatcher)
        {
            this.dispatcher = dispatcher;
        }
 
        public virtual void Subscribe(IListener listener)
        {
            ForEachListenerInterfaceImplementedBy(listener, Subscribe);
        }
 
        public virtual void Unsubscribe(IListener listener)
        {
            ForEachListenerInterfaceImplementedBy(listener, Unsubscribe);
        }
 
        private static void ForEachListenerInterfaceImplementedBy(IListener listener, Action&lt;Type, IListener&gt; action)
        {
            var listenerTypeName = typeof(IListener).Name;
 
            foreach (var interfaceType in listener.GetType().GetInterfaces().Where(i =&gt; i.Name.StartsWith(listenerTypeName)))
            {
                Type typeOfEvent = GetEventType(interfaceType);
 
                if (typeOfEvent != null)
                {
                    action(typeOfEvent, listener);
                }
            }
        }
 
        private static Type GetEventType(Type type)
        {
            if (type.GetGenericArguments().Count() &gt; 0)
            {
                return type.GetGenericArguments()[0];
            }
 
            return null;
        }
 
        public virtual void Subscribe&lt;TEvent&gt;(IListener&lt;TEvent&gt; listener) where TEvent : Event
        {
            Subscribe(typeof(TEvent), listener);
        }
 
        protected virtual void Subscribe(Type typeOfEvent, IListener listener)
        {
            lock (listenerLock)
            {
                if (!listeners.ContainsKey(typeOfEvent))
                {
                    listeners.Add(typeOfEvent, new List&lt;IListener&gt;());
                }
 
                if (listeners[typeOfEvent].Contains(listener))
                {
                    throw new InvalidOperationException(&quot;You're not supposed to register to the same event twice&quot;);
                }
 
                listeners[typeOfEvent].Add(listener);
            }
        }
 
        public virtual void Unsubscribe&lt;TEvent&gt;(IListener&lt;TEvent&gt; listener) where TEvent : Event
        {
            Unsubscribe(typeof(TEvent), listener);
        }
 
        protected virtual void Unsubscribe(Type typeOfEvent, IListener listener)
        {
            lock(listenerLock)
            {
                if (listeners.ContainsKey(typeOfEvent))
                {
                    listeners[typeOfEvent].Remove(listener);
                }
            }
        }
 
        public virtual void Publish&lt;TEvent&gt;(TEvent message) where TEvent : Event
        {
            var typeOfEvent = typeof(TEvent);
 
            lock (listenerLock)
            {
                if (!listeners.ContainsKey(typeOfEvent)) return;
 
                foreach (var listener in listeners[typeOfEvent])
                {
                    var typedReference = (IListener&lt;TEvent&gt;)listener;
                    dispatcher.BeginInvoke(() =&gt; typedReference.Handle(message));
                }
            }
        }
 
        public virtual void Publish&lt;TEvent&gt;() where TEvent : Event, new()
        {
            Publish(new TEvent());
        }
    }
[/csharp]
</div>

In case you're wondering... the IDispatcher interface is merely a way to wrap Silverlight's real Dispatcher.  We wrap it so we can use a different implementation of the IDispatcher in our automated tests.  Other than that, the implementation is very straightforward.

We started using this very recently, so this implementation might change in the upcoming weeks, but for now it does what it needs to do and it does so pretty well.  In our case, every view's Presenter automatically has an IEventAggregator property so whenever we need to publish an event, we can simply do something like EventAggregator.Publish(new SomeEvent(someParameter)).  Or, Presenters that need to listen to events can simply say EventAggregator.Subscribe(this) or only subscribe to some specific events whenever they need to and their specific Handle method will be called whenever someone publishes the event.  This also allowed us to get rid of the somewhat awkward syntax when testing events (subscription, unsubscription and the actual handing of events) with mocking frameworks.

And as a last bonus, i put a call to the Unsubscribe(IListener) method in the Dispose method of our base Presenter implementation.  Which means that none of them will be left subscribed to events by accident anymore :)