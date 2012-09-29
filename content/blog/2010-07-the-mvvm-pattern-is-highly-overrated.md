Update: check out my [MVP In Silverlight/WPF series](/blog/2010/08/mvp-in-silverlightwpf-series/) which discusses the MVP approach as an alternative to MVVM

If you're doing Silverlight or WPF, you've no doubt come across the MVVM (Model-View-ViewModel) pattern.  It seems to be the most popular client-side architecture pattern used among Silverlight/WPF developers.  I find the pattern to be highly overrated, and actually have some big issues with the whole thing. 

First, let's briefly cover what MVVM is about for those of you who don't know yet.  MVVM virtually eliminates all of the code that would typically be placed in the code-behind file of your View (a user control, a screen, whatever) by putting all of that logic in the ViewModel.  The ViewModel itself is never tightly coupled to the View. If it does have a reference to it, it's typically through an interface that the View implements instead of a reference of the actual type of the View.  This increases, or better yet, introduces testability to a large part of your UI code that you normally wouldn't be able to cover with unit tests if you'd go with the traditional "put it all in the code-behind"-approach.

The ViewModel typically contains properties for the data that is to be shown in the View, and also raises notification events when the data in those properties changes.  The View uses the powerful data-binding capabilities of Silverlight/WPF to bind the properties of the ViewModel to other user controls that the View is composed of.  User-events that are captured by the View are sent to the ViewModel through Commands.  Typically, these commands execute a method in the ViewModel which contains some kind of logic... usually to either send the updated data in the ViewModel's properties to the Model (usually a Service Layer in Silverlight, in WPF it could be either a true Domain Model or also a Service Layer), to perform some business logic in the Model, or to retrieve data from the Model so it can be placed in the ViewModel's properties so the View can bind to it.

That is, in a nutshell, how the MVVM pattern works.  So why do I have issues with this? You can develop and test most of the application's logic without being dependent on a physical View and that is typically a Good Thing, no? It sure is!  However, my problem with this approach is that  too many responsibilities are concentrated within the ViewModel.  Its main responsibilities are to facilitate databinding *and* to interact with the Model.  And that, in my opinion, isn't very clean.  In some ways, you could actually think of the ViewModel as a glorified code-behind, with the only difference being that it's not tightly coupled to the (physical) View.

In most (if not all) MVVM implementations, a ViewModel has many possible reasons to be changed.  It might need to be changed because of different data-binding requirements.  Then again, it might also require changes when a part of the Model is modified.  Now, I'm sure many of you can agree with me when I say that two of the most important principles in software development are [Seperation of Concerns (SoC)](http://en.wikipedia.org/wiki/Separation_of_concerns) and the [Single Responsibility Principle (SRP)](http://en.wikipedia.org/wiki/Single_responsibility_principle).  Obviously, the ViewModel doesn't fare well when it comes to both of these principles. 

Lets forget about MVVM for a second and focus on the concerns and responsibilities that a user control can have... say, a user control that shows customer information and allows the user to edit that data so it can be persisted:

- visualization of the actual control (its own layout as well drawing other user controls that it is composed of)
- communication/interaction with the Model
- making data (from the Model) available so it can be displayed
- outputting data in the correct user controls (for instance: various textboxes)
- (simple validation) of modified/inputted data (for instance: no string values for numeric fields, etc...)

Without MVVM, all of these would be taken care of in the View.  Obviously, not a good idea right?  After all, if it were a good idea, we'd never have had a reason to start using MVVM in the first place.

Now, with MVVM, a lot of people would divide these concerns and responsibilities like this:

View:

- visualization of the actual control (its own layout as well drawing other user controls that it is composed of)
- outputting data in the correct user controls (for instance: various textboxes)
- (simple) validation of modified/inputted data (for instance: no string values for numeric fields, etc...)

ViewModel:

- communication/interaction with the Model
- making data (from the Model) available so it can be displayed

In this case, the View still has 3 responsibilities which is still too much according to 'the guidelines', but it's not that big of a deal (though plenty of people would argue that the simple validation would be better placed in the ViewModel).  You're highly unlikely to actually want to write automated tests for pure visualization anyway and the SRP is not something that you absolutely need to follow to the extreme in every single place.  For the View, this is really not a problem and very much acceptable.

The ViewModel however has 2 important responsibilities in this case, and I'd argue that these 2 things should not be done in the same place.  Making data available is done through data-binding.  To do this, you need a set of properties and you need to raise the necessary events.  In most cases, raising those events is very straightforward, but in more complex controls you might need a bit of additional logic to determine which event should be raised at what point.  The other important responsibility is the communication/interaction with the Model.  In most Silverlight applications, the Model will be a Service Layer.  To communicate with this Service Layer, you need Service Proxies.  That means that your ViewModel is essentially responsible for communicating with the Service Layer, dealing with business exceptions that might be thrown by some service calls, and dealing with technical exceptions that can occur simply because of network-related problems.  Group all of those together and I don't think I'm going out on a limb here by saying that that is a lot of logic to put in *one* class, no?

(Sidenote: what I don't really understand is that many people who vigorously advocate adherence to SRP and SoC in their domain and business code don't seem to hold their UI code to the same standards. I do.)

At work, we do *a lot* of Silverlight development.  We typically have around 5 Silverlight projects in active development at the same time.  And it's been that way for over a year now.  That equals a lot of Silverlight code that we've written and experience and knowledge that we've built up.  And we haven't used MVVM for any of it.  All this time, we've been using the MVP pattern (Supervising Controller variant) with a slight twist.  That twist being a slimmed down version of a [Presentation Model](http://www.martinfowler.com/eaaDev/PresentationModel.html).  Our Presentation Model's sole responsibility is to facilitate data-binding, and in some cases, a touch of validation is added as well.

If we go back to our previous example of the customer screen, the responsibilities and concerns would be divided like this in our MVP-PMlight approach:

View:

- visualization of the actual control (its own layout as well drawing other user controls that it is composed of)
- outputting data in the correct user controls (for instance: various textboxes)
- (simple) validation of modified/inputted data (for instance: no string values for numeric fields, etc...)

Presenter:

- communication/interaction with the Model based on the contents of the Presentation Model

Presentation Model: 

- making data (from the Model) available so it can be displayed

Which leads to classes which are more focused on their task instead of trying to focus on too many things at the same time.  In my opinion, this approach is much better/cleaner than MVVM.  Not only is there a noticeable benefit in code quality (classes are more focused), there is also increased potential to reuse our 'light Presentation Models' in multiple controls.  Testability is increased over MVVM as well since it's always easier to test classes which are focused versus testing classes which have too many responsibilities.  All in all, a couple of important benefits and we still haven't thought of a real drawback compared to MVVM.

Update: check out my [MVP In Silverlight/WPF series](/blog/2010/08/mvp-in-silverlightwpf-series/) which discusses the MVP approach as an alternative to MVVM
