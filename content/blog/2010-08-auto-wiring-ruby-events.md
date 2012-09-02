My <a href="http://davybrion.com/blog/2010/08/using-more-rubyesq-events-in-ruby/">Rubyesq events</a> received some nice comments in the <a href="http://5by5.tv/rubyshow/130">Ruby Show #130</a> (fast forward to the 15 minute mark in the audio stream, or the 14 minute mark in the video stream if you wanna see or hear the part about the events), so i figured: why not make them even better?

Alex Simkin had suggested to implement some kind of auto-wiring of events.  I thought it would be fun to implement, so i did.  Suppose we have the following class:

<div>
[ruby]
class Publisher
	include EventPublisher
	event :first_event
	event :second_event
	
	def trigger_events
		trigger :first_event, &quot;first event&quot;
		trigger :second_event, &quot;second event&quot;
	end
end
[/ruby] 
</div>

If we want to subscribe to both events, we'd need to write code like this:

<div>
[ruby]
		@publisher.subscribe :first_event, method(:first_event_handler)
		@publisher.subscribe :second_event, method(:second_event_handler)
[/ruby]
</div>

But it obviously would be much nicer if we could just do something like this:

<div>
[ruby]
		@publisher.subscribe_all self
[/ruby]
</div>

The subscribe_all method could then just look at all the methods that the passed-in instance contains, and it could automatically subscribe each suitable handler (based on a simple convention) to the correct event.

That means we could just have a Subscriber class like this:

<div>
[ruby]
class Subscriber
	def initialize(publisher)
		@publisher = publisher
		@publisher.subscribe_all self
	end
	
	def stop_listening
		@publisher.unsubscribe_all self
	end
	
	def first_event_handler(args)
		puts args
	end
	
	def second_event_handler(args)
		puts args
	end
end
[/ruby]
</div>

This was pretty easy to do actually.  Our Event class remains the same, but our EventPublisher module does get a few new methods:

<div>
[ruby]
	def each_suitable_handler(subscriber)
		possible_handlers = subscriber.class.instance_methods.select { |name| name =~ /\w_handler/ }
		possible_handlers.each do |method_name|
			event_name = /(?&lt;event_name&gt;.*)_handler/.match(method_name)[:event_name]	
			if EVENTS.include? event_name.to_sym
				yield event_name.to_sym, method_name.to_sym
			end
		end
	end

	def subscribe_all(subscriber)
		each_suitable_handler(subscriber) do |event_symbol, method_symbol|
			subscribe event_symbol, subscriber.method(method_symbol)
		end
	end
	
	def unsubscribe_all(subscriber)
		each_suitable_handler(subscriber) do |event_symbol, method_symbol|
			unsubscribe event_symbol, subscriber.method(method_symbol)
		end
	end
[/ruby]
</div>

And the method to define an event was slightly modified, so it now looks like this:

<div>
[ruby]
	self.class.class_eval do
		EVENTS = []

		def event(symbol)
			getter = symbol
			variable = :&quot;@#{symbol}&quot;
			EVENTS &lt;&lt; symbol

			define_method getter do
				event = instance_variable_get variable

				if event == nil
					event = Event.new(symbol.to_s)
					instance_variable_set variable, event
				end

				event
			end
		end
	end
[/ruby]
</div>

The only difference here is that we define an EVENTS array constant, and every time an event is defined, we add the symbol of the event to the EVENTS array.  I'm not very happy with using an array constant to do this, but it was the only way i found to store the symbol name of each event when it's defined while also being able to access those symbols from within the each_suitable_handler method.  Again, i'm new at Ruby so i'm probably missing an easier alternative here.

But with these changes in place, we can now run the following code:

<div>
[ruby]
publisher = Publisher.new
subscriber = Subscriber.new(publisher)
publisher.trigger_events
subscriber.stop_listening
publisher.trigger_events
[/ruby]
</div>

Which produces the expected output:

first event
second event

For the 5 people who are going to want to use this: i'm going to create a gem for this soon, and the code will be hosted on GitHub.  I just need to learn how to create a gem and learn how Git works first though :)