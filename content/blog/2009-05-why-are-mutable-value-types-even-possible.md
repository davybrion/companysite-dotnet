A coworker of mine was recently trying to figure out why some code wouldn't compile.

Consider the following code:

<script src="https://gist.github.com/3684569.js?file=s1.cs"></script>

This compiles and works.  Now consider the following class:

<script src="https://gist.github.com/3684569.js?file=s2.cs"></script>

And then this small piece of code:

<script src="https://gist.github.com/3684569.js?file=s3.cs"></script>

You'd typically expect that this code would compile, right? Well it doesn't.  It fails with the following compiler error:

error CS1612: Cannot modify the return value of 'SomeApp.SomeClass.SomePoint' because it is not a variable

Not exactly what you would expect, right? Well, the compiler error in this case is actually a good thing, though the message isn't as clear as it should be.  See, System.Windows.Point is actually a value type instead of a reference type, and as you should know, value types behave very differently from reference types when passed around.  You basically get a copy every time instead of the actual instance.

Let me just borrow the explanation of the "DO NOT define mutable value types" guideline, from Microsoft's <a href="http://www.amazon.com/Framework-Design-Guidelines-Conventions-Development/dp/0321545613/ref=sr_1_1?ie=UTF8&s=books&qid=1241181546&sr=8-1">Framework Design Guidelines</a>:


> Mutable value types have several problems.  For example, when a property getter returns a value type, the caller receives a copy. Because the copy is created implicitly, developers might not be aware that they are mutating the copy, and not the original value.

Now the compiler error makes sense, right? Well, the message still sucks, but it makes sense that the compiler would protect you from making this mistake.

It won't protect you from the following mistake though:

<script src="https://gist.github.com/3684569.js?file=s4.cs"></script>

This code will compile, though if you expect that myObject.SomePoint.X will return 5, you're in for a treat.  Hopefully nobody writes code like this, but imagine having to debug something like this in some legacy code that was never properly tested.

But really, why are mutable value types even possible? In what situation does a mutable value type make sense? I'd really like to know, so please point out those situations in the comments if you know of one.  The only reason I can think of is performance but the number of situations where a mutable value type is going to give you a significant performance boost is probably so small that it would've made more sense to enable mutability for value types with a language keyword or something like that.  Unless there is another benefit that I'm not aware of yet?