Note: This post is part of a series. You can find the introduction and overview of the series <a href="/blog/2010/08/mvp-in-silverlightwpf-series">here</a>.

The first UserControl that we're going to implement looks like this:

<a href="/postcontent/sample_overview1.png"><img src="/postcontent/sample_overview1.png" alt="" title="sample_overview" width="500" height="344" class="aligncenter size-full wp-image-2452" /></a>

There's a TreeView which shows a hierarcy of User Groups and there's a Button to create a new one. When this UserControl is first loaded, it needs to retrieve the User Group hierarchy and show it in the TreeView.  When a user selects a User Group, an event should be published to notify anyone else who might be interested in the selection of a User Group (in our case, our Details UserControl which I'll cover in the next post).  When the Button is pushed, another event is published to notify anyone who might be listening that we need a new User Group to be created.  Again, this event will be handled by our Details UserControl.

So what exactly does this UserControl need to do? After all, showing some data and publishing a few events isn't really all that complex, right?  Well, it also has to listen to some other events and it needs to update the data it's showing accordingly.  First of all, if a new User Group is created, it will receive an event so it can add the new User Group to its TreeView.  If a User Group is modified, it will also receive an event so it can deal with that.  In the case of a modification, it might be as simple as updating the User Group's name, but it might also require removing the User Group from its current parent in the TreeView and attach it to anther parent.  It might also become a root User Group.  And finally, if a User Group is deleted, it will also receive an event so it can remove the User Group from the TreeView.  

Now, some of you are probably thinking: why not just retrieve the User Group hierarchy again on every change and bind to that? Well, that would be the easiest way out, but that's not necessarily the best option.  You already have all the information you need to update your TreeView, so there's no need to bother both the server to execute the request, and the user who will be waiting for the request to complete.  You might be thinking "what's the big deal? a single service call doesn't take long enough to be noticed".  And you might be right.  A single service call will hardly be noticeable.  Especially not while you're developing and testing your software on your (overpowered) development workstation.  But resort to that approach too frequently, and all those 'single service calls' that <em>all of your users</em> will be executing will start to add up.  Also, keep in mind that you're not developing a typical web application.  You are not required to work stateless, so you're not required to reload everything you need all the time.  In fact, it would be wise to take advantage of the state that your client can hold since it enables you to reduce server load, and to improve overall responsiveness (and thus, perceived performance) of the client. So we skip the lazy approach and we update the TreeView with the data that we already have.

Oh and obviously, we need to worry about permissions as well.  So we need to make sure that a user can't create a new User Group if the user doesn't actually have the required permission to do that.

Right, let's get started on this, shall we?

First of all, we need to look at what kind of data we're going to get back from the Service Layer.  The response to our GetAllUserGroupsRequest looks like this:

<script src="https://gist.github.com/3728019.js?file=s1.cs"></script>

(If you're unfamiliar with this kind of Request/Response Service Layer implementation, you can catch up <a href="/blog/2009/11/requestresponse-service-layer-series/">here</a>)

And the UserGroupDto class looks like this:

<script src="https://gist.github.com/3728019.js?file=s2.cs"></script>

So basically, the data we're getting back from the Service Layer is a flattened (as in: non-hierarchical) list of UserGroupDto's.  And that makes sense, since we can't expect our Service Layer to know that we want to visualize this in a TreeView... after all, the same Request could be used in other situations where a hierarchical representation isn't required.  That does mean that we have some work to do though.  We need to convert that flattened list of data in a hierarchical model that we can bind to our TreeView.  So we could begin with the following simple HierarchicalUserGroupBindingModel:

<script src="https://gist.github.com/3728019.js?file=s3.cs"></script>

Nothing special here... just something simple which we can use to show a UserGroup with its children.  Perfect for displaying a hierarchy, right?  We can have multiple root User Groups though, so we need something that will contain these HierarchicalUserGroupBindingModel instances.  We can start off with something like this:

<script src="https://gist.github.com/3728019.js?file=s4.cs"></script>

This structure is sufficient for holding the data that our TreeView can bind to, but we're going to need to add some code to deal with all the changes that might occur.  First, we need to be able to transform the flattened list of UserGroupDto's that we receive from the Service Layer to this hierarchical model that we'll be using here.  So we add the following method to the UserGroupsBindingModel:

<script src="https://gist.github.com/3728019.js?file=s5.cs"></script>

(not your typical way to transform a flattened list into a hierarchy, but I figured: why not go for something different for a change?)

Alright... we can already transform the list of UserGroupDtos that we'll receive from the Service Layer into a hierarchical structure that we can bind our TreeView too.  But there are a few more things that we need to be able to support for this hierarchical structure.  For starters, we need to be able to add a new User Group to this structure, so we add the following method to the UserGroupsBindingModel:

<script src="https://gist.github.com/3728019.js?file=s6.cs"></script>

A User Group can't just be added, it can be removed too.  So we need to add some more code to support that:

<script src="https://gist.github.com/3728019.js?file=s7.cs"></script>

Finally, we also need to support changes to existing User Groups, so once again we add a new method to the UserGroupsBindingModel:

<script src="https://gist.github.com/3728019.js?file=s8.cs"></script>

We already have quite a bit of code huh? But if you think about it, we already have everything we need to correctly show and update the data in our TreeView.  And that is by far the hardest part of this UserControl.  These BindingModels have a specific purpose and they only contain code to serve that purpose. Also, none of the methods that you can call on these 2 BindingModels can cause unexpected side effects (like making Service Layer calls for instance).  The code we've written so far is very cohesive, and there is very little coupling to speak of.

Now we can start working on our Presenter.  We first add a new class which looks like this:

<script src="https://gist.github.com/3728019.js?file=s9.cs"></script>

Notice that one of the generic type parameters that we pass to the base Presenter class is the IUserGroupsView interface.  We don't have that interface yet so we'll need to create it first:

<script src="https://gist.github.com/3728058.js?file=s1.cs"></script>

Now we can write the code we need to make sure that our UserGroupsPresenter will retrieve the User Groups from the Service Layer, and populate the UserGroupsBindingModel so the TreeView in our View could bind to it and display it.  Let's add the following Initialize method to our Presenter:

<script src="https://gist.github.com/3728058.js?file=s2.cs"></script>

We use <a href="http://code.google.com/p/agatha-rrsl/">Agatha</a>'s IAsyncRequestDispatcherFactory to create an IAsyncRequestDispatcher that we can use to send our requests to the Service Layer.  Notice that we send 2 requests: one to retrieve the User Groups, and another one to see whether our user has permission to create a new User Group.  Naturally, we make use of Agatha's service call batching capabilities because as responsible developers, we don't want to make more (expensive) remote calls than we need to.

Once we've received the responses from the Service Layer, the ResponsesReceived method will be called.  If something went wrong, the PublishRemoteException method of the Presenter base class will be called instead.  The ResponsesReceived method looks like this:

<script src="https://gist.github.com/3728058.js?file=s3.cs"></script>

We populate the UserGroupsBindingModel with the data that we've gotten back in the GetAllUserGroupsResponse and then we <em>tell the View</em> to expand its TreeView. We also <em>tell the View</em> to hide the AddNewButton if the user doesn't have the required permission to create new User Groups.  Obviously, we need to add these 2 method declarations to the IUserGroupsView:

<script src="https://gist.github.com/3728058.js?file=s4.cs"></script>

Allow me to expand on the notion of a Presenter telling a View to do something.  In many MVVM implementations, interaction from the ViewModel to the View is discouraged.  Many people even seem to go out of their way to avoid any direct interaction from the ViewModel to the View and try to make everything work through data binding.  While you can do that, there's no reason you really <em>need to</em>.  There's nothing wrong a little bit of code in the View, as long as that code is only concerned with true View concerns.  In this case, expanding a TreeView (which you can't do automatically in Silverlight... that's right, not even in the year 2010) or hiding a button seem like reasonable things to put in the View's code.  We'll get to the actual implementations of those methods later on.

At this point, we've got everything we need to retrieve the data we need to show, and to actually show it.  We're still not done though.  We still need to publish some events when a user selects a User Group, or when the user clicks on the AddNewButton:

<script src="https://gist.github.com/3728058.js?file=s5.cs"></script>

Pretty straightforward, no? We basically just publish an event and whatever else is listening for those events needs to (and will, as you'll see in the next post) react to it.

But we also still need to handle some events as well... more specifically, we need to handle the situation where a User Group is deleted, and where a User Group is changed.  There are 2 events that we need to handle:

<script src="https://gist.github.com/3728058.js?file=s6.cs"></script>

The UserGroupChangedEvent is used for both modified User Groups, or newly created ones.  I know, it'd be cleaner to have separate events for both situations but that would make my code in the Details UserControl uglier.  So I chose to go with one event for both situations.

Before we can handle these events, we need to modify the declaration of our UserGroupsPresenter class:

<script src="https://gist.github.com/3728058.js?file=s7.cs"></script>

Notice that we now explicitly declare that we are listening to these 2 events.  Before we will be notified of these events, we still need to subscribe with the Event Aggregator though.  So you'll need to add the following line of code to either the Initialize method, or the constructor:

<script src="https://gist.github.com/3728058.js?file=s8.cs"></script>

And now we can finally add the 2 event handlers:

<script src="https://gist.github.com/3728058.js?file=s9.cs"></script>

Again, pretty straightforward stuff. You'll notice that we've introduced a new method to the IUserGroupsView interface:

<script src="https://gist.github.com/3728068.js?file=s1.cs"></script>

And with that, our UserGroupsPresenter is done. All we need to do now, is the View.  Lets start with the XAML.  Gotta warn you though, I'm not good at XAML. I truly dislike it as a 'language' and I really hate having to edit XAML in Visual Studio which just seems much slower than it needs to be.  So, keep that in mind if you spot XAML that isn't quite up to standard ;)

<script src="https://gist.github.com/3728068.js?file=s2.xaml"></script>

Nothing special here.  The only thing that might surprise people here is that I'm using regular, old-school event handlers to deal with user events.  I don't really see the benefit in using the command pattern that so many MVVM users advocate.  In a lot of MVVM implementations, those commands simply delegate to methods on the ViewModel, essentially turning them in glorified method calls with the only benefit (though I'd argue it's not actually a benefit) that they are defined in XAML and that you don't need to put those method calls in the code-behind.  Again, there is no <em>real</em> problem with a little bit of code in the View and avoiding it at all costs is <em>simply not necessary</em>.  In other MVVM implementations, they contain actual logic but I prefer to have that stuff in the Presenter, especially since that logic typically requires state which you already have in the Presenter anyway.

The code-behind of this View looks like this:

<script src="https://gist.github.com/3728068.js?file=s3.cs"></script>

As you can see, apart from writing code to expand all items in the TreeView (again, I can't for the life of me understand why something like this isn't available out of the box in a UI framework but hey, I guess that's just me) there's nothing special going on here.  In fact, it's quite simple.  As is the rest of the code we wrote.