I wanted to see how easy or difficult it would be to use your own Ruby classes from C# through IronRuby.  Turns out it's pretty easy to do so.

Suppose we have the following simple Ruby classes:

<script src="https://gist.github.com/3728341.js?file=s1.rb"></script>

Each of those classes is located in its own file, but i've listed all of the code together here.  Now, how hard or easy would it be to say, create an Order instance in C#?

It turns out to be pretty easy.  It's pretty easy to start up a Ruby engine in .NET and have it execute some Ruby files.  Now, i don't want to tell it to execute each file, so i create a bootstrap.rb file which contains the following code:

<script src="https://gist.github.com/3728341.js?file=s2.rb"></script>

Obviously, this just loads each entity's file into the current scope.

Now, i can just do this in C#:

<script src="https://gist.github.com/3728341.js?file=s3.cs"></script>

And the output of that is this:

order total: 550 </br>

<code>
#&lt;Order:0x000005c @customer=#&lt;Customer:0x0000056 @name='Davy Brion', @email='davy@gmail.com'&gt;, @discount=nil, @date=9/5/2010 22:36:14, @items=[#&lt;OrderItem:0x000005e @product=#&lt;Product:0x0000058 @name='product1', @price=50&gt;, @count=5&gt;, #&lt;OrderItem:0x0000060 @product=#&lt;Product:0x000005a @name='product2', @price=60&gt;, @count=5&gt;]&gt; 
</code>

Not sure what you think of that, but i thought it was pretty impressive.  I'd hoped that this would be possible, but i wasn't sure since most of the examples you see about IronRuby seem to be focused on using .NET types from Ruby code that is interpreted by IronRuby's interpreter.  But given the flexibility that you have in Ruby when it comes to designing classes, i'm much more interested in using Ruby classes from C# code instead of the other way around.

Let's go over some parts of the code...

<script src="https://gist.github.com/3728341.js?file=s4.cs"></script>

This is all you need to do to start the Ruby engine (no idea why they call it that but whatever) and execute our bootstrap.rb file so that our classes are defined.  We then have access to IronRuby's top-level binding through the engine.Runtime.Globals property.

So now we can simply create instances of these classes like this:

<script src="https://gist.github.com/3728341.js?file=s5.cs"></script>

It's too bad that we have to escape the 'new' method because the C# compiler should be capable of figuring out that we aren't using the new operator there.  But other than that, i'm pretty happy with how this works.

<script src="https://gist.github.com/3728341.js?file=s6.cs"></script>

As you can see, calling methods on the instances of our Ruby classes looks normal as well, except for the fact that those classes use the typical Ruby naming conventions instead of offering a typical .NET-looking AddItem method.  We'll fix that later though ;)

<script src="https://gist.github.com/3728341.js?file=s7.cs"></script>

Now this is actually cooler than it might appear on first sight. First of all, notice the lack of parentheses when we call order.items, item.count and item.product.price.  Big deal, you use properties all the time right? Well, Ruby doesn't have properties... it just has methods and due to some of its rules you can write this in Ruby:

<script src="https://gist.github.com/3728341.js?file=s8.cs"></script>

Which is actually only syntactical sugar for what it really is:

<script src="https://gist.github.com/3728341.js?file=s9.cs"></script>

The fact that using these methods as if they are properties in C# is a nice touch, though it only works for accessor methods which were defined with the attr_reader, attr_accessor and attr_writer methods in your Ruby classes.  If you defined your own accessor methods, you will have to use parentheses when you call them in C#.

The other thing that i find pretty cool about that piece of code is that you can use the foreach statement to loop through the return value of the order.items method, which is a <em>Ruby array</em>.  Not sure whether IronRuby implicitly wraps Ruby arrays as IEnumerables or if it does that with all Ruby types which mix in the Enumerable module, but whatever it is, it's cool.  

All in all, it's pretty nice that we can easily create and use these instances of classes that we defined in Ruby.  But i'm a big fan of sticking to the accepted naming guidelines for each language.  In Ruby, each method is lowercased and optionally uses underscores instead of the capitalized pascal cased method names that we typically use in C#.  And when you use CLR types in Ruby code that is running in IronRuby, you can indeed stick to Ruby's naming conventions and IronRuby will automatically 'translate' method calls like write_line to WriteLine.  It would be cool if, for instance, we could do order.AddItem in the example above and that it would just be 'translated' to order.add_item.  And of course, if we could use capitalized versions of the accessor methods then it would look like pretty typical .NET code apart from the call to the @new method.

With Ruby code, pretty much everything is possible so it shouldn't be a surprise that we can easily 'fix' the naming convention issue. Keep in mind though that the approach i'm going to show is quite crude, and there most likely is a better way that i haven't thought of yet.  I just added the following code at the bottom of the bootstrap.rb file:

<script src="https://gist.github.com/3728350.js?file=s1.rb"></script>

Since we only need to do this if we're running in IronRuby, we first check whether the IronRuby constant is defined.  If it isn't there's no point in adding the .NET-friendly method aliases.  If we are running in IronRuby, we just add a method alias for each public instance method of our 4 classes.  Again, it's a crude solution, but it does enable us to write the following C# code:

<script src="https://gist.github.com/3728350.js?file=s2.cs"></script>

And it works just like you'd expect it to.  So far, i'm pretty happy, and very impressed with IronRuby :) 