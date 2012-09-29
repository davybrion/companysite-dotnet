Just read a <a href="http://jonathan-oliver.blogspot.com/2009/10/ddd-entity-injection-and-mocking-time.html">post</a> by Jonathan Oliver where he talks about a solution he saw to tackle the problem of date-dependent code in tests.  As you probably know, code that uses the current date can very easily cause testability problems.  Code that accesses DateTime.Now or DateTime.UctNow can quite easily cause tests to fail for no valid reason when the tests happen to run on a certain date, on weekend days, at the end of the month, etc... 

The problem obviously is that you can't override the value that DateTime.Now will return during your tests.  Many people seem to resort to using some kind of DateTimeService dependency which each piece of code that needs the current date will use.  Basically, something like this:

<script src="https://gist.github.com/3685240.js?file=s1.cs"></script>

The implementation that will be used at runtime then looks like this:

<script src="https://gist.github.com/3685240.js?file=s2.cs"></script>

Code that needs the current date simply declares a dependency on IDateTimeService and at run-time the IOC container will inject an instance of DateTimeService to fulfill the IDateTimeService dependency.  At test-time, the IDateTimeService is mocked so you can easily set which date should be returned for each test. 

Personally, I'm not a fan of this approach.  I mean, I use the same approach for most dependencies but for simply getting the current date this is a bit too much IMO.

Instead, we simply use something like this:

<script src="https://gist.github.com/3685240.js?file=s3.cs"></script>

And our real code simply calls DateTimeProvider.Now instead of DateTime.Now.  In our tests, we call the SetDateTimeToReturn method to provide the date that we want to use for a particular test.  At the end of the test, simply call the ResetCurrentDateTime method and that's it.

I generally don't like static methods but in this case, this is a very simple solution to this specific problem.  And you know how the saying goes: "do the simplest thing that could possibly work".  And as long as that works, resist the urge to change it.