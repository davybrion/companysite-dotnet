I'm still learning Ruby, and am almost through my second book about it.  But i finally caved in to the urge to just start playing around with it instead of reading about it first.  One of the things i noticed so far about the language, is that it doesn't have something like C#'s events.  At least not out of the box.  I thought it would be fun to write something that allows me to define and use events in Ruby in a way that is very similar to how it works in C#.  It actually is pretty easy to do this and i think it shows some of the power and flexibility of the Ruby language. 

For those of you who already know and use Ruby: i know that this is most likely not the best way to do this, and that all of this is probably already available.  But keep in mind that this is my 'hello world' in Ruby and that i'm just playing around with it.

First of all, we're going to need an Event class:

<script src="https://gist.github.com/3728156.js?file=s1.rb"></script>

This code is really simple.  An Event instance has a name, and an array of handlers.  A handler is just a reference to a Method that we can execute whenever we want.  The + method allows you to add a handler to the event, and it simply returns self, so we can sort of mimic the C# event subscription code.  Our Ruby variant basically looks like this:

<script src="https://gist.github.com/3728156.js?file=s2.rb"></script>

The - method basically uses the same trick, so unsubscribing from an event looks like this:

<script src="https://gist.github.com/3728156.js?file=s3.rb"></script>

With that in place, we need a way to define an event in a class, and to trigger it.  Preferably, this has to look as natural as possible and with that i mean that it should look like it's just supported by language keywords.  We naturally can't add language keywords, but we can fake it sort of by adding methods which you can call without parentheses so at least it'll look like language keywords.  There are multiple ways to do this, but i've chosen the simplest one, which is to open the Object class and add a few private methods it.  Note that in Ruby, private methods can be used by derived classes so these methods are accessible by any class that inherits from it, but you'll never be able to call them on any instance but yourself.

<script src="https://gist.github.com/3728156.js?file=s4.rb"></script>

This piece of code probably deserves some more explanation :).  We basically add two methods to the Object class: define_event and trigger_event.  When define_event is called, we dynamically add 2 methods to the class: a getter and a setter for the newly created Event.  The only reason we need the setter is to enable the subscription syntax:

<script src="https://gist.github.com/3728156.js?file=s5.rb"></script>

Which is basically the same as doing this:

<script src="https://gist.github.com/3728156.js?file=s6.rb"></script>

The trigger_event method is very straightforward: it just retrieves the instance variable for the event, and calls its trigger method and passes the args variable.

And that's it... lets demonstrate this new 'language feature' with a simple example.  First, we have the Publisher class:

<script src="https://gist.github.com/3728156.js?file=s7.rb"></script>

It defines an event with the name 'notify' and it has a public method to trigger the event.  We also have a Subscriber class:

<script src="https://gist.github.com/3728156.js?file=s8.rb"></script>

As you can see, the Subscriber subscribes and unsubscribes from the 'notify' event in a manner that is very similar to how it's done in C#.  

Finally, the output of the following code:

<script src="https://gist.github.com/3728156.js?file=s9.rb"></script>

is this:

2148074920 Sun Aug 22 12:17:58 +0200 2010 received: hello world!</br>
2148074900 Sun Aug 22 12:17:58 +0200 2010 received: hello world!</br>
2148074900 Sun Aug 22 12:17:58 +0200 2010 received: hello world!</br>

As you can see, subscriber1 received the event once, while subscriber2 received it twice.

Again, this is certainly not the best way to do this but i just wanted to try this because i can.  And for the experienced Ruby devs among you, please go easy on me since this is just my first piece of Ruby code :)