Note: This post is part of a series. You can find the introduction and overview of the series <a href="/blog/2010/08/mvp-in-silverlightwpf-series">here</a>.

In the previous post, we discussed the different parts this client-side architecture is composed of.  Before we can start using these parts effectively in our code, we'll need some infrastructural base classes that we can inherit from. In this post, we'll cover base classes for the View, the Presenter and the BindingModel.  I'm also going to briefly discuss how various user controls can communicate with each other without being directly coupled to each other, and how we'll communicate with the Model (which is a Service Layer in this series and its accompanying sample).

Let's start off with the BindingModel. In the previous post of this series, i said that the BindingModel is supposed to facilitate data binding.  So what exactly does that mean? Well, that means that the BindingModel is responsible for everything that's required to take full advantage of Silverlight/WPF's data binding features.  In the case of Silverlight, that means at least implementing INotifyPropertyChanged and raising the necessary events at the right time, and if you want to use client-side validation that works together with the data binding features, the INotifyDataErrorInfo interface. Note: this interface is not available yet in WPF but you can use the IDataErrorInfo interface to do something similar (though far less elegantly).

In our case, the BindingModel implements both INotifyPropertyChanged and INotifyDataErrorInfo.  Tom Ceulemans (who still doesn't have a blog i can link to, which is too bad because he'd put many Silverlight 'experts' and book authors to shame), a coworker of mine, recently wrote something that makes it incredibly easy to elegantly use simple client-side validation in combination with data binding.  So before i can show you the base BindingModel class, i need to show a small utility class that will be used by our BindingModel:

<script src="https://gist.github.com/3727959.js?file=s1.cs"></script>

This class basically makes it possible to define the validation for a property of your BindingModel with a nice fluent syntax.  I'll show you an example of this, right after i show you the BindingModel base class in its entirety:

<script src="https://gist.github.com/3727959.js?file=s2.cs"></script>

That's quite a bit huh? It's not really all that much though.  Here's what this class does:
<ul>
	<li>gives us a type-safe way of raising the PropertyChanged event based on an expression</li>
	<li>when a PropertyChanged event is raised, we automatically validate that property if a validation for that property has been defined</li>
	<li>provides a very easy way to define validations for properties</li>
	<li>enables you to validate all properties (for which validations have been defined), or just a specific one</li>
	<li>takes full advantage of the INotifyDataErrorInfo functionality of Silverlight which means that as soon as a property doesn't validate, the UI will show this to the user <em>automatically</em> without us having to do anything for it</li>
</ul>

For instance, suppose we have a Name property that is bound to a TextBox using two-way binding.  We can define the validation of the Name property like this:

<script src="https://gist.github.com/3727959.js?file=s3.cs"></script>

And the Name property would be implemented like this:

<script src="https://gist.github.com/3727959.js?file=s4.cs"></script>

That's it.  If the user (or you) puts an invalid value in the Name property, the InputControl that is bound to it will automatically display the validation message.  If the user (or you) puts a valid value in the Name property, its validation message will automatically be removed by the InputControl.  You'll see some extensive examples of BindingModels once we explore how to build our User Controls in this series.

Next up is the Presenter.  As i mentioned in the previous post, this class is a bit of a coordinator between the user, the View and the Model.  That means it interacts with both the View and the Model.  It can interact with its View directly, which is still done through an interface to enable easy testability, or with <em>other</em> Views' Presenter indirectly.  For this indirect communication between Presenters, we use Event Aggregation through an Event Aggregator.  I've covered this before, so i'm just going to link to <a href="/blog/2009/10/event-aggregation/">my old post about it</a>.  In the sample that accompanies this series, you'll find the code for the Event Aggregator and since it's also shown in the post i just linked to, there's no need to show it in this post again.  Just keep in mind that each Presenter will automatically have a reference to the Event Aggregator.

Another thing that each Presenter needs to have is a reference to a factory which creates proxies to communicate with the Model (Service Layer).  I'm using <a href="http://code.google.com/p/agatha-rrsl/">Agatha</a>'s IAsyncRequestDispatcherFactory for this since i believe it's the most optimal and clean way to communicate with a WCF Service Layer, especially when doing Silverlight, provided of course that the Service Layer is also implemented with Agatha.  Now, since i'm the author of the Agatha project i'm obviously a bit biased but check out the Agatha project and decide for yourself whether this is a good choice or not.

Having said all that, we can now take a look at the implementation of the base Presenter class:

<script src="https://gist.github.com/3727959.js?file=s5.cs"></script>

As you can see, there's not really much to say about this.  The Presenter has its View, IEventAggregator and IAsyncRequestDispatcherFactory dependencies injected in the constructor and it automatically instantiates its BindingModel.  The dependencies are made available to the derived classes (your actual Presenters) through protected properties.  The Initialize method is something that you can optionally implement and will be called by the View when it has received its reference to the Presenter.  There's also a protected utility method called PublishRemoteException which publishes a RemoteExceptionOccurredEvent through the Event Aggregator.  This method can be used (but doesn't have to be) as the fallback whenever a service call fails.  The idea is that something else is subscribed to the RemoteExceptionOccurredEvent with the Event Aggregator and that it can display these error messages in a uniform manner (or log them, or whatever).  That's it as far as the base Presenter implementation goes.

Finally, we get to the View.  We have the following base IView interface which the View class will implement:

<script src="https://gist.github.com/3727959.js?file=s6.cs"></script>

As you can see, the IView interface inherits from the IDisposable interface.  Many people think i go overboard with using the IDisposable interface, but i still think it's very important to deal with .NET's memory management efficiently, and especially in the case of Silverlight where we have observed numerous memory leaks in core UserControls due to, well, bad programming practices.

In this particular implementation of MVP, you are required to resolve the Presenter manually in your View code.  I really would've preferred that this would be done automatically, but a Silverlight User Control can't inherit from a generic base class (where the generic parameter would be the type of the Presenter) because that introduces problems with both the Visual Studio designer as well as Blend.  Another possibility would've been for the IOC container to instantiate the View, pass it to the Presenter since it's a constructor dependency and then let the Presenter pass itself to the View, but then you lose the ability to simply use your User Controls in the XAML of <em>other</em> User Controls.  So for those reasons, the View is not instantiated by the IOC container, and you are required to resolve your own Presenter reference in your View.  I know it's not exactly a best practice when it comes to Dependency Injection and using an Inversion Of Control container, but then again, we shouldn't sacrifice usability in order to achieve a theoretically technically cleaner solution either.  So, without further ado, here's the base View class:

<script src="https://gist.github.com/3727959.js?file=s7.cs"></script>

First of all, you might notice that i'm storing a dynamic reference to the Presenter instead of a typed reference.  I can't use a typed reference because i have no way to define the actual Presenter type parameter without causing problems with either Visual Studio's XAML designer, or Blend.  Now, i could've introduced a non-generic base Presenter class which defines the Initialize method and a non-typed BindingModel property.  But then i'd just be introducing more code solely for the purpose of paying lip service to the compiler.  If you have a problem with the usage of the dynamic keyword, feel free to waste some time adding code that's not going to offer you any real benefit whatsoever.

As you can see, the CreateAndInitializePresenter method will resolve the Presenter through the IOC container (Castle Windsor in this sample) and i'm using a feature of the container which allows me to provide one of the dependencies myself (in this case, the View instance). There's another way to pass in the instance of the View with an anonymous type, but that didn't work with Windsor in Silverlight.  After the Presenter has been resolved through the container, we call its Initialize method and we set the DataContext of our UserControl to the BindingModel instance of the Presenter.  In the (naive) Dispose implementation, we release the Presenter instance through the container and we'll go through all of the children of this User Control in the Visual Tree to dispose the ones that implement IDisposable (for instance, embedded UserControls where you're also using this MVP approach) and we also manually stop every ProgressBar control that we might have.  The only reason we do that is because there is <a href="/blog/2009/02/silverlights-progressbar-and-possible-memory-leaks/">a memory leak in the ProgressBar control</a> which may have been fixed in Silverlight 4, but i haven't checked if this is indeed the case.  

And that is it.  We have a base class for our BindingModels, we have a base class for our Presenters, and we have a base class for our Views.  We can now start using these parts together very easily.

In the next post, i'll describe the Sample project that accompanies this series (and yes, you'll finally be able to download it and check out all of the code on your own) and after that, we'll explore how we can develop User Controls with this approach.