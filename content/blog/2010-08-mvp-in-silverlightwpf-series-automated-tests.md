Note: This post is part of a series. You can find the introduction and overview of the series <a href="/blog/2010/08/mvp-in-silverlightwpf-series">here</a>.

The MVP approach that I've shown in this series makes it easy to write clean code where responsibilities are properly separated.  One of the biggest benefits of that is that you get easy testability because of it as well.  Now, the MVVM proponents will tell you that MVVM leads to code that is highly testable as well and they're right.  However, I think that because of the cleaner separation in MVP, the automated tests you can write for your code is often cleaner, simpler, requiring less set up and often resulting in more focused tests.  I'm going to show a few tests of the sample project accompanying this series to show you what I mean.

First of all, let's start off with our BindingModels.  Every single thing that your BindingModel does should be tested.  That means testing whether the PropertyChanged event is always raised as expected, testing the validation you do, and testing that your population and mutation methods work properly.  

For instance, the UserGroupDetailBindingModel from the sample has a Name property.  Since the View binds to that property, we need to make sure that the PropertyChanged event is raised when its value is set.  We've also defined some validation for the Name property so we need to test that our BindingModel raises the ErrorsChanged event and that the correct validation message is made available through the GetErrors method.  Now, I'm a big fan of using base classes for test fixtures which contain utility assert methods which reduce code noise in your tests as much as possible.  It makes it possible to write tests like these:

<script src="https://gist.github.com/3728116.js?file=s1.cs"></script>

The BindingModelFixture base class that is used in the sample automatically creates the correct BindingModel and exposes it through the BindingModel property.  There are various utility methods to assert that the PropertyChanged event is correctly raised when you want it to, as well as utility methods to assert that the validation is working correctly.  These utility methods use the BindingModel property, which is why we only need to pass an Expression to the utility methods in the 2 tests above to indicate which property we're testing.  

In the first test, the utility method will assign the given value (in this case a string) to the property passed in with the Expression and will then make sure that the PropertyChanged event for that property was indeed raised.  In the second test, the utility method will retrieve the errors of the current BindingModel and make sure that the expected error message is among them.  As you can see, both tests are very short and very simple.  And they most definitely provide value.  It doesn't really get any easier than that folks.

It's also important to test the methods you have on your model to populate it with data, or to change the data that is present, like these 2 tests for instance:

<script src="https://gist.github.com/3728116.js?file=s2.cs"></script>

Again, both test are short, very simple and focused.  They don't have to worry about anything that isn't relevant to what we're actually trying to test.

I only showed 4 tests for BindingModels here, but the sample of this series has 30 tests for 3 BindingModels and I encourage you to check them out.

Obviously, you want to cover the logic in your Presenters with valuable and maintainable tests as well.  Typical things that you need to test in the presenter are things like:
<ul>
	<li>making sure the presenter makes the correct service layer calls at the right time</li>
	<li>making sure that the presenter doesn't inadvertently make unwanted service layer calls at the wrong time</li>
	<li>that it handles received events from the Event Aggregator correctly</li>
	<li>that it publishes the correct events through the Event Aggregator at the correct time</li>
	<li>that it doesn't publish events through the Event Aggregator when it shouldn't do so</li>
	<li>that it interacts with the BindingModel correctly</li>
	<li>that it interacts with the View correctly</li>
</ul>

The only thing that makes testing the Presenter a little bit more difficult than typical automated tests, is the fact that the presenter always calls the service layer asynchronously.  Asynchronous calls are typically somewhat more complex to test, but luckily for me I'm using Agatha's RequestDispatchers to call the service layer, and I can use Agatha's RequestDispatcherStub class in my tests to make the whole thing a lot easier.

Let's go to some examples. The UserGroupDetailPresenter needs to retrieve the selected UserGroup from the service layer (to make sure we're working with the latest data), and it also needs to retrieve a list of suitable parent UserGroups.  When the data is returned from the service, the model needs to be populated.  Take a look at the following 3 tests:

<script src="https://gist.github.com/3728116.js?file=s3.cs"></script>

Once again, these tests are very short (except for the last one, which is still pretty short considering what it's actually testing) and highly focused.  We're not doing anything here that isn't relevant to what we're actually testing.  

We can also easily test whether the presenter interacts with the view as expected, like these 2 tests illustrate:

<script src="https://gist.github.com/3728116.js?file=s4.cs"></script>

For those wondering: I'm using the (excellent) <a href="http://code.google.com/p/moq/">Moq</a> library to mock the view in these tests.

I also said that we can easily test that the presenter doesn't make unwanted service layer calls, which you can see in this test:

<script src="https://gist.github.com/3728116.js?file=s5.cs"></script>

Testing whether the presenter publishes the correct events is pretty easy as well:

<script src="https://gist.github.com/3728116.js?file=s6.cs"></script>

As you can see, we can really test a lot of code if we want to (you do want to test a lot of code don't you?).  And it really is pretty easy to do so.  The sample project of this series contains 47 tests for 2 presenters.  I again encourage you to check out those tests to see how easily you can cover a lot of functionality.