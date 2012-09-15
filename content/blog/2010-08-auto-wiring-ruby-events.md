My <a href="/blog/2010/08/using-more-rubyesq-events-in-ruby/">Rubyesq events</a> received some nice comments in the <a href="http://5by5.tv/rubyshow/130">Ruby Show #130</a> (fast forward to the 15 minute mark in the audio stream, or the 14 minute mark in the video stream if you wanna see or hear the part about the events), so i figured: why not make them even better?

Alex Simkin had suggested to implement some kind of auto-wiring of events.  I thought it would be fun to implement, so i did.  Suppose we have the following class:

<script src="https://gist.github.com/3727807.js?file=s1.rb"></script>

If we want to subscribe to both events, we'd need to write code like this:

<script src="https://gist.github.com/3727807.js?file=s2.rb"></script>

But it obviously would be much nicer if we could just do something like this:

<script src="https://gist.github.com/3727807.js?file=s3.rb"></script>

The subscribe_all method could then just look at all the methods that the passed-in instance contains, and it could automatically subscribe each suitable handler (based on a simple convention) to the correct event.

That means we could just have a Subscriber class like this:

<script src="https://gist.github.com/3727807.js?file=s4.rb"></script>

This was pretty easy to do actually.  Our Event class remains the same, but our EventPublisher module does get a few new methods:

<script src="https://gist.github.com/3727807.js?file=s5.rb"></script>

And the method to define an event was slightly modified, so it now looks like this:

<script src="https://gist.github.com/3727807.js?file=s6.rb"></script>

The only difference here is that we define an EVENTS array constant, and every time an event is defined, we add the symbol of the event to the EVENTS array.  I'm not very happy with using an array constant to do this, but it was the only way i found to store the symbol name of each event when it's defined while also being able to access those symbols from within the each_suitable_handler method.  Again, i'm new at Ruby so i'm probably missing an easier alternative here.

But with these changes in place, we can now run the following code:

<script src="https://gist.github.com/3727807.js?file=s7.rb"></script>

Which produces the expected output:

first event
second event

The code can be found on [GitHub](https://github.com/davybrion/EventPublisher).
