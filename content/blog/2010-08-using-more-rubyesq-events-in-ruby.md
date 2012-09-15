In my <a href="http://davybrion.com/blog/2010/08/using-c-style-events-in-ruby/">previous post</a> i showed a way to use events in Ruby in a way that is very similar to how it is in C#.  Somebody left a comment saying that i should take off my C# hat, and do it in a more typical Ruby way.  I agree with that so i wanted to create an implementation based on his <a href="http://davybrion.com/blog/2010/08/using-c-style-events-in-ruby/#comment-55315">comment</a>.  I also wanted to avoid opening up the Object class and requiring you to mix-in a module in order to use the events.

The goal is basically to declare an event like this:

<script src="https://gist.github.com/3728187.js?file=s1.rb"></script>

And subscribing to the event would be done like this:

<script src="https://gist.github.com/3728187.js?file=s2.rb"></script>

or

<script src="https://gist.github.com/3728187.js?file=s3.rb"></script>

and if you subscribed with a method, you should be able to unsubscribe like this:

<script src="https://gist.github.com/3728187.js?file=s4.rb"></script>

This was again pretty simple to implement, though this implementation is not a robust as it could be (so keep that in mind if you ever decide to use this approach).  First of all, we again start off with the Event class, which now looks like this:

<script src="https://gist.github.com/3728187.js?file=s5.rb"></script>

Nothing special here, except that the add method can accept a Method instance, a block, or both.  The default value of the method parameter is nil so you can skip it if you only want to hook a block to the event.

And then we have the EventPublisher module that you can mix-in (more on this in a bit) to your class:

<script src="https://gist.github.com/3728187.js?file=s6.rb"></script>

You might be wondering what the following line does: </br>

event = send(symbol) </br>

This dynamically calls the method with the given symbol.  In our case, that would be the getter method to access the event, which we only create during the registration of an event, so we can't call this method like we'd normally do.

Also note that there is no way to unsubscribe a block from the event.  Well, there might be a way but i simply don't know how to do it, since a block is not an object and it has no identity.  AFAIK (and again, i'm a ruby n00b) there is no good way to compare blocks, so we can't unsubscribe them from events either.  So you really only want to subscribe blocks to an event if you're sure that the event will not be published at a time when you don't want the block to be executed.  Also, keep in mind that any variables that the block closes on will be kept in memory, so if your block closes on object references, their instances will be kept in memory until the publisher of the event is garbage collected.

If you need to be sure that you can remove the behavior you've added to an event, subscribe with a Method instance and unsubscribe when you need the added behavior removed again!

Now we can define our Publisher from the previous post like this:

<script src="https://gist.github.com/3728187.js?file=s7.rb"></script>

The EventPublisher module is used as a <a href="http://en.wikipedia.org/wiki/Mixin">mixin</a> in the Publisher class.  Simply put, that means that the methods defined in EventPublisher are now a part of the Publisher class as well (including their definition), and the nice thing about it is that we didn't have to inherit from a base class to inherit this extra functionality.

We also have the following two classes:

<script src="https://gist.github.com/3728187.js?file=s8.rb"></script>

The first class subscribes to the event through a Method instance, the second simply assigns a block.  As you can see, the first class can unsubscribe from the event by passing the Method instance to the unsubscribe method for that given event. 

And if we run the following code:

<script src="https://gist.github.com/3728187.js?file=s9.rb"></script>

We get the following output:

SubscriberWithMethod received: hello world! </br>
block received: hello world!</br>
block received: hello world!</br>