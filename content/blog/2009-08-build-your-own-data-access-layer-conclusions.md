Note: This post is part of a series.  Be sure to read the introduction <a href="/blog/2009/08/build-your-own-data-access-layer-series/">here</a>.

Building your own DAL is almost never a cost-efficient solution.  In this case, I wrote this DAL in 24 working hours, but it is limited in scope, power, flexibility and functionality.  Having said that, I do think it's better than every single custom DAL I've come across so far.  Taking it to the next step however would take a lot more effort, which truly is never worth it.  As you try to provide more functionality, overall complexity of your custom DAL will increase heavily and the effort you'll eventually spend on it will more than outweigh any of the downsides that might come with using something that already exists.  Building your own DAL is an undertaking that should always be questioned, and shouldn't be considered unless the alternative of doing so is even worse.

If however, you're in a situation where it does make sense, then I hope this series might have been helpful for you.  I've shown that you can come up with something relatively decent without having to resort to code generation, without having to spend an insane amount of effort on it, and without having to write repetitive and error-prone code in your application code.  Those were the goals I had in mind when I started working on this DAL, and I think I've succeeded at achieving those goals.  The final result is an easy-to-use DAL which is far from as powerful as already existing solutions, but it is pretty good for the scenario's where we intend to use this.

The code itself is clear, easy to maintain and in some cases, very easy to extend as well.  In total, this DAL is slightly less than 1100 lines of code, and I think the complexity of the code is relatively low so everyone should be able to understand what's going on, how it works, and where things could be modified in order to fix issues or to add new features.

Also, this series of blog posts helps in figuring out how it works since pretty much every aspect of it is now documented pretty extensively :)

All in all, I had a lot of fun in writing both the DAL and this series of posts (which took another 8 hours in total).  

In the introduction of this series, I said that the purpose of this series was to:

* Show you that you really don’t need to resort to code generation to build your own custom DAL
* Show you what kind of complexity is involved with the implementation of a good DAL
* Convince you that you typically are better off with simply using something that is already available as a mature, powerful and proven solution

So tell me, did the series succeed in these listed goals? Would you still go for code generation if you had to create a custom DAL? Would you still prefer to use a custom DAL over something that already exists? How would you react to having to use this particular DAL?  What would need to be added or modified before you would find it acceptable?  What are your thoughts on this in general?

I'd be very interested in hearing about it, so please do post your opinions in the comments :)
