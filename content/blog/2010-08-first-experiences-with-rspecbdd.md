I wanted to write some tests for the <a href="http://davybrion.com/blog/2010/08/using-more-rubyesq-events-in-ruby/">EventPublisher</a> Ruby module i've been playing around with, so i figured i'd just use <a href="http://rspec.info/">RSpec</a> for it since that appears to be the most popular testing library in the Ruby world.  Now, in the .NET world i never really got into the whole BDD thing and i stuck with TDD because i was quite happy with the coverage that it gave me.  In Ruby however, due to the whole dynamic environment i think it's more important to test functionality as completely as possible with as little knowledge as possible of implementation details while mocking/stubbing/faking as little as possible.  That doesn't mean i wouldn't mock anything in Ruby tests... it just means that i would try to follow <a href="http://davybrion.com/blog/2008/08/test-doubles-when-to-not-use-them/">my own rules</a> on the subject as much as possible, whereas in the .NET world many of us (myself included) probably go a little overboard with the whole mocking/stubbing/faking thing.

Something to keep in mind for the rest of this post: i did not write my tests first for this thing.  I know, i know, test-first is better.  I generally prefer to write my tests before my real code as well, but in this case, the EventPublisher code was the result of just some first time Ruby experiments, and since i'm pretty happy with the code i don't want to get rid of it just so i could do it "right" by re-writing it test-first.  So these tests were not meant to drive the design, only to verify the correctness of the code.  Also note that the tests are not complete yet.  More should be added, but i thought i had enough to post here and hopefully collect some feedback from you guys/gals.

When i started with these tests for the EventPublisher module, i instinctively wanted to test on a too technical level, like i often do in .NET.  For instance, i wrote a test that proved that when you called the subscribe method, that the passed in method was actually added to the Event instance that the EventPublisher uses.  The thing is: if you use the EventPublisher, you never directly use Event instances.  So why on earth should i even know about them in my tests, right? After all, they are an implementation detail.  I had to switch my reasoning from "is the code doing what i, a software developer, think it should do?" to something along the lines of "what needs to happen when i trigger an event?".  For instance, if i trigger an event, all i should care about is that the subscribed methods are called correctly and that they receive their arguments correctly.  How that actually happens is something that i probably shouldn't care about at all in these tests. 

I eventually ended up with the following:

<div>
[ruby]
class Publisher
	include EventPublisher
	event :my_first_event
	event :my_second_event
	
	def trigger_first_event(args)
		trigger :my_first_event, args
	end
	
	def trigger_second_event(arg1, arg2)
		trigger :my_second_event, arg1, arg2
	end
end

describe EventPublisher, &quot;: triggering event&quot; do
	before(:each) do
	  @publisher = Publisher.new
	end

	it &quot;should not fail without any subscribers&quot; do
	  @publisher.trigger_first_event &quot;testing&quot;
	end

	it &quot;should pass single event arg correctly to subscribed method with one argument&quot; do
		@args = nil
	  def my_first_event_handler(args);
			@args = args
		end

		@publisher.subscribe :my_first_event, method(:my_first_event_handler)
		@publisher.trigger_first_event &quot;testing!&quot;
		@args.should == &quot;testing!&quot;
	end
	
	it &quot;should pass multiple event args correctly to subscribed method with multiple arguments&quot; do
		@args2_1, @args2_2 = nil, nil
		def my_second_event_handler(arg1, arg2)
			@args2_1, @args2_2 = arg1, arg2
		end
		
		@publisher.subscribe :my_second_event, method(:my_second_event_handler)
		@publisher.trigger_second_event &quot;second&quot;, &quot;event&quot;
		@args2_1.should == &quot;second&quot;
		@args2_2.should == &quot;event&quot;
	end

	it &quot;should pass single event arg correctly to subscribed block with one argument&quot; do
		event_args = nil
		@publisher.subscribe(:my_first_event) { |args| event_args = args }
		@publisher.trigger_first_event &quot;test&quot;
	  event_args.should == &quot;test&quot;
	end
	
	it &quot;should pass multiple event args correctly to subscribed block with two arguments&quot; do
	  first_arg, second_arg = nil, nil
		@publisher.subscribe(:my_second_event) { |arg1,arg2| first_arg, second_arg = arg1, arg2 }
		@publisher.trigger_second_event &quot;first&quot;, &quot;second&quot;
		first_arg.should == &quot;first&quot;
		second_arg.should == &quot;second&quot;
	end
	
	it &quot;should call subscribed method once for each time it was subscribed&quot; do
	  @counter1 = 0
		def my_first_event_handler(args)
			@counter1 += 1
		end
		
		2.times { @publisher.subscribe :my_first_event, method(:my_first_event_handler) }
		@publisher.trigger_first_event &quot;test&quot;
		@counter1.should == 2
	end
	
	it &quot;should call all subscribed methods&quot; do
		@counter1, @counter2 = 0, 0
		def handler1(args)
			@counter1 += 1
		end
		
		def handler2(args)
			@counter2 += 1
		end
		
		@publisher.subscribe :my_first_event, method(:handler1)
		@publisher.subscribe :my_first_event, method(:handler2)
		@publisher.trigger_first_event &quot;first_event&quot;
		@counter1.should == 1
		@counter2.should == 1
	end
	
end
[/ruby]
</div>

There are a couple of things i like about this.  For starters, the output of running this code looks like this:

EventPublisher: triggering event
  should not fail without any subscribers
  should pass single event arg correctly to subscribed method with one argument
  should pass multiple event args correctly to subscribed method with multiple arguments
  should pass single event arg correctly to subscribed block with one argument
  should pass multiple event args correctly to subscribed block with two arguments
  should call subscribed method once for each time it was subscribed
  should call all subscribed methods

Anyone can read that and understand what kind of functionality is supported.  

Another big benefit of these tests is that they contain zero knowledge of the actual implementation of the EventPublisher module.  They merely initiate its functionality, and verify whether the expected behavior in the given functional context occurred.  I could seriously refactor (or even rewrite) the actual EventPublisher code and i wouldn't have to change my tests as long as i don't change the name and arguments of the subscribe and trigger methods.  

For now, i'm pretty happy with this style and organization of tests and will probably stick with it for a while in my Ruby coding.  Unless one (or some) of you tell me how i can improve it :)