Note: This post is part of a series. You can find the introduction and overview of the series <a href="/blog/2010/08/mvp-in-silverlightwpf-series">here</a>.

The second (and last) UserControl of this series and its accompanying sample looks like this:

<a href="/postcontent/sample_details1.png"><img src="/postcontent/sample_details1.png" alt="" title="sample_details" width="503" height="92" class="aligncenter size-full wp-image-2463" /></a>

Nothing fancy (it never is when I do the UI) and pretty much a typical edit screen, though there are very few fields to edit obviously. The DropDown shows the suitable parent User Groups for this User Group.  These suitable parents are retrieved from the Service Layer and the 'logic' behind them is very simple: it can't be the selected User Group, and it can't be any User Group that is currently below it in the hierarchy.  Other than that, anything goes. 

The 3 buttons should be self-explanatory as well. If you press the Cancel button, the TextBox and the DropDown should be reset to their initial values, which are either empty values in case the user is creating a new User Group, or the original values of the currently selected User Group in our previous UserControl's TreeView.  If you press the Delete button, the currently selected User Group needs to be deleted.  The Delete button can obviously only be shown in case we're editing an existing User Group, and never if we're creating a new one since that wouldn't make sense.  The Save button persists the changes, which means either updating the currently selected User Group or inserting the newly created one.  The Delete button can't be shown when the user does not have the required permission to delete a User Group, and the Save button can't be shown if the user doesn't have the required permission to edit a User Group.  However, if the user does have permission to create a new User Group (which is a separate permission from editing an existing one) then the Save button must be visible.

Alright, let's get started.  I'm going to use a different style in this post than I used in the last one though.  The last post was more of a step-by-step walk through of writing the actual code, but that leads to very long posts (and takes up a lot more of my time to write it), and I'd like to keep this one a bit shorter.  So I'm just going to show the entire code of each class with some comments on it. 

As usual I like to start off with the BindingModel.  What exactly are we going to put into it? We'll obviously need some stuff from the User Group: its ID (though we won't display that), name and the parent User Group (if there is one).  We'll also need a list of suitable parents.  Remember that we also need to support the Cancel button, so we need to store the original values.  This is what I came up with:

<script src="https://gist.github.com/3728097.js?file=s1.cs"></script>

Looking back on this now, there are a couple of things that I don't really use.  For one, the ID property raises the PropertyChanged event even though there's nothing that binds to it.  I also have an IsExistingUserGroup property but I don't use it anywhere.  Unfortunately, I only noticed this after releasing the sample so I'm not just gonna go back and change it now.  Probably just a brainfart on my part. Anyways, the only interesting parts to note about this BindingModel is the simple validation that we define on the Name property (and which was actually already discussed in the post about the infrastructure bits) and the fact that we add a default 'empty' User Group to the SuitableParentUserGroups collection.  Other than that, everything here should be very clear and straightforward by now so let's just move on to the presenter already:

<script src="https://gist.github.com/3728097.js?file=s2.cs"></script>

As you can see, this presenter doesn't retrieve any data in its Initialize method.  In fact, it just hides the View and subscribes with the Event Aggregator.  This UserControl only needs to be visible once the user has selected a User Group in the Overview UserControl, so the View remains hidden until we actually need to show something.  

In the Handle(UserGroupNeedsToBeCreatedEvent) method, we first instruct the View to prevent the user from pressing the Delete button (since that wouldn't make sense during the creation of a new User Group) and we call the LoadData method.  The Handle(UserGroupSelectedEvent) method first instructs the view to enable everything (all controls basically) and then calls the LoadData method with the ID of the currently selected User Group.  If the userGroupId parameter is passed into the LoadData method, we'll not only retrieve the suitable parents, but also the details of the current User Group, as well as check whether our user has permission to delete and/or edit a User Group.  And obviously, being the responsible programmers that we are, we send all 3 requests in the same roundtrip since there is no reason whatsoever not to do so.

In the ResponsesReceived method, we populate the model based on the data we've received from the Service Layer.  We also tell the View to prevent deletion of the current User Group if the user doesn't have permisson to do so, and we also tell the View to prevent modification if necessary.  Finally, we tell the View to show itself to the user.

The PersistChanges method is the one that will be called by the View when the Save button is clicked.  If the BindingModel has validation errors, we simply return from the method.  Since we use the INotifyDataErrorInfo interface in our BindingModel (as discussed in the Infrastructure Bits post), the View will automatically show the validation message anyway and we don't need to do anything.  We could have also bound the Visibility property of the Save button to the HasErrors property of the BindingModel to prevent it from being visible as long as there are validation problems, but then we'd also need to keep the permissions into account.  You could do it in various ways, and I just didn't go through the extra effort of actually doing so since this is after all just a silly sample.  Anyways, if there are no validation errors, we send a request to the Service Layer to save the User Group's data.

In the PersistChanges_ResponsesReceived method, we update the Id property of the BindingModel if necessary, and we publish a UserGroupChangedEvent.  As you've seen in the last post, that event will be handled by the Overview UserControl so it can update its TreeView.  As you can see, the Delete method is pretty similar, so there's no need to explain it.  And finally, the Cancel method simply calls the RevertToOriginalValues method on the BindingModel.

Now that we have our BindingModel and our Presenter, we can start working on our View.  The XAML looks like this (again, I suck at XAML so this is probaby far from good XAML... if there is such a thing, that is):

<script src="https://gist.github.com/3728097.js?file=s3.xaml"></script>

And the View's code would be this:

<script src="https://gist.github.com/3728097.js?file=s4.cs"></script>

And that's it.

I apologize to those of you who prefer the style of the previous post, but I'm sort of behind schedule and won't be able to write anything for the next 4 days, so I'm trying to get ahead enough of the posting schedule ;).  Though I hope you'll agree that the walk-through style of the previous post wasn't necessary anymore after going through a full implementation once.

Anyways, in the next post of the series, we'll look into the automated tests of both the BindingModel and the Presenters.