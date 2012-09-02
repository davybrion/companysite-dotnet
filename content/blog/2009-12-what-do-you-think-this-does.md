<p>I’m only going to show a part of a QuickNet Acid Test, and i’m intentionally leaving a lot of things out:</p> 

<div>
[csharp]
        private void EnsureExceptionInfoIsCorrect(Predicate&lt;Exception&gt; exceptionPredicate, ExceptionType exceptionTypeEnum, Type exceptionType, Response[] responses)
        {
            int index = exceptionsThrownFromRequestHandlers.FindIndex(exceptionPredicate);
            Ensure.Equal(exceptionTypeEnum, responses[index].ExceptionType);
            Ensure.Equal(exceptionsThrownFromRequestHandlers[index].Message, responses[index].Exception.Message);
            Ensure.Equal(exceptionsThrownFromRequestHandlers[index].StackTrace, responses[index].Exception.StackTrace);
            Ensure.Equal(exceptionType.FullName, responses[index].Exception.Type);
        }
 
        [SpecFor(typeof(ProcessRequestsTransition))]
        public Spec ProcessRequestsWithBusinessException(ProcessInput input, Response[] output)
        {
            Predicate&lt;Exception&gt; predicate = exception =&gt; exception != null &amp;&amp; exception.GetType() == typeof(BusinessException);
 
            return new Spec(() =&gt; EnsureExceptionInfoIsCorrect(predicate, ExceptionType.Business, typeof(BusinessException), output))
                .IfAfter(() =&gt; exceptionsThrownFromRequestHandlers.Exists(predicate));
        }
 
        [SpecFor(typeof(ProcessRequestsTransition))]
        public Spec ProcessRequestsWithSecurityException(ProcessInput input, Response[] output)
        {
            Predicate&lt;Exception&gt; predicate = exception =&gt; exception != null &amp;&amp; exception.GetType() == typeof(SecurityException);
 
            return new Spec(() =&gt; EnsureExceptionInfoIsCorrect(predicate, ExceptionType.Security, typeof(SecurityException), output))
                .IfAfter(() =&gt; exceptionsThrownFromRequestHandlers.Exists(predicate));
        }
 
        [SpecFor(typeof(ProcessRequestsTransition))]
        public Spec ProcessRequestsWithUnknownException(ProcessInput input, Response[] output)
        {
            Predicate&lt;Exception&gt; predicate = exception =&gt; exception != null &amp;&amp; exception.GetType() == typeof(UnknownException);
 
            return new Spec(() =&gt; EnsureExceptionInfoIsCorrect(predicate, ExceptionType.Unknown, typeof(UnknownException), output))
                .IfAfter(() =&gt; exceptionsThrownFromRequestHandlers.Exists(predicate));
        }
 
        [SpecFor(typeof(ProcessRequestsTransition))]
        public Spec ProcessRequestsWithAnotherUnknownException(ProcessInput input, Response[] output)
        {
            Predicate&lt;Exception&gt; predicate = exception =&gt; exception != null &amp;&amp; exception.GetType() == typeof(AnotherUnknownException);
 
            return new Spec(() =&gt; EnsureExceptionInfoIsCorrect(predicate, ExceptionType.Unknown, typeof(AnotherUnknownException), output))
                .IfAfter(() =&gt; exceptionsThrownFromRequestHandlers.Exists(predicate));
        }
[/csharp]
</div>

<p>Now, what do you think this does? </p>