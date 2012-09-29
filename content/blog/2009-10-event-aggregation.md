We're experimenting at work with a bit of a different approach as to how we structure our views and how they will interact with each other.  We already know that our views will be reused in many different contexts so having them communicate with other views is something that needs to be done in a very loosely coupled manner.  I don't want any of the views to even know about the existence of other views, let alone having them know about specific instances of them.

But these views do have to interact with each other.  I didn't want to use typical events because that would require either a certain view to know about another view to be able to subscribe to its events, or you'd need some other component which knows which views need to be hooked to each other.  We really need maximum flexibility for what we have in mind with our views, so it only made sense to finally start using the <a href="http://martinfowler.com/eaaDev/EventAggregator.html">Event Aggregation pattern</a>.  The idea is that a view can basically publish events, without knowing who is subscribed to these events, and that suscribers will be notified whenever these events occur.  However, subscribers don't know anything about who is publishing the events.  Instead, both publishers and subscribers only know of the Event Aggregator.  A publisher tells the aggregator to publish an event to all subscribed listeners for that event.  Each subscribers simply tells the aggregator "i'd like to be notified whenever this event occurs, and I don't care where it comes from".  
Plenty of implementations of this pattern can be found online already, so I figured: why not add my own? :p

Fist of all, an event is nothing more than a class that inherits from this class:

<script src="https://gist.github.com/3685223.js?file=s1.cs"></script>

Every event should inherit from this class, and add whatever necessary properties that are important for that particular type of event. 

If a class is interested in listening to a specific event, it needs to implement the following interface:

<script src="https://gist.github.com/3685223.js?file=s2.cs"></script>

If a class is interested in multiple events, it simply needs to implement the generic IListener interface for each type of event that it wants to handle.

Then we obviously need the Event Aggregator.  I wanted an aggregator that allowed listeners to either subscribe/unsubscribe to/from very specific events, or just subscribe/unsubscribe to/from whatever it supports.  So I have the following IEventAggregator interface:

<script src="https://gist.github.com/3685223.js?file=s3.cs"></script>

The Subscribe and Unsubscribe methods that simply take an IListener reference will either subscribe or unsubscribe the given listener to/from every event that it can handle.  In other words, for every generic IListener interface that it implements.  Yet you also have the ability to subscribe/unsubscribe from a specific event type.

And here's the implementation:

<script src="https://gist.github.com/3685223.js?file=s4.cs"></script>

In case you're wondering... the IDispatcher interface is merely a way to wrap Silverlight's real Dispatcher.  We wrap it so we can use a different implementation of the IDispatcher in our automated tests.  Other than that, the implementation is very straightforward.

We started using this very recently, so this implementation might change in the upcoming weeks, but for now it does what it needs to do and it does so pretty well.  In our case, every view's Presenter automatically has an IEventAggregator property so whenever we need to publish an event, we can simply do something like EventAggregator.Publish(new SomeEvent(someParameter)).  Or, Presenters that need to listen to events can simply say EventAggregator.Subscribe(this) or only subscribe to some specific events whenever they need to and their specific Handle method will be called whenever someone publishes the event.  This also allowed us to get rid of the somewhat awkward syntax when testing events (subscription, unsubscription and the actual handing of events) with mocking frameworks.

And as a last bonus, I put a call to the Unsubscribe(IListener) method in the Dispose method of our base Presenter implementation.  Which means that none of them will be left subscribed to events by accident anymore :)