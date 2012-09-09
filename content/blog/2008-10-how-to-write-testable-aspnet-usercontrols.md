Ever since i wrote my <a href="http://davybrion.com/blog/2008/07/how-to-write-testable-aspnet-webforms/">How To Write Testable ASP.NET WebForms</a> post i've had people asking me how to make it work with UserControls.  I pretty much avoided UserControls with this approach for as long as i could, but for our current project we really had a need for it.  So i started implementing this together with a coworker, and this is the solution we came up with.

Note: if you haven't read that post on how to write testable ASP.NET WebForms, be sure to read it first because this approach is very similar and i won't repeat all of the general concepts of the approach here.

In the implementation for WebForms, we call the pages Views, and they all implement their own interface which inherits from our own IView interface. Now we wanted something that would work both with UserControls and Web Parts.  So we figured we should call both of them ViewParts and we have the following base interface that each ViewPart should implement:

<script src="https://gist.github.com/3684062.js?file=s1.cs"></script>

If you need more operations that each ViewPart should be able to offer, you can obviously just add whatever you want to this interface.

We also want each ViewPart to have its own Controller, which we call PartControllers.  Since we want to be able to test both the ViewParts and their containing Views in isolation, each View's Controller can never communicate directly with the specific PartController(s) of the ViewPart(s) that it contains. So we first need the following base interface for each PartController:

<script src="https://gist.github.com/3684062.js?file=s2.cs"></script>

In our case, each PartController will need an IDispatcher instance to be able to communicate with our <a href="http://davybrion.com/blog/2008/07/the-request-response-service-layer/">Request/Response Service Layer</a>.  So the IDispatcher reference is only necessary if you're using that as well.  The idea is that the containing View's Controller needs to provide its IDispatcher instance to each contained ViewPart's PartController so the initial requests of each ViewPart can be sent to the service layer together with whatever initial requests the containing View's Controller needs to send when the View is loaded.  That's basically what the AddInitialRequests and GetInitialResponses methods are for. Again, this is very specific to the usage of the Request/Response Service Layer, so you might want to put some entirely different basic operations in your base IPartController interface.

We need to be able to ask each ViewPart for its typed PartController, so we also have this interface:

<script src="https://gist.github.com/3684062.js?file=s3.cs"></script>

We also need to be able to ask each PartController for a typed instance of its ViewPart, so we also have the following interface:

<script src="https://gist.github.com/3684062.js?file=s4.cs"></script>

And then our PartController base class looks like this:

<script src="https://gist.github.com/3684062.js?file=s5.cs"></script>

Now, when we want to write UserControls that can work with this approach, we need to inherit from the following UserControl base class:

<script src="https://gist.github.com/3684062.js?file=s6.cs"></script>

When the UserControl is constructed, we retrieve an instance of the specific PartController through the IOC container, and we pass the newly created instance of our UserControl as the ViewPart dependency of the PartController.

What you've seen so far is all very abstract, so let's go over a small example.  Suppose we have a View (DummyPage) which contains a ViewPart (DummyPart).  First, let's create the DummyPart:

<script src="https://gist.github.com/3684062.js?file=s7.cs"></script>

Our DummyPart (the ViewPart) inherits from our UserControl base class and passes the interface type of our PartController as the type parameter of the UserControl.  It also implements the IDummyViewPart interface (which is empty in this simple example):

<script src="https://gist.github.com/3684062.js?file=s8.cs"></script>

The IDummyViewPartController interface looks like this:

<script src="https://gist.github.com/3684062.js?file=s9.cs"></script>

The implementation of the DummyPartController looks like this:

<script src="https://gist.github.com/3684070.js?file=s1.cs"></script>

So what do we have now? A reusable UserControl which has its own controller where the actual logic of the UserControl will be implemented.  We can write unit tests for all of the logic that the UserControl needs to have.  We can also reuse this UserControl in a Page, and do so in a manner which enables us to fake the implementation of the UserControl for the tests we'll write for the logic in that Page.

Suppose we have a DummyPage which contains the DummyPart UserControl. Our DummyPage implements the following interface:

<script src="https://gist.github.com/3684070.js?file=s2.cs"></script>

The code of our actual DummyPage looks like this:

<script src="https://gist.github.com/3684070.js?file=s3.cs"></script>

And the code of the DummyController looks like this:

<script src="https://gist.github.com/3684070.js?file=s4.cs"></script>

In the Load method of the DummyController, we retrieve the DummyPartController and we can communicate with it.  And since we're talking to an interface type, we can easily provide a mocked IDummyPartController instance for our unit tests.

This approach makes it possible to create UserControls which you can easily write unit tests for, and you can reuse the UserControls in containing pages while remaining the flexibility to write unit tests for those containing pages without being dependent on the actual implementation of the UserControl.