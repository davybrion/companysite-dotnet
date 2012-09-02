If you have a web application which communicates with a remote service, it's important to protect that web application from any problems the remote service might be dealing with.  For instance, if the remote service goes down (for whatever reason) you really don't want your application to keep making calls to this service.  These failing calls increase the load on the service, which is already having problems, and will also block your threads which takes away resources from your application to deal with other requests.  One pattern which is very suitable to reduce the problems for this situation is the <a href="http://davybrion.com/blog/2008/05/the-circuit-breaker/">Circuit Breaker</a> (read that unless you're familiar with the circuit breaker).

The biggest issue i have with my previous implementation is that it required you to call it manually to protect potentially risky calls.  I don't like having to call my circuit breaker whenever i want to make a service call because as a consumer of a service proxy, i shouldn't even know about the circuit breaker.  I also don't want any coupling between my service proxy and the actual circuit breaker.  Sounds like a good candidate for some AOP magic, right? 

We're going to use Castle Windsor's Interceptors to make this work.  First, the implementation of the CircuitBreaker class:

<div>
[csharp]
    public class CircuitBreaker : IInterceptor
    {
        private readonly object monitor = new object();
        private CircuitBreakerState state;
        private int failures;
        private int threshold;
        private TimeSpan timeout;
 
        public CircuitBreaker(int threshold, TimeSpan timeout)
        {
            this.threshold = threshold;
            this.timeout = timeout;
            MoveToClosedState();
        }
 
        public void Intercept(IInvocation invocation)
        {
            using (TimedLock.Lock(monitor))
            {
                state.ProtectedCodeIsAboutToBeCalled();
            }
 
            try
            {
                invocation.Proceed();
            }
            catch (Exception e)
            {
                using (TimedLock.Lock(monitor))
                {
                    failures++;
                    state.ActUponException(e);
                }
                throw;
            }
 
            using (TimedLock.Lock(monitor))
            {
                state.ProtectedCodeHasBeenCalled();
            }
        }
 
        private void MoveToClosedState()
        {
            state = new ClosedState(this);
        }
 
        private void MoveToOpenState()
        {
            state = new OpenState(this);
        }
 
        private void MoveToHalfOpenState()
        {
            state = new HalfOpenState(this);
        }
 
        private void ResetFailureCount()
        {
            failures = 0;
        }
 
        private bool ThresholdReached()
        {
            return failures &gt;= threshold;
        }
 
        private abstract class CircuitBreakerState
        {
            protected readonly CircuitBreaker circuitBreaker;
 
            protected CircuitBreakerState(CircuitBreaker circuitBreaker)
            {
                this.circuitBreaker = circuitBreaker;
            }
 
            public virtual void ProtectedCodeIsAboutToBeCalled() { }
            public virtual void ProtectedCodeHasBeenCalled() { }
            public virtual void ActUponException(Exception e) { }
        }
 
        private class ClosedState : CircuitBreakerState
        {
            public ClosedState(CircuitBreaker circuitBreaker)
                : base(circuitBreaker)
            {
                circuitBreaker.ResetFailureCount();
            }
 
            public override void ActUponException(Exception e)
            {
                if (circuitBreaker.ThresholdReached()) circuitBreaker.MoveToOpenState();
            }
        }
 
        private class OpenState : CircuitBreakerState
        {
            private readonly Timer timer;
 
            public OpenState(CircuitBreaker circuitBreaker)
                : base(circuitBreaker)
            {
                timer = new Timer(circuitBreaker.timeout.TotalMilliseconds);
                timer.Elapsed += TimeoutHasBeenReached;
                timer.AutoReset = false;
                timer.Start();
            }
 
            private void TimeoutHasBeenReached(object sender, ElapsedEventArgs e)
            {
                circuitBreaker.MoveToHalfOpenState();
            }
 
            public override void ProtectedCodeIsAboutToBeCalled()
            {
                throw new OpenCircuitException();
            }
        }
 
        private class HalfOpenState : CircuitBreakerState
        {
            public HalfOpenState(CircuitBreaker circuitBreaker) : base(circuitBreaker) { }
 
            public override void ActUponException(Exception e)
            {
                circuitBreaker.MoveToOpenState();
            }
 
            public override void ProtectedCodeHasBeenCalled()
            {
                circuitBreaker.MoveToClosedState();
            }
        }
    }
[/csharp]
</div>

Notice how the CircuitBreaker class implements Windsor's IInterceptor interface.  The Intercept method will be called by Windsor whenever we try to call a method from a protected component.  Within the Intercept method we can add the necessary logic to apply the Circuit Breaker pattern to the code that was originally called. 

Now we just need to configure the Windsor IOC container to apply this bit of AOP magic for us.

First, we register the CircuitBreaker with the container:

<div>
[csharp]
            container.Register(Component.For&lt;CircuitBreaker&gt;().LifeStyle.Singleton
                                   .Named(&quot;serviceProxyCircuitBreaker&quot;)
                                   .DependsOn(new Hashtable { { &quot;threshold&quot;, 5 }, { &quot;timeout&quot;, TimeSpan.FromMinutes(5) } }));
[/csharp]
</div>

Notice that we register the CircuitBreaker implementation with a Singleton lifestyle, a custom name and the required constructor parameters to create an instance of the CircuitBreaker.

Then we register our service proxy:

<div>
[csharp]
            container.Register(Component.For&lt;IServiceProxy&gt;().ImplementedBy&lt;ServiceProxy&gt;().LifeStyle.Transient
                                .Interceptors(InterceptorReference.ForKey(&quot;serviceProxyCircuitBreaker&quot;)).Anywhere);
[/csharp]
</div>

Notice how we registered the service proxy as a transient component, while referencing the singleton CircuitBreaker interceptor.  This means that each resolved instance of our service proxy will be protected by the same CircuitBreaker instance.  If you have multiple services that you want to protect, simply register multiple CircuitBreakers with different keys and link each service you want to protect with the correct CircuitBreaker key.