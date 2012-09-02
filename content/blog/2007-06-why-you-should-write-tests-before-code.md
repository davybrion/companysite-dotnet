It's often hard to convince developers to write tests before writing the actual code. At least one of my readers (Hi Stefan! ;)) knows all too well how hard it was to convince me. I didn't really believe it would be that beneficial to our projects. I could not have been more wrong. These days, i'm very much convinced that writing tests before writing code offers <b>many</b> advantages. Not only does it improve the code, it increases the chance of a successful project and can make developers better than they already are.  Nevertheless, there are still lots of highly skilled developers that don't do it.

So this is my list of reasons (in no specific order) why you should be writing your tests first:

<ul>

 <li>It leads to <strong>developer-friendly API's</strong>. When you write the test, you're already the first user of your class.  You'll automatically define an intuitive API simply because you're forced to think about it before you do anything else.  This is one of the most underrated benefits IMHO.
 </li>
<br />
 <li>If you do it right, there will be <strong>no unnecessary code</strong>. We've all been there... you're implementing a feature and suddenly you think of something else you also need to write. A lot of times, that something else you write turns out to be unnecessary. And code that shouldn't be there is a shame. The best quote i ever read about code was something like "code is not finished when there's nothing left to add, it's finished when there's nothing left to remove". I don't remember who originated it, but it's oh so true.  This unnecessary code is just a waste of time. That code wasted time while it was being written, and it will continue to waste time everytime it is read. Think about that. How often have you spent time trying to figure out code and you weren't really sure if it was actually being used? If you write your tests, and then only write the code to make the tests pass you should be able to avoid unnecessary code. It's not always easy and it's a matter of discipline but it's really a goal you should be striving for.
 </li>
<br />
 <li>It leads to <strong>better code</strong>. Code that is easily testable is usually flexible code. Flexible code is easier to change and improve. And the tests back you up while improving the code to make sure you didn't introduce errors.
 </li>
<br />
 <li>It <strong>increases the skills of your developers</strong>. In more than one way actually. When developers write code, they make mistakes. Every last one of them. The sooner you learn about the mistake, the higher the odds that you'll avoid the mistake next time around. Some bugs aren't discovered until the system is already in production. Often, the developer that made the mistake is not around anymore. How is he gonna learn from the mistake then? He won't. If he has a test that will inform him of his mistake immediately after writing the code, he will have learned something. Just to be clear: i'm not saying that writing tests first will eliminate all bugs. It won't. But it should severely reduce them.  Another area where the developers will improve is design. The more tests they write, the more they will learn about creating code that is easy to test. And if it's easy to test, it's easy to change thus indicating good design.
 </li>
<br />
 <li>A code base that is the result of test-first development is a code base that can be modified in a safe way. It allows developers to <strong>confidently make changes</strong> to code. We've all been in the situation where you need to fix a bug, but you're affraid of changing the code because it's very likely that you'll introduce new bugs. If you have an extensive suite of tests, you should immediately know if you've introduced new bugs.  This allows you to confidently make the change you think is the best one.  Make the change, run the tests and if everything remains green, perfect! If it turns red, great! You'll know it was the wrong fix before your customers, your manager and your boss knows about it.
 </li>
<br />
 <li>Writing tests first allow you to <strong>significantly reduce defects</strong>. You'll never write bug-free software. It's very important that you realize that. But you should at least try to minimize the amount of defects in the software you develop. If you write your tests first, you'll not only get instant feedback on newly written code, but you'll also be better protected against regressions in the existing code.
 </li>
<br />
</ul>

This list is far from complete... these are just the benefits that i can think of at the top of my head.

I'm probably preaching to the converted already, but at least now I can simply direct the uncoverted to this post instead of repeating the same thing over and over again :)