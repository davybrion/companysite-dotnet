If you have a web application which communicates with a remote service, it's important to protect that web application from any problems the remote service might be dealing with.  For instance, if the remote service goes down (for whatever reason) you really don't want your application to keep making calls to this service.  These failing calls increase the load on the service, which is already having problems, and will also block your threads which takes away resources from your application to deal with other requests.  One pattern which is very suitable to reduce the problems for this situation is the <a href="/blog/2008/05/the-circuit-breaker/">Circuit Breaker</a> (read that unless you're familiar with the circuit breaker).

The biggest issue i have with my previous implementation is that it required you to call it manually to protect potentially risky calls.  I don't like having to call my circuit breaker whenever i want to make a service call because as a consumer of a service proxy, i shouldn't even know about the circuit breaker.  I also don't want any coupling between my service proxy and the actual circuit breaker.  Sounds like a good candidate for some AOP magic, right? 

We're going to use Castle Windsor's Interceptors to make this work.  First, the implementation of the CircuitBreaker class:

<script src="https://gist.github.com/3684582.js?file=s1.cs"></script>

Notice how the CircuitBreaker class implements Windsor's IInterceptor interface.  The Intercept method will be called by Windsor whenever we try to call a method from a protected component.  Within the Intercept method we can add the necessary logic to apply the Circuit Breaker pattern to the code that was originally called. 

Now we just need to configure the Windsor IOC container to apply this bit of AOP magic for us.

First, we register the CircuitBreaker with the container:

<script src="https://gist.github.com/3684582.js?file=s2.cs"></script>

Notice that we register the CircuitBreaker implementation with a Singleton lifestyle, a custom name and the required constructor parameters to create an instance of the CircuitBreaker.

Then we register our service proxy:

<script src="https://gist.github.com/3684582.js?file=s3.cs"></script>

Notice how we registered the service proxy as a transient component, while referencing the singleton CircuitBreaker interceptor.  This means that each resolved instance of our service proxy will be protected by the same CircuitBreaker instance.  If you have multiple services that you want to protect, simply register multiple CircuitBreakers with different keys and link each service you want to protect with the correct CircuitBreaker key.