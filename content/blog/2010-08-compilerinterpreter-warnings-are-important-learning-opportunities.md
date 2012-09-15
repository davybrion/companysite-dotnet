When i first started to write C# code, i payed a lot of attention to compiler warnings.  I wanted to avoid them at all costs.  And with that i don't mean suppressing them, but preventing them from being issued in the first place.  I learned quite a few things from actually trying to understand why a certain compiler warning was issued, instead of just ignoring it like so many other developers do.  In fact, when it comes to C#, i'd recommend turning on the Treat Warnings As Errors option on each project since i've <em>never</em> come across a C# compiler warning that you couldn't avoid.  And in practically every single case, avoiding the warning led to <em>better code</em>.  When writing C#, i've never seen a single warning that was pointless.  There might be a few esoteric ones that aren't worth fixing, but the vast majority of us will never run into those.  So do yourself a favor: if you get a compiler warning, make sure you understand why the warning was issued, and fix your code based on what you just learned while researching the warning.  There simply is no reason not to do so, unless you happen to bump into the few cases where it really doesn't matter but those cases will be far and between.  In fact, i'd bet that only people like Ayende will run into them while us mortals never will.    

So, how do i feel about warnings in the context of my ongoing Ruby journey? I have so little experience with Ruby that i can't state that every single warning should be avoided.  But i am of the opinion that you should at least be aware of every warning, and investigate whether or not you should modify your code to avoid it.  Today i wrote my first Rakefile which automatically runs all of my RSpec tests for my EventPublisher module and one of the options of the SpecTask was to enable warnings from the Ruby interpreter.  I was actually surprised that i hadn't yet ran my Ruby code with warnings turned on.  Maybe i was just too busy being impressed with the whole Ruby + TextMate package.  Anyways, i turned on the warnings, ran 'rake' and watched a few warnings show up, much to my disappointment.  Well, i was disappointed at first because i thought the code was in good shape but then i figured "ok, this is no big deal... i just gotta fix my code and i'll learn from it".  And i did learn from it.  The first one was a simple RSpec assertion that i wrote which looked like this:

<script src="https://gist.github.com/3727849.js?file=s1.rb"></script>

This generated the following warning:
warning: useless use of == in void context

I looked into it, and learned that when using RSpec, the last line should've been written like this:

<script src="https://gist.github.com/3727849.js?file=s2.rb"></script>

You're probably thinking "now that's a tiny difference and not really worth it".  And you'd be wrong.  While the resulting modification in code is indeed minor, i learned about RSpec's Matchers and how they work.  And that knowledge is gonna help me in future code.

I also had the following piece of a code:

<script src="https://gist.github.com/3727849.js?file=s3.rb"></script>

When i wrote it and saw that it worked, i was pretty happy.  But it turns out that this code causes the interpreter to issue the following warning:

warning: instance variable @first_event not initialized

In this case, @first_event was the value of the 'variable' variable, which is why the warning looks like that when the code is actually executed.   So again, i looked into it, and learned that i should've written that code like this:

<script src="https://gist.github.com/3727849.js?file=s4.rb"></script>

Moral of the story? Do not ignore compiler/interpreter warnings.  They are there to help you improve not only your current code, but also your future code.  That is, if you're willing to pay attention to them.

Note: if you have experience with a variety of C/C++ compilers from back in the day, i can imagine that your opinion differs greatly from mine.  However, i'm not talking about C/C++, so please keep the context (not to mention the decade...) in mind before you start typing a reply about how warnings in C/C++ could easily be justified depending on the compiler you were using ;)