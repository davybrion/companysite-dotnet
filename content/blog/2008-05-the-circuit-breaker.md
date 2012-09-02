One of the coolest patterns discussed in <a href="http://www.amazon.com/Release-Production-Ready-Software-Pragmatic-Programmers/dp/0978739213/ref=pd_bbs_sr_1?ie=UTF8&s=books&qid=1211066583&sr=8-1">Release It</a> is the Circuit Breaker pattern. The pattern enables you to wrap dangerous or risky operations with a component that avoids calling the real operation when a certain failure rate has been reached.  For instance, suppose you need to call an external web service in your application. The external web service is known to be flaky at times which causes each call to it to hang for a while before it eventually fails. Now, if that service is in a bad state, your application threads will waste a lot of time waiting on the failure.  Wouldn't it be better if your application could detect when the service is in a bad shape and in that case, immediately throw an exception when the service is called? As the author of Release It often says: it's better to fail fast if you know something's wrong. 

The Circuit Breaker pattern allows you to do this pretty easily. The Circuit Breaker basically has 3 states: Closed, Open and HalfOpen. When in Closed state, each call to the protected resource is allowed. But each time it fails, a failure counter is incremented, and when the failure counter reaches a configurable threshold, the Circuit Breaker moves to the Open state.  When it moves to Open state, it starts a timer set to elapse at a configurable timeout value.  If the timeout has not been reached, each call to the protected resource is not allowed, and an exception is thrown to indicate that the Circuit Breaker is not allowing calls at the moment.  When the timeout has been reached, the Circuit Breaker moves to HalfOpen state. In this state, the next call to the protected resource is allowed, but if it fails, the Circuit Breaker immediately switches back to the Open state and the timeout period starts again. If the call to the protected resource while in HalfOpen state succeeds, the Circuit Breaker switches back to the Closed state.  I hope i explained it clearly, i didn't really find a good explanation of the Circuit Breaker pattern online... one more reason to buy the Release It book i suppose ;)

Anyway, let's try implementing a Circuit Breaker in C#.  Since the Circuit Breaker operates differently depending on its State, this is perfect candidate for using the <a href="http://en.wikipedia.org/wiki/State_pattern">State pattern</a>.  First, lets define the CircuitBreakerState class:

<div>
[csharp]
    public abstract class CircuitBreakerState
    {
        protected readonly CircuitBreaker circuitBreaker;
 
        protected CircuitBreakerState(CircuitBreaker circuitBreaker)
        {
            this.circuitBreaker = circuitBreaker;
        }
 
        public virtual void ProtectedCodeIsAboutToBeCalled() { }
        public virtual void ProtectedCodeHasBeenCalled() { }
        public virtual void ActUponException(Exception e) { circuitBreaker.IncreaseFailureCount(); }
    }
[/csharp]
</div>

Pretty simple stuff so far... There are 3 virtual methods that will be called by the CircuitBreaker. I think the names speak for themselves so you shouldn't have any problem figuring out when these methods will be called. Each derived state simply needs to plug in its necessary logic in the right method and then the right logic will be executed at the right time, according to the state the CircuitBreaker is in.

Let's take a look at what should happen when the CircuitBreaker is in Closed state:

<div>
[csharp]
    public class ClosedState : CircuitBreakerState
    {
        public ClosedState(CircuitBreaker circuitBreaker) : base(circuitBreaker)
        {
            circuitBreaker.ResetFailureCount();
        }
 
        public override void ActUponException(Exception e)
        {
            base.ActUponException(e);
            if (circuitBreaker.ThresholdReached()) circuitBreaker.MoveToOpenState();
        }
    }
[/csharp]
</div>

When a Closed state instance is created, it resets the failure count of the CircuitBreaker. When an exception is thrown by the protected code, the failure count is increased (the CircuitBreakerState base class does this) and then we check if the failure threshold has been reached and if so, we instruct the CircuitBreaker to move to the Open state.

This is how the Open state looks:

<div>
[csharp]
    public class OpenState : CircuitBreakerState
    {
        private readonly Timer timer;
 
        public OpenState(CircuitBreaker circuitBreaker) : base(circuitBreaker)
        {
            timer = new Timer(circuitBreaker.Timeout.TotalMilliseconds);
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
            base.ProtectedCodeIsAboutToBeCalled();
            throw new OpenCircuitException();
        }
    }
[/csharp]
</div>

When an Open state instance is created, it starts a timer which is set to elapse when the CircuitBreaker's timeout period has been reached. When the timeout has been reached, the CircuitBreaker is moved to the HalfOpen state. When the protected code is about to be called when we're in this state, we throw an OpenCircuitException to prevent the protected code from being called.

This is what the HalfOpen state looks like:

<div>
[csharp]
    public class HalfOpenState : CircuitBreakerState
    {
        public HalfOpenState(CircuitBreaker circuitBreaker) : base(circuitBreaker) {}
 
        public override void ActUponException(Exception e)
        {
            base.ActUponException(e);
            circuitBreaker.MoveToOpenState();
        }
 
        public override void ProtectedCodeHasBeenCalled()
        {
            base.ProtectedCodeHasBeenCalled();
            circuitBreaker.MoveToClosedState();
        }
    }
[/csharp]
</div>

If an exception is thrown by the protected code while we're in this state, we immediately move back to the Open state to prevent the protected code from being called again.  But if the protected code has been called without exceptions, we can move back to the Closed state.

We've implemented all the state-based logic in these state classes.  Now we can easily implement the CircuitBreaker.  When you create the CircuitBreaker, you pass the failure threshold and the timeout period to the constructor.  After that, you can pass any code block to the AttemptCall method.

Here's the full code of the CircuitBreaker:

<div>
[csharp]
    public class CircuitBreaker
    {
        private readonly object monitor = new object();
        private CircuitBreakerState state;
 
        public CircuitBreaker(int threshold, TimeSpan timeout)
        {
            if (threshold &lt; 1)
            {
                throw new ArgumentOutOfRangeException(&quot;threshold&quot;, &quot;Threshold should be greater than 0&quot;);
            }
 
            if (timeout.TotalMilliseconds &lt; 1)
            {
                throw new ArgumentOutOfRangeException(&quot;timeout&quot;, &quot;Timeout should be greater than 0&quot;);
            }
 
            Threshold = threshold;
            Timeout = timeout;
            MoveToClosedState();
        }
 
        public int Failures { get; private set; }
        public int Threshold { get; private set; }
        public TimeSpan Timeout { get; private set; }
 
        public bool IsClosed
        {
            get { return state is ClosedState; }
        }
 
        public bool IsOpen
        {
            get { return state is OpenState; }
        }
 
        public bool IsHalfOpen
        {
            get { return state is HalfOpenState; }
        }
 
        internal void MoveToClosedState()
        {
            state = new ClosedState(this);
        }
 
        internal void MoveToOpenState()
        {
            state = new OpenState(this);
        }
 
        internal void MoveToHalfOpenState()
        {
            state = new HalfOpenState(this);
        }
 
        internal void IncreaseFailureCount()
        {
            Failures++;
        }
 
        internal void ResetFailureCount()
        {
            Failures = 0;
        }
 
        public bool ThresholdReached()
        {
            return Failures &gt;= Threshold;
        }
 
        public void AttemptCall(Action protectedCode)
        {
            using (TimedLock.Lock(monitor))
            {
                state.ProtectedCodeIsAboutToBeCalled();
            }
 
            try
            {
                protectedCode();
            }
            catch (Exception e)
            {
                using (TimedLock.Lock(monitor))
                {
                    state.ActUponException(e);
                }
                throw;
            }
 
            using (TimedLock.Lock(monitor))
            {
                state.ProtectedCodeHasBeenCalled();
            }
        }
 
        public void Close()
        {
            using (TimedLock.Lock(monitor))
            {
                MoveToClosedState();
            }
        }
 
        public void Open()
        {
            using (TimedLock.Lock(monitor))
            {
                MoveToOpenState();
            }
        }
    }
[/csharp]
</div>

Here's a small example of how you could use it:

<div>
[csharp]
        public void ProcessOrder(OrderInfo orderInfo)
        {
            var proxy = new OrderProcessorServiceProxy();
            _orderProcessoryCircuitBreaker.AttemptCall(() =&gt; proxy.ProcessOrder(orderInfo));
        }
[/csharp]
</div>

You can download the solution (code + tests) <a href="http://davybrion.com/files/circuitbreaker.zip">here</a>.
