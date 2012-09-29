One approach to implement business logic that I've seen a couple of times now, is to use a class that implements an execution pipeline, which triggers virtual methods in a specific order and provides general error handling or transaction management for that pipeline.  The idea is usually to get most developers to write their code in a similar manner, while trying to relieve them of the burden of exception handling, transaction management or whatever else you want to provide in the pipeline execution.  This approach is far from ideal (personally I think it sucks), but when you're facing a large application with a bunch of code that already uses this, you pretty much gotta live with it.

Here's a simplified example of a class implementing such an execution pipeline:

<script src="https://gist.github.com/3675532.js?file=s1.cs"></script>

There are many problems with this approach, the most important one being that most developers usually put too much code directly within the virtual methods and usually even using concrete dependencies, which leads to all kinds of testing difficulties.  I hope you spotted the outdated exception handling approach, but I'm not even gonna get into that in this post. 

Here's an example of how a small piece of business logic might be implemented with this approach:

<script src="https://gist.github.com/3675532.js?file=s2.cs"></script>

If you've been reading this blog for a while, then I hope you can spot the 2 biggest problems already. The first is this line:

<script src="https://gist.github.com/3675532.js?file=s3.cs"></script>

And the second is this one:

<script src="https://gist.github.com/3675532.js?file=s4.cs"></script>

These are 2 concrete dependencies which can't easily be replaced with mock implementations while testing this code.  Basically, any code that is performed by the data access layer component, or the AuthorizationManager's static IsAllowedToAccess method will have to be set up properly in every test that you write for this class.  Depending on what needs to be done during that set up, this can really become a major pain in the ass.  For instance, if you wanted to write a test that would check if the right error message was created when the user is not allowed to access this command, then you have to make sure that the call to IsAllowedToAccess method actually fails.  Depending on the implementation of the AuthorizationManager, this can be quite tedious work.  Keep in mind that because of the template method pipeline approach that is used here, you need to set this up for <strong>every</strong> test for this class.  The same thing goes for the call to the GetOutstandingOrderCount method of the data access layer component that is being performed in the ValidateInput method. 

Suppose you have the following tests for this class:

 * AddsErrorMessageWhenUserDoesntHaveAccessToThisCommand
 * DoesNotAddErrorMessageWhenuserDoesntHaveAccessToThisCommand
 * AddsErrorMessageWhenCustomerHasOutstandingOrders
 * DoesNotAddErrorMessageWhenCustomerHasNoOutstandingOrders
 * DeletesCustomerWhenThereAreNoErrorMessages
 * DoesNotDeleteCustomerWhenThereAreNoErrorMessages

For each test, the entire pipeline is executed.  That means that for each test, you need to set up data that is not always relevant to the test itself.  You're actually doing more setting up than you really need to do for that small piece of code you're trying to test.  You can see where this is going right? These 6 tests would basically lead to unecessary expensive setup.

Now suppose you have an application where you have hundreds of these command classes, and thousands of tests.  Running all the tests becomes so slow it's unbearable, not to mention all the wasted effort that has been spent writing all that unnecessary setup, and maintaining it as well because you can be pretty sure that a lot of tests are pretty fragile.

Now, ideally you'd want to make sure that you could talk to a mocked AuthorizationManager and to a mocked data access layer component when this class is being tested.  After all, these are dependencies of this class, and thus, the functionality offered by those dependencies should be covered by <strong>their own tests</strong>. So to correctly test the functionality of this class, you really don't need to use the actual implementations.  In the tests of this command class, you need to verify that it <strong>reacts</strong> properly to the possible return values of the dependencies.

Unfortunately, refactoring to the approach I just mentioned is not always feasible.  So how do you try to minimize the pain? A coworker of mine actually pointed out an approach that allows us to at least minimize the set up that is needed for some tests, without having to redesign the existing code.  His suggestion was to mock template methods in the pipeline that were unrelated to the current test.  If you can't redesign the existing code, this is probably the next best thing.

Let's give it a shot.  Suppose we want to test the CheckAuthorization method.  We basically don't need anything to happen in the other methods when we're testing the authorization, so we'll try to mock those with empty implementations:

<script src="https://gist.github.com/3675532.js?file=s5.cs"></script>

Note that in order for this to work, you have to change the protected virtual methods to internal protected (and use the InternalsVisibleTo attribute if your tests are in a different assembly).  For this test, we only had to set up the authorization manager, nothing else.

If you want to test the ValidateInput method, you could do something like this:

<script src="https://gist.github.com/3675532.js?file=s6.cs"></script>

In this test, we only had to set up the order for the customer, but we didn't have to set up the authorization manager.  If you implement your tests this way instead of going through the entire pipeline each time, and thus, taking the performance hit of all the unnecessary set up every time, you can probably cut a lot of time off of those test runs.

Note: again, I want to make it clear that I don't advocate using the template method pipeline approach. If you absolutely want to use it, at least inject each dependency in a way that allows easy mocking.  If you can't modify existing code that already is implemented like this, the partial mocking technique in this post is a good idea, but ideally, this entire thing should be avoided.
