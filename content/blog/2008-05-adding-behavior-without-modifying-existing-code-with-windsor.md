The Windsor container makes it quite easy to add behavior to components, without having to modify their implementation. This could be useful in many scenario's. Suppose you need to log whenever a method from our OrderRepository class is called.  But we should be able to turn the logging on and off whenever we want.  Preferably, without having to modify the code all the time. Now, you could easily write a logger class that checks for a configuration setting and only logs when needed. This approach would definitely work. But then there's logging code all over the OrderRepository class and in most cases, it's not even necessary since they only want to be able to log under certain circumstances. Should the OrderRepository class really care about the logging? Why litter the code with logging statements?

If you're using the Windsor container, you could easily add logging behavior to the OrderRepository class without having to change any of the existing code. Windsor has this concept of Interceptors. Basically you can assign an interceptor to any component and you can plug in your custom behavior when the component is called. Lets get into an example... Since logging is such a common requirement, we decided to put it in one class instead of littering our entire code base with logging statements. So we wrote the following class:

<script src="https://gist.github.com/3612352.js?file=s1.cs"></script>

This class implements the IInterceptor interface by implementing the Intercept method. When that method is called we simply construct the full method name, log when we enter the method, call the original method and then we log again when we leave the method.  Nothing more, nothing less. Also notice how the LoggingInterceptor has a dependency on an ILogger instance. That instance will be injected by the container as well.

So first of all, we need to define the ILogger and LoggingInterceptor components:

<script src="https://gist.github.com/3612352.js?file=s2.xml"></script>

Right... so now we need to add this behavior to the OrderRepository class. This only requires modifying the registration of the IOrderRepository component:

<script src="https://gist.github.com/3612352.js?file=s3.xml"></script>

What we basically did was tell Windsor that whenever an IOrderRepository is requested, we should return an instance of OrderRepository and each time a method of that instance is called, it needs to be intercepted by our LoggingInterceptor.

So if we simply call the IOrderRepository methods like this (obviously these are dummy calls without real parameters and we're also ignoring return values):

<script src="https://gist.github.com/3612352.js?file=s4.cs"></script>

The following output is logged:

<script src="https://gist.github.com/3612352.js?file=s5.txt"></script>

And we didn't have to change the OrderRepository implementation. In fact, we can use our LoggingInterceptor wherever we like, as long as the component to be logged is registered with Windsor.  And we can easily switch between logging or no logging by switching config files.

Obviously, this was just a really simple example but i hope you realize how powerful this technique is and how far you can go with this.
