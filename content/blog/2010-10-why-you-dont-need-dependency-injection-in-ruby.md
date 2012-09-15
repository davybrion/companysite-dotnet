If you're new to Ruby and you're coming from static languages like C# or Java, you'll probably wonder why there isn't much interest in Dependency Injection in the Ruby community.  The answer is quite simple: because you don't need it.  Now, that's not to say that Dependency Injection isn't a valuable technique in your toolbox.  In fact, if you're doing C# or Java i'd even go as far as saying it's absolutely necessary to use Dependency Injection in most of your code.  Two of the biggest reasons (i know there are more, but let's focus on these for now) why Dependency Injection is important if you're using a static language are these:
<ul>
	<li>Highly increased testability because you can control the dependencies during automated tests</li>
	<li>Lowered coupling between classes which enables you to change implementations of dependencies at will (granted, not a lot of people actually do that often but it certainly is <a href="http://davybrion.com/blog/2009/12/real-world-benefits-from-loose-coupling-inversion-of-control-and-dependency-injection/">a real benefit</a>)</li>
</ul>

In Ruby however, you don't really need dependency injection to achieve the 2 benefits mentioned above as i hope the following contrived example shows.  Suppose we have the following 2 classes.

<script src="https://gist.github.com/3728390.js?file=s1.rb"></script>

This is no good. The work_your_magic_on method from SomeClass directly instantiates a new instance of the Dependency class.  During automated tests, we could actually replace the implementation of the new method of the Dependency class to return a stub or a mock if we want to instead of an instance of the real thing.  But we could never easily change the implementation of the dependency that SomeClass requires to function properly in real production code without screwing up everything else that also happens to depend on the Dependency class.

If you're coming from a static language, you'd probably be inclined to change SomeClass to this:

<script src="https://gist.github.com/3728390.js?file=s2.rb"></script>

Ahh, that's much better.  The dependency is now injected in SomeClass' initializer method and we can very easily achieve the above mentioned 2 benefits by passing whatever we want to each instance of SomeClass, as long as it has a do_something_with method.  The biggest downside however is that every consumer of SomeClass instances now needs to know about the dependencies that it requires to function properly.  This quickly becomes very painful because using Dependency Injection throughout your codebase will very quickly lead to having to satisfy the dependencies of the dependencies of the dependencies of the dependencies of the class you actually need to use.  This quickly requires the usage of a good Inversion Of Control Container to handle all of these dependencies for you.  There's just one problem: there doesn't seem to be a widely used IOC Container in Ruby.  Which in itself tells you that it's simply not needed in Ruby.

There's a better way to modify SomeClass:

<script src="https://gist.github.com/3728390.js?file=s3.rb"></script>

Now, the ALT.NET fanbois will still tell you how wrong this is because SomeClass still has a direct dependency on the Dependency class.  I should know because i was one of them.  And again, in C# or Java i'd definitely agree that this code is bad.  Not so in Ruby however because i can easily replace the actual implementation of SomeClass' dependency in both automated tests and actual production code, without impacting anything else that uses the Dependency class.

Suppose we have the following test:

<script src="https://gist.github.com/3728390.js?file=s4.rb"></script>

Since i don't provide the dependency to the object that i'm testing, i can't really verify that the dependency was used correctly. I'm also not using any mocking framework since i want to show how the language itself takes away the need to inject your dependencies.  Given the following spy class:

<script src="https://gist.github.com/3728390.js?file=s5.rb"></script>

I could now write my test like this:

<script src="https://gist.github.com/3728390.js?file=s6.rb"></script>

And it'll pass.  The 'trick' is that i simply change the implementation of the get_dependency method for <em>the instance that i have</em>.  This doesn't change anything at the class level, merely at the instance level.  Technically, i don't really change the implementation of get_dependency in SomeClass, i merely insert a method in this particular instance which will precede the one in SomeClass during Ruby's method lookup procedure.

Also, i would like to point out that this kind of testing isn't exactly something that i'd encourage you to do, though this technique can be useful if you really need to do interaction testing.  But it's a good illustration of why you don't really need to do Dependency Injection in Ruby to write testable code.

Now you might be wondering, what's the difference between doing something like this and using a tool like TypeMock in .NET to basically achieve the same thing?  Well, when writing code like this in C# and testing it with TypeMock, you achieve one of the benefits that you could have with using Dependency Injection: being able to control the dependencies.  But you can't change the implementation of the dependency at runtime in normal production code.  In Ruby, with the approach outlined above, i can still easily achieve that like this:

<script src="https://gist.github.com/3728390.js?file=s7.rb"></script>

If this code is executed after the earlier definition of SomeClass, it will reopen SomeClass and change the implementation of the get_dependency method for each instance that will be created.  This effectively gives you the ability to change the implementation of a dependency at runtime in production code, without having to use Dependency Injection.  Now, some Dependency Injection purists will still claim that this approach is bad because SomeClass knows which implementation of the dependency it uses.  And my question to those people is: so what? I can easily change it in any situation i'd run into.  You can also consider the presence of the actual type of the dependency as the default implementation to use, without having to force the requirement of this knowledge on consumers. 

There is still one situation where i would probably use Dependency Injection in Ruby though, and that is when you want to benefit from what i consider to be yet another great reason to use Dependency Injection in static languages:
<ul>
	<li>Not having to know anything about the lifecycle of your dependencies</li>
</ul>

In this case, it's probably much easier to just inject a long-living dependency in an object with a shorter lifecycle.  However, if the dependency is basically a singleton (which is still the default for many of the .NET IOC Containers), then i actually would consider implementing the singleton as a class with nothing but class methods (similar to static methods in static languages, but not quite since you can still change them whenever you want) and having my other classes that depend on it call those methods directly, or through helper methods that i can still change when i need to.  

I'm sure many people will disagree with some of the points i try to make in this post, but until i get some actual real-world reasons that invalidate my points, i simply don't see the point in sticking to a set of rules and guidelines that were largely made up out of necessity to deal with shortcomings of static languages.  That's not to say that dynamic languages don't have any shortcomings or drawbacks, but it does mean that the rules and guidelines of how to write good code are, well, simply different.  And as such, it wouldn't be wise to blindly stick with rules that were made for a different way of programming.  Question what you already know, because it might not be relevant to what you need to do <em>now</em>.