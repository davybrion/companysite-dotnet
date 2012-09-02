Note: This post is part of a series. You can find the introduction and overview of the series <a href="http://davybrion.com/blog/2010/08/mvp-in-silverlightwpf-series">here</a>.

The second (and last) UserControl of this series and its accompanying sample looks like this:

<a href="http://davybrion.com/blog/wp-content/uploads/2010/08/sample_details1.png"><img src="http://davybrion.com/blog/wp-content/uploads/2010/08/sample_details1.png" alt="" title="sample_details" width="503" height="92" class="aligncenter size-full wp-image-2463" /></a>

Nothing fancy (it never is when i do the UI) and pretty much a typical edit screen, though there are very few fields to edit obviously. The DropDown shows the suitable parent User Groups for this User Group.  These suitable parents are retrieved from the Service Layer and the 'logic' behind them is very simple: it can't be the selected User Group, and it can't be any User Group that is currently below it in the hierarchy.  Other than that, anything goes. 

The 3 buttons should be self-explanatory as well. If you press the Cancel button, the TextBox and the DropDown should be reset to their initial values, which are either empty values in case the user is creating a new User Group, or the original values of the currently selected User Group in our previous UserControl's TreeView.  If you press the Delete button, the currently selected User Group needs to be deleted.  The Delete button can obviously only be shown in case we're editing an existing User Group, and never if we're creating a new one since that wouldn't make sense.  The Save button persists the changes, which means either updating the currently selected User Group or inserting the newly created one.  The Delete button can't be shown when the user does not have the required permission to delete a User Group, and the Save button can't be shown if the user doesn't have the required permission to edit a User Group.  However, if the user does have permission to create a new User Group (which is a separate permission from editing an existing one) then the Save button must be visible.

Alright, let's get started.  I'm going to use a different style in this post than i used in the last one though.  The last post was more of a step-by-step walk through of writing the actual code, but that leads to very long posts (and takes up a lot more of my time to write it), and i'd like to keep this one a bit shorter.  So i'm just going to show the entire code of each class with some comments on it. 

As usual i like to start off with the BindingModel.  What exactly are we going to put into it? We'll obviously need some stuff from the User Group: its ID (though we won't display that), name and the parent User Group (if there is one).  We'll also need a list of suitable parents.  Remember that we also need to support the Cancel button, so we need to store the original values.  This is what i came up with:

<div>
[csharp]
    public class UserGroupDetailBindingModel : BindingModel&lt;UserGroupDetailBindingModel&gt;
    {
        private string originalName;
        private Guid? originalId;
        private UserGroupDto originalSelectedParent;
 
        public ObservableCollection&lt;UserGroupDto&gt; SuitableParentUserGroups { get; private set; }
 
        private UserGroupDto selectedParentUserGroup;
 
        public UserGroupDto SelectedParentUserGroup
        {
            get { return selectedParentUserGroup; }
            set
            {
                selectedParentUserGroup = value;
                NotifyPropertyChanged(m =&gt; m.SelectedParentUserGroup);
            }
        }
 
        private Guid? id;
 
        public Guid? Id
        {
            get { return id; }
            set
            {
                id = value;
                NotifyPropertyChanged(m =&gt; m.Id);
                NotifyPropertyChanged(m =&gt; m.IsExistingUserGroup);
            }
        }
 
        public bool IsExistingUserGroup { get { return id.HasValue &amp;&amp; id.Value != Guid.Empty; } }
 
        private string name;
 
        public string Name
        {
            get { return name; }
            set
            {
                name = value;
                NotifyPropertyChanged(m =&gt; m.Name);
            }
        }
 
        public UserGroupDetailBindingModel()
        {
            SuitableParentUserGroups = new ObservableCollection&lt;UserGroupDto&gt;();
            Clear();
            AddValidationFor(m =&gt; m.Name)
                .When(m =&gt; string.IsNullOrWhiteSpace(m.name))
                .WithMessage(&quot;name is a required field&quot;);
        }
 
        public void Clear()
        {
            SuitableParentUserGroups.Clear();
            SuitableParentUserGroups.Add(new UserGroupDto { Id = Guid.Empty, Name = &quot;None&quot; });
            SelectedParentUserGroup = SuitableParentUserGroups[0];
 
            originalId = Id = null;
            originalName = Name = null;
            originalSelectedParent = SelectedParentUserGroup;
        }
 
        public void Populate(IEnumerable&lt;UserGroupDto&gt; suitableParentUserGroups, UserGroupDto currentUserGroup = null)
        {
            foreach (var suitableParentUserGroup in suitableParentUserGroups)
            {
                SuitableParentUserGroups.Add(suitableParentUserGroup);
            }
 
            if (currentUserGroup != null)
            {
                originalName = Name = currentUserGroup.Name;
                originalId = Id = currentUserGroup.Id;
                originalSelectedParent = SelectedParentUserGroup;
 
                if (currentUserGroup.ParentId.HasValue)
                {
                    originalSelectedParent = SelectedParentUserGroup = SuitableParentUserGroups.First(u =&gt; u.Id == currentUserGroup.ParentId);
                }
            }
        }
 
        public void RevertToOriginalValues()
        {
            Name = originalName;
            Id = originalId;
            SelectedParentUserGroup = originalSelectedParent;
        }
    }
[/csharp]
</div>

Looking back on this now, there are a couple of things that i don't really use.  For one, the ID property raises the PropertyChanged event even though there's nothing that binds to it.  I also have an IsExistingUserGroup property but i don't use it anywhere.  Unfortunately, i only noticed this after releasing the sample so i'm not just gonna go back and change it now.  Probably just a brainfart on my part. Anyways, the only interesting parts to note about this BindingModel is the simple validation that we define on the Name property (and which was actually already discussed in the post about the infrastructure bits) and the fact that we add a default 'empty' User Group to the SuitableParentUserGroups collection.  Other than that, everything here should be very clear and straightforward by now so let's just move on to the presenter already:

<div>
[csharp]
    public class UserGroupDetailPresenter : Presenter&lt;IUserGroupDetailsView, UserGroupDetailBindingModel&gt;,
        IListenTo&lt;UserGroupSelectedEvent&gt;, IListenTo&lt;UserGroupNeedsToBeCreatedEvent&gt;
    {
        public UserGroupDetailPresenter(IUserGroupDetailsView view, IEventAggregator eventAggregator, IAsyncRequestDispatcherFactory requestDispatcherFactory)
            : base(view, eventAggregator, requestDispatcherFactory) {}
 
        public override void Initialize()
        {
            View.Hide();
            EventAggregator.Subscribe(this);
        }
 
        public void Handle(UserGroupNeedsToBeCreatedEvent receivedEvent)
        {
            View.PreventDeletion();
            LoadData();
        }
 
        public void Handle(UserGroupSelectedEvent receivedEvent)
        {
            View.EnableEverything();
            LoadData(receivedEvent.SelectedUserGroupId);
        }
 
        private void LoadData(Guid? userGroupId = null)
        {
            BindingModel.Clear();
 
            var requestDispatcher = RequestDispatcherFactory.CreateAsyncRequestDispatcher();
 
            if (userGroupId.HasValue)
            {
                requestDispatcher.Add(new CheckPermissionsRequest {PermissionsToCheck = new[] {Permissions.DeleteUserGroup, Permissions.EditUserGroup}});
                requestDispatcher.Add(new GetUserGroupRequest { UserGroupId = userGroupId.Value });
            }
            requestDispatcher.Add(new GetSuitableParentUserGroupsRequest {UserGroupId = userGroupId});
            requestDispatcher.ProcessRequests(ResponsesReceived, PublishRemoteException);
        }
 
        private void ResponsesReceived(ReceivedResponses receivedResponses)
        {
            if (receivedResponses.HasResponse&lt;GetUserGroupResponse&gt;())
            {
                BindingModel.Populate(receivedResponses.Get&lt;GetSuitableParentUserGroupsResponse&gt;().SuitableParentUserGroups,
                    receivedResponses.Get&lt;GetUserGroupResponse&gt;().UserGroup);
            }
            else
            {
                BindingModel.Populate(receivedResponses.Get&lt;GetSuitableParentUserGroupsResponse&gt;().SuitableParentUserGroups);
            }
 
            if (receivedResponses.HasResponse&lt;CheckPermissionsResponse&gt;())
            {
                var response = receivedResponses.Get&lt;CheckPermissionsResponse&gt;();
                if (!response.AuthorizationResults[Permissions.DeleteUserGroup]) View.PreventDeletion();
                if (!response.AuthorizationResults[Permissions.EditUserGroup]) View.PreventModification();
            }
 
            View.Show();
        }
 
        public void PersistChanges()
        {
            BindingModel.ValidateAll();
            if (BindingModel.HasErrors) return;
 
            var dispatcher = RequestDispatcherFactory.CreateAsyncRequestDispatcher();
            dispatcher.Add(new SaveUserGroupRequest
            {
                Id = BindingModel.Id,
                Name = BindingModel.Name,
                ParentId = BindingModel.SelectedParentUserGroup.Id != Guid.Empty ? BindingModel.SelectedParentUserGroup.Id : (Guid?)null
            });
            dispatcher.ProcessRequests(PersistChanges_ResponseReceived, PublishRemoteException);
        }
 
        private void PersistChanges_ResponseReceived(ReceivedResponses responses)
        {
            var response = responses.Get&lt;SaveUserGroupResponse&gt;();
 
            if (response.NewUserGroupId.HasValue)
            {
                BindingModel.Id = response.NewUserGroupId.Value;
            }
 
            EventAggregator.Publish(new UserGroupChangedEvent
            {
                Id = BindingModel.Id.Value,
                Name = BindingModel.Name,
                ParentId = BindingModel.SelectedParentUserGroup.Id != Guid.Empty ? BindingModel.SelectedParentUserGroup.Id : (Guid?)null,
                IsNew = response.NewUserGroupId.HasValue
            });
        }
 
        public void Delete()
        {
            var dispatcher = RequestDispatcherFactory.CreateAsyncRequestDispatcher();
            dispatcher.Add(new DeleteUserGroupRequest { UserGroupId = BindingModel.Id.Value });
            dispatcher.ProcessRequests(DeleteUserGroup_ResponseReceived, PublishRemoteException);
        }
 
        private void DeleteUserGroup_ResponseReceived(ReceivedResponses responses)
        {
            EventAggregator.Publish(new UserGroupDeletedEvent(BindingModel.Id.Value));
        }
 
        public void Cancel()
        {
            BindingModel.RevertToOriginalValues();
        }
    }
[/csharp]
</div>

As you can see, this presenter doesn't retrieve any data in its Initialize method.  In fact, it just hides the View and subscribes with the Event Aggregator.  This UserControl only needs to be visible once the user has selected a User Group in the Overview UserControl, so the View remains hidden until we actually need to show something.  

In the Handle(UserGroupNeedsToBeCreatedEvent) method, we first instruct the View to prevent the user from pressing the Delete button (since that wouldn't make sense during the creation of a new User Group) and we call the LoadData method.  The Handle(UserGroupSelectedEvent) method first instructs the view to enable everything (all controls basically) and then calls the LoadData method with the ID of the currently selected User Group.  If the userGroupId parameter is passed into the LoadData method, we'll not only retrieve the suitable parents, but also the details of the current User Group, as well as check whether our user has permission to delete and/or edit a User Group.  And obviously, being the responsible programmers that we are, we send all 3 requests in the same roundtrip since there is no reason whatsoever not to do so.

In the ResponsesReceived method, we populate the model based on the data we've received from the Service Layer.  We also tell the View to prevent deletion of the current User Group if the user doesn't have permisson to do so, and we also tell the View to prevent modification if necessary.  Finally, we tell the View to show itself to the user.

The PersistChanges method is the one that will be called by the View when the Save button is clicked.  If the BindingModel has validation errors, we simply return from the method.  Since we use the INotifyDataErrorInfo interface in our BindingModel (as discussed in the Infrastructure Bits post), the View will automatically show the validation message anyway and we don't need to do anything.  We could have also bound the Visibility property of the Save button to the HasErrors property of the BindingModel to prevent it from being visible as long as there are validation problems, but then we'd also need to keep the permissions into account.  You could do it in various ways, and i just didn't go through the extra effort of actually doing so since this is after all just a silly sample.  Anyways, if there are no validation errors, we send a request to the Service Layer to save the User Group's data.

In the PersistChanges_ResponsesReceived method, we update the Id property of the BindingModel if necessary, and we publish a UserGroupChangedEvent.  As you've seen in the last post, that event will be handled by the Overview UserControl so it can update its TreeView.  As you can see, the Delete method is pretty similar, so there's no need to explain it.  And finally, the Cancel method simply calls the RevertToOriginalValues method on the BindingModel.

Now that we have our BindingModel and our Presenter, we can start working on our View.  The XAML looks like this (again, i suck at XAML so this is probaby far from good XAML... if there is such a thing, that is):

<div>
[xml]
&lt;MVP:View x:Class=&quot;SilverlightMVP.Client.Views.UserGroupDetail&quot;
   xmlns=&quot;http://schemas.microsoft.com/winfx/2006/xaml/presentation&quot;
   xmlns:x=&quot;http://schemas.microsoft.com/winfx/2006/xaml&quot;
   xmlns:MVP=&quot;clr-namespace:SilverlightMVP.Client.Infrastructure.MVP&quot; &gt;
 
    &lt;Grid x:Name=&quot;LayoutRoot&quot; Background=&quot;White&quot; MinHeight=&quot;75&quot; MaxHeight=&quot;75&quot; MinWidth=&quot;455&quot; &gt;
 
        &lt;Grid.ColumnDefinitions&gt;
            &lt;ColumnDefinition /&gt;
            &lt;ColumnDefinition /&gt;
            &lt;ColumnDefinition /&gt;
        &lt;/Grid.ColumnDefinitions&gt;
 
        &lt;Grid.RowDefinitions&gt;
            &lt;RowDefinition /&gt;
            &lt;RowDefinition /&gt;
            &lt;RowDefinition /&gt;
        &lt;/Grid.RowDefinitions&gt;
 
        &lt;TextBlock Text=&quot;Name&quot; Grid.Column=&quot;0&quot; Grid.Row=&quot;0&quot; /&gt;
        &lt;TextBox x:Name=&quot;NameTextBox&quot; Text=&quot;{Binding Path=Name, Mode=TwoWay, ValidatesOnExceptions=True, NotifyOnValidationError=True}&quot;
                Grid.Column=&quot;1&quot; Grid.Row=&quot;0&quot; Grid.ColumnSpan=&quot;2&quot; /&gt;
 
        &lt;TextBlock Text=&quot;Parent&quot; Grid.Column=&quot;0&quot; Grid.Row=&quot;1&quot; /&gt;
        &lt;ComboBox x:Name=&quot;SuitableParentUserGroupsComboBox&quot; ItemsSource=&quot;{Binding Path=SuitableParentUserGroups}&quot; MinWidth=&quot;150&quot;
                 DisplayMemberPath=&quot;Name&quot; SelectedItem=&quot;{Binding Path=SelectedParentUserGroup, Mode=TwoWay}&quot;
                 Grid.Column=&quot;1&quot; Grid.Row=&quot;1&quot; Grid.ColumnSpan=&quot;2&quot; /&gt;
 
        &lt;Button x:Name=&quot;DeleteButton&quot; Content=&quot;Delete&quot; Click=&quot;DeleteButton_Click&quot; Grid.Column=&quot;0&quot; Grid.Row=&quot;2&quot; /&gt;
        &lt;Button x:Name=&quot;CancelButton&quot; Content=&quot;Cancel&quot; Click=&quot;CancelButton_Click&quot; Grid.Column=&quot;1&quot; Grid.Row=&quot;2&quot; /&gt;
        &lt;Button x:Name=&quot;SaveButton&quot; Content=&quot;Save&quot; Click=&quot;SaveButton_Click&quot; Grid.Column=&quot;2&quot; Grid.Row=&quot;2&quot; /&gt;
 
    &lt;/Grid&gt;
&lt;/MVP:View&gt;
[/xml]
</div>

And the View's code would be this:

<div>
[csharp]
    public interface IUserGroupDetailsView : IView
    {
        void PreventDeletion();
        void PreventModification();
        void EnableEverything();
    }
 
    public partial class UserGroupDetail : IUserGroupDetailsView
    {
        private readonly UserGroupDetailPresenter presenter;
 
        public UserGroupDetail()
        {
            InitializeComponent();
            presenter = CreateAndInitializePresenter&lt;UserGroupDetailPresenter&gt;();
        }
 
        public void EnableEverything()
        {
            DeleteButton.Visibility = Visibility.Visible;
            CancelButton.Visibility = Visibility.Visible;
            SaveButton.Visibility = Visibility.Visible;
            NameTextBox.IsEnabled = true;
            SuitableParentUserGroupsComboBox.IsEnabled = true;
        }
 
        public void PreventDeletion()
        {
            DeleteButton.Visibility = Visibility.Collapsed;
        }
 
        public void PreventModification()
        {
            NameTextBox.IsEnabled = false;
            SuitableParentUserGroupsComboBox.IsEnabled = false;
            CancelButton.Visibility = Visibility.Collapsed;
            SaveButton.Visibility = Visibility.Collapsed;
        }
 
        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            presenter.Delete();
        }
 
        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            presenter.Cancel();
        }
 
        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            presenter.PersistChanges();
        }
    }
[/csharp]
</div>

And that's it.

I apologize to those of you who prefer the style of the previous post, but i'm sort of behind schedule and won't be able to write anything for the next 4 days, so i'm trying to get ahead enough of the posting schedule ;).  Though i hope you'll agree that the walk-through style of the previous post wasn't necessary anymore after going through a full implementation once.

Anyways, in the next post of the series, we'll look into the automated tests of both the BindingModel and the Presenters.