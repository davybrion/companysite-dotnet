A mistake that i used to see a lot, and sometimes still do, is that developers hide inherited members in derived types. For those of you who don’t know what that means, check out the following class:

<script src="https://gist.github.com/3692965.js?file=s1.cs"></script>

As you can see, the DoSomething method does something important, so you as a developer should consider its behavior important as well. In some cases, you might want to add something extra to this behavior in a derived class. Some developers would do that like this:

<script src="https://gist.github.com/3692965.js?file=s2.cs"></script>

When they compile this, they’ll get the following warning:

warning CS0108: 'HidingMethods.MyDerivedClass.DoSomething()' hides inherited member 'HidingMethods.MyClass.DoSomething()'. 
Use the new keyword if hiding was intended.

When this occurs, either one of 4 things can happen:

- The developer doesn’t bother to read compiler warnings (a severe offense IMO) and is not aware of a possible problem
- The developer sees the warning and just adds the ‘new’ keyword to the method.
- The developer reads the documentation to figure what this really means, and hopefully realizes his mistake. If he does, he combines this option with option 4. If he doesn’t, he goes the option 2 route.
- The developer realizes his mistake and either makes the base method virtual and adds the override keyword to the method in the derived class, or when that’s not possible, either renames the method or thinks of a different approach.

If you’re unlucky, you either end up with no modification or the method will now look like this:

<script src="https://gist.github.com/3692965.js?file=s3.cs"></script>

Great, no more compiler warning! All is well in the world now, right? Err… not really. The DoSomething method of MyDerivedClass actually <em>hides</em> the original DoSomething method. The following code will always produce the expected behavior:

<script src="https://gist.github.com/3692965.js?file=s4.cs"></script>

That is, when the DoSomething method of an instance of MyClass is called, it will obviously execute the original DoSomething method. And if the DoSomething method of an instance of MyDerivedClass class is called <em>through a reference of MyDerivedClass</em> then it will call the ‘new’ DoSomething method. The following code however, would not produce the expected behavior:

<script src="https://gist.github.com/3692965.js?file=s5.cs"></script>

In this example, the DoSomething method is always called through a reference of MyClass. When the DoIt method receives an instance of MyClass, the original DoSomething method will obviously be executed. What some (many?) people unfortunately aren’t aware of is that when an instance of MyDerivedClass is passed into the DoIt method, the ‘new’ DoSomething method will not be executed but only the original one will be executed. The reason is because methods that hide inherited members can only be called through references of the type, or derived types of that, that hides the inherited method. Doesn’t really sound like a fun situation to debug, right?

So anyways, back to my original question: is there any valid reason why you would want to do this? Or better yet, can you share a situation where you had to resort to hiding an inherited member and if so, are you happy with the solution or did you consider it a hack? And are you aware that it is essentially a bug waiting to happen? So far, i have never actually <em>seen</em> a valid reason for doing this. The only reason i’ve seen so far for occurrences of hidden members was because the developers that used it simply didn’t know any better. In some cases, either me or someone else wasted debugging time on this when a piece of code that used a reference of a base class suddenly didn’t do what was expected when it was passed an instance of a derived class which hid the original method.

I have <em>read</em> of only one valid reason to do this, and that is if a (either virtual or non-virtual) method with the same signature is introduced outside of your control in a base class that you can’t modify (like say, a class in the .NET framework or some other 3rd party assembly that you depend on) and you want to get rid of the compiler error that you got when recompiling against the newer version of the assembly. In that case, it does make sense though i’d still say it’s going to be a future source of confusion sooner or later, and quite likely lead to a future bug as well. In that situation, i’d much rather rename the member in my class. Even if it means that consumers of my code will be forced to deal with the breaking change. After all, a simple rename will always be less work than having to debug an issue because of a hidden member.