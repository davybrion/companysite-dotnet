Note: This post is part of a series. You can find the introduction and overview of the series <a href="http://davybrion.com/blog/2010/08/mvp-in-silverlightwpf-series">here</a>.

The MVP approach that i've shown in this series makes it easy to write clean code where responsibilities are properly separated.  One of the biggest benefits of that is that you get easy testability because of it as well.  Now, the MVVM proponents will tell you that MVVM leads to code that is highly testable as well and they're right.  However, i think that because of the cleaner separation in MVP, the automated tests you can write for your code is often cleaner, simpler, requiring less set up and often resulting in more focused tests.  I'm going to show a few tests of the sample project accompanying this series to show you what i mean.

First of all, let's start off with our BindingModels.  Every single thing that your BindingModel does should be tested.  That means testing whether the PropertyChanged event is always raised as expected, testing the validation you do, and testing that your population and mutation methods work properly.  

For instance, the UserGroupDetailBindingModel from the sample has a Name property.  Since the View binds to that property, we need to make sure that the PropertyChanged event is raised when its value is set.  We've also defined some validation for the Name property so we need to test that our BindingModel raises the ErrorsChanged event and that the correct validation message is made available through the GetErrors method.  Now, i'm a big fan of using base classes for test fixtures which contain utility assert methods which reduce code noise in your tests as much as possible.  It makes it possible to write tests like these:

<div>
[csharp]
        [TestMethod]
        public void SettingNameRaisesPropertyChangedEvent()
        {
            AssertThatPropertyChangesIsTriggeredCorrectly(m =&gt; m.Name, &quot;some name&quot;);
        }
 
        [TestMethod]
        public void SettingNameToInvalidValueCausesValidationError()
        {
            BindingModel.Name = null;
            AssertHasErrorMessageForProperty(m =&gt; m.Name, &quot;name is a required field&quot;);
        }
[/csharp]
</div>

The BindingModelFixture base class that is used in the sample automatically creates the correct BindingModel and exposes it through the BindingModel property.  There are various utility methods to assert that the PropertyChanged event is correctly raised when you want it to, as well as utility methods to assert that the validation is working correctly.  These utility methods use the BindingModel property, which is why we only need to pass an Expression to the utility methods in the 2 tests above to indicate which property we're testing.  

In the first test, the utility method will assign the given value (in this case a string) to the property passed in with the Expression and will then make sure that the PropertyChanged event for that property was indeed raised.  In the second test, the utility method will retrieve the errors of the current BindingModel and make sure that the expected error message is among them.  As you can see, both tests are very short and very simple.  And they most definitely provide value.  It doesn't really get any easier than that folks.

It's also important to test the methods you have on your model to populate it with data, or to change the data that is present, like these 2 tests for instance:

<div>
[csharp]
        [TestMethod]
        public void Populate_SetsSelectedParentUserGroupIfCurrentGroupHasParent()
        {
            var currentUserGroup = new UserGroupDto {Id = Guid.NewGuid(), ParentId = Guid.NewGuid()};
            var suitableParents = new[] {new UserGroupDto {Id = currentUserGroup.ParentId.Value}};
            BindingModel.Populate(suitableParents, currentUserGroup);
 
            Assert.AreEqual(currentUserGroup.ParentId.Value, BindingModel.SelectedParentUserGroup.Id);
        }
 
        [TestMethod]
        public void RevertToOriginalValues_SetsIdAndNameToOriginalValues()
        {
            var userGroup = new UserGroupDto {Id = Guid.NewGuid(), Name = &quot;some name&quot;};
            BindingModel.Populate(new UserGroupDto[0], userGroup);
            BindingModel.Id = Guid.NewGuid();
            BindingModel.Name = &quot;some other name&quot;;
 
            BindingModel.RevertToOriginalValues();
 
            Assert.AreEqual(userGroup.Id, BindingModel.Id);
            Assert.AreEqual(userGroup.Name, BindingModel.Name);
        }
[/csharp]
</div>

Again, both test are short, very simple and focused.  They don't have to worry about anything that isn't relevant to what we're actually trying to test.

I only showed 4 tests for BindingModels here, but the sample of this series has 30 tests for 3 BindingModels and i encourage you to check them out.

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

The only thing that makes testing the Presenter a little bit more difficult than typical automated tests, is the fact that the presenter always calls the service layer asynchronously.  Asynchronous calls are typically somewhat more complex to test, but luckily for me i'm using Agatha's RequestDispatchers to call the service layer, and i can use Agatha's RequestDispatcherStub class in my tests to make the whole thing a lot easier.

Let's go to some examples. The UserGroupDetailPresenter needs to retrieve the selected UserGroup from the service layer (to make sure we're working with the latest data), and it also needs to retrieve a list of suitable parent UserGroups.  When the data is returned from the service, the model needs to be populated.  Take a look at the following 3 tests:

<div>
[csharp]
        [TestMethod]
        public void RetrievesUserGroupDetails()
        {
            var userGroupId = Guid.NewGuid();
            Presenter.Handle(new UserGroupSelectedEvent(userGroupId));
            Assert.AreEqual(userGroupId, RequestDispatcherStub.GetRequest&lt;GetUserGroupRequest&gt;().UserGroupId);
        }
 
        [TestMethod]
        public void RetrievesSuitableParentUserGroups()
        {
            var userGroupId = Guid.NewGuid();
            Presenter.Handle(new UserGroupSelectedEvent(userGroupId));
            Assert.AreEqual(userGroupId, RequestDispatcherStub.GetRequest&lt;GetSuitableParentUserGroupsRequest&gt;().UserGroupId.Value);
        }
 

        [TestMethod]
        public void ResponsesReceived_PopulatesModel()
        {
            var userGroup = new UserGroupDto { Id = Guid.NewGuid() };
            var suitableParents = new[] { new UserGroupDto { Id = Guid.NewGuid() } };
 
            Presenter.Handle(new UserGroupSelectedEvent(Guid.NewGuid()));
            RequestDispatcherStub.SetResponsesToReturn(new GetUserGroupResponse { UserGroup = userGroup },
                                                       new GetSuitableParentUserGroupsResponse { SuitableParentUserGroups = suitableParents });
            RequestDispatcherStub.ReturnResponses();
 
            Assert.AreEqual(userGroup.Id, Presenter.BindingModel.Id);
            Assert.AreEqual(suitableParents[0].Id, Presenter.BindingModel.SuitableParentUserGroups[1].Id);
        }
[/csharp]
</div>

Once again, these tests are very short (except for the last one, which is still pretty short considering what it's actually testing) and highly focused.  We're not doing anything here that isn't relevant to what we're actually testing.  

We can also easily test whether the presenter interacts with the view as expected, like these 2 tests illustrate:

<div>
[csharp]
        [TestMethod]
        public void ResponsesReceived_DoesNotTellViewToPreventModificationIfUserHasPermission()
        {
            Presenter.Handle(new UserGroupSelectedEvent(Guid.NewGuid()));
            RequestDispatcherStub.SetResponsesToReturn(new GetUserGroupResponse(),
                                                       new GetSuitableParentUserGroupsResponse {SuitableParentUserGroups = new UserGroupDto[0]},
                                                       new CheckPermissionsResponse
                                                       {
                                                               AuthorizationResults = new Dictionary&lt;Guid, bool&gt; {{Permissions.DeleteUserGroup, true}, {Permissions.EditUserGroup, true}}
                                                       });
            RequestDispatcherStub.ReturnResponses();
 
            ViewMock.Verify(v =&gt; v.PreventModification(), Times.Never());
        }
 
        [TestMethod]
        public void ResponsesReceived_ShowsTheView()
        {
            var userGroup = new UserGroupDto { Id = Guid.NewGuid() };
            var suitableParents = new[] { new UserGroupDto { Id = Guid.NewGuid() } };
 
            Presenter.Handle(new UserGroupSelectedEvent(Guid.NewGuid()));
            RequestDispatcherStub.SetResponsesToReturn(new GetUserGroupResponse { UserGroup = userGroup },
                                                       new GetSuitableParentUserGroupsResponse { SuitableParentUserGroups = suitableParents });
            RequestDispatcherStub.ReturnResponses();
 
            ViewMock.Verify(v =&gt; v.Show());
        }
[/csharp]
</div>

For those wondering: i'm using the (excellent) <a href="http://code.google.com/p/moq/">Moq </a>library to mock the view in these tests.

I also said that we can easily test that the presenter doesn't make unwanted service layer calls, which you can see in this test:

<div>
[csharp]
        [TestMethod]
        public void DoesNotProceedIfModelIsInvalid()
        {
            Presenter.BindingModel.Name = null;
            Presenter.PersistChanges();
            Assert.IsFalse(RequestDispatcherStub.HasRequest&lt;SaveUserGroupRequest&gt;());
        }
[/csharp]
</div>

Testing whether the presenter publishes the correct events is pretty easy as well:

<div>
[csharp]
        [TestMethod]
        public void PublishesUserGroupDeletedEventWhenResponseIsReturned()
        {
            Presenter.BindingModel.Id = Guid.NewGuid();
            RequestDispatcherStub.SetResponsesToReturn(new DeleteUserGroupResponse());
 
            Presenter.Delete();
            RequestDispatcherStub.ReturnResponses();
 
            Assert.AreEqual(Presenter.BindingModel.Id.Value, EventAggregatorStub.GetPublishedEvents&lt;UserGroupDeletedEvent&gt;()[0].UserGroupId);
        }
[/csharp]
</div>

As you can see, we can really test a lot of code if we want to (you do want to test a lot of code don't you?).  And it really is pretty easy to do so.  The sample project of this series contains 47 tests for 2 presenters.  I again encourage you to check out those tests to see how easily you can cover a lot of functionality.