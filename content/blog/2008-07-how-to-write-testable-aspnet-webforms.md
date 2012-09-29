Microsoft's upcoming ASP.NET MVC framework makes it easy to write tests for your application layer logic. But what about those of us who are stuck with ASP.NET WebForms?  You can still write highly testable ASP.NET WebForms with only a little bit of extra effort. But that extra effort really pays off in the long run.  In this post, I'll give a detailed description of one approach that has worked for me really well.  I first started using this approach for a project at work sometime last year. It allowed me to cover my application logic with a lot of tests which weren't a hassle to write or run, so naturally I've always wanted to write a detailed post on this subject.  I hope you'll like it :) (Btw, if you're new to mocking this might serve as an introduction to that as well)

This is the screen we're going to create:

<a href='/postcontent/searchproducts.png'><img src="/postcontent/searchproducts.png" alt="" title="searchproducts" width="500" height="243" class="alignleft size-full wp-image-161" /></a>

The very first thing you'll notice is that I completely suck at graphic design. So try to ignore the crappy look, and lets focus on what this screen should do. A user can perform a search on products based on the name, the product category and the supplier of the product. After clicking the Search button, the user is presented with a list of products that match the search criteria.  Next to each product is an Edit link (because I was too lazy to find a nice image for this). When clicked, the application should navigate to an Edit screen where the chosen product can be edited.  To keep this example short (this post will be long enough already!), that's all for this screen.

So what are the things that we would want to test for a simple screen like this? For starters, I want to be sure that when this page is loaded, it retrieves the list of product categories and suppliers, and that it displays them.  Another thing I want to test is that when the search button is clicked, the screen has to retrieve all the matching products and display them. And when the Edit link is clicked, I want to make sure that this screen navigates to the Edit screen with the correct parameters.

I'm going to do this using an MVP (Model-View-Presenter) approach, the <a href="http://www.martinfowler.com/eaaDev/SupervisingPresenter.html">Supervising Controller variant</a> to be more specific.  As with any pattern, I believe you should use it in a pragmatic way. In this implementation I don't follow every rule strictly, I will just try to provide an approach that offers you all the advantages of the pattern, while trying to make it as easy as possible to implement.

I'm also going to use a couple of techniques that will allow me to write fast tests which should be easy to maintain as well.  I'll use a mocking framework (Rhino Mocks), Dependency Injection and an Inversion of Control container (Castle Windsor). Don't worry if you're unfamiliar with these topics, when needed I'll try to explain everything. And of course, you're always welcome to ask questions :)

Anyway, enough talk... let's get started shall we?

First, we need an abstract way to define a View (which corresponds to a page basically):

<script src="https://gist.github.com/3675170.js?file=s1.cs"></script>

Every page in this application implements the IView interface, although each page also implements a more specific interface.  Each view will have a controller, which has to be able to communicate with the view.  This communication is usually limited to providing data and telling the view to perform a DataBind operation, or telling it to display a certain message.  But the controller can sometimes also request information from the view, like asking if the view is currently in a PostBack, or if the view is currently valid, or whatever else you might need.

This is the interface of the page shown above:

<script src="https://gist.github.com/3675170.js?file=s2.cs"></script>

As you can see, this interface merely provides a few properties on top of what the IView interface provides. This is one example of where I deviate from the typical implementations of this pattern.  Most people define events in the view's interface for each user action that can occur.  The controller then subscribes to these events when it is bound to the view, and it handles those events.  While that is theoretically a nice approach, I found it to be somewhat cumbersome, both in writing more code than you really need and making the tests a bit more cumbersome to write. So in my implementation, the Controller actually offers public methods for each user action. The view then simply calls the controller's public methods when these actions occur.  This means that both the view and the controller know about each other. A lot of purists will not like this, but I believe the (mostly theoretical) downsides to the view and the controller knowing about each other don't match up to simpler implementation.

Anyways, you probably want to know what the controller looks like. We'll get to that soon, but first we define a base Controller type that each controller will inherit from:

<script src="https://gist.github.com/3675170.js?file=s3.cs"></script>

Right now, this is a pretty simple class, but as you implement more screens, you will most likely refactor common controller methods to this base class.  In this application, the controller will usually communicate with a proxy to a remote service. That proxy is actually the model in this implementation. Obviously, if you don't need a service layer you can simply use the real Model objects in the controller.  But since a proxy to a remote service is an expensive object that needs to be cleaned up properly, I made the controller inherit from the <a href="/blog/2008/06/disposing-of-the-idisposable-implementation/">Disposable class</a>.  Each derived controller will need to provide a method to clean up its expensive resources.

The specific controller for this application looks like this:

<script src="https://gist.github.com/3675170.js?file=s4.cs"></script>

We'll add the methods to handle the user actions later on, so this class is not complete yet.  You can see that the controller has 3 dependencies... the first being the view, the second is the service and the third one is an instance of the IProductsNavigator interface.  I use small navigator classes to perform all of my navigation because it makes it easy to test that a navigation has occurred without actually having to move to another page. 

The IProductsNavigator interface looks like this:

<script src="https://gist.github.com/3675170.js?file=s5.cs"></script>

Nothing special here, just a method to move to the edit screen with an optional product Id (the edit screen is also used to edit a new product's data, and then the productId parameter will be null) and another method to move to the Search screen.  The code of the class that implements this interface merely does a redirect to the correct page.  But it's important to get that code out of the controller because it would lower testability.

Anyways, let's get to the whole writing tests part. Because we're going to write as much code as possible in the controller instead of the view, we will simply test the controller with a fake view and a fake model (service). That's right, we're going to test our application code for this page without an actual page.  We will mock the view and the service, and we'll pass those mocked dependencies to the controller. In our tests we will then instruct the mocks to behave like their real versions, depending on what we're trying to test.

First of all, we'll define a base controller test class:

<script src="https://gist.github.com/3675170.js?file=s6.cs"></script>

This class will simply provide some helper methods that will be common to our controller tests. The method that is already there can be ignored for now, but if you want to know what it does you can look <a href="/blog/2008/06/testing-batched-service-calls/">here</a>.  I'll also (briefly) explain it when it's used in a test.

Our test class needs to set up the mocked dependencies and provide a way to create the controller with those mocks so we already have the following code:

<script src="https://gist.github.com/3675170.js?file=s7.cs"></script>

Nothing special here... the mocks are create before each test in the SetUp method, and we have helper method which creates the controller with the mocks so we don't have to do this ourselves in each test.

Ok, now we can finally get to our first test. I don't know about you, but I really hate it when a page performs code that it really doesn't have to do in a PostBack.  So we'll guard against that with the following test:

<script src="https://gist.github.com/3675170.js?file=s8.cs"></script>

We instruct the mocked view to return true for the IsPostBack property. Then we create the controller, call its Load method and we verify that the Service's Process method was not called in any way. Pretty simple, right? It does get a bit more complicated when we want to test that the correct data is retrieved when the page is initially loaded:

<script src="https://gist.github.com/3675170.js?file=s9.cs"></script>

First, we create two empty arrays of objects that we'll instruct the mocked service to return when its Process method is called.  We're using the PrepareServiceToReturnResponses method here, which you've seen listed in the ControllerTest class. It basically allows you to provide Response instances and it uses the ServiceRequestResponseSpy class to hook into the mocked service.  If you want to know the details behind this technique, go <a href="/blog/2008/06/testing-batched-service-calls/">here</a>.  

Then we set some expectations on the view. We expect that its ProductCategories property will be set to the value that we've instructed the mocked service to return.  Same thing for the Suppliers property. Then we define an expectation that the view's DataBind method should be called.  After that, we create the controller, call the Load method and we verify that all expectations on the view were met. We also assert that the service indeed received the proper requests.

So what code did we just test? Well, the Load method of the controller, which now looks like this:

<script src="https://gist.github.com/3675198.js?file=s1.cs"></script>

The load method uses the service to retrieve the product categories and the suppliers, in one remote call.  You can find more information on that <a href="/blog/2008/06/batching-wcf-calls/">here</a> and <a href="/blog/2008/06/the-service-call-batcher/">here</a>.

Now we can write a test to make sure that the controller behaves correctly when the user presses the Search button:

<script src="https://gist.github.com/3675198.js?file=s2.cs"></script>

This should look somewhat familiar by now. We set up an empty array of ProductOverview instances that we want the service to return when it receives a request.  We then set the expectations on the view, just like we did in the previous test.  Then we create the controller and call its Search method with some search parameters. Then we verify that the view's expectations were met, and we use the service spy to retrieve the request that it received.  We then verify that the request parameters are the same as the ones we sent to the controller.

The Search method of the controller looks like this:

<script src="https://gist.github.com/3675198.js?file=s3.cs"></script>

Our final test is very simple. We just need to make sure that the page navigates to another page with correct parameter when the user presses the Edit link next to a product:

<script src="https://gist.github.com/3675198.js?file=s4.cs"></script>

This really doesn't need any explanation right? :)

And the code in the controller looks like this:

<script src="https://gist.github.com/3675198.js?file=s5.cs"></script>

We've already implemented all of the logic we need for this page, and we haven't even started working on the page yet! First we provide a bit of plumbing code to make sure our pages are capable of correctly creating the correct controller and making sure it gets disposed properly when the page has been rendered.  So we have the following base page:

<script src="https://gist.github.com/3675198.js?file=s6.cs"></script>

And the code of the real page looks like this:

<script src="https://gist.github.com/3675198.js?file=s7.cs"></script>

So we've minimized the code in the actual page, and the important parts are all covered with tests. You're probably thinking "that is a lot of test code for so little real code", and you're right... this approach does lead to a lot of test code.  But it also leads to a lot less debugging.

This was a pretty simple example... but you can of course use this approach on complex screens as well.  You just need to provide a public method for each kind of action on your controller, and try to do as much as possible in the controller.  You really want the view to remain as "dumb" as possible.  It should delegate all logic to the controller, and then simply focus on data binding, and in screens where you can edit data, client-side input validation.  The more code you can push to your controller, the more you can cover it with tests.

For instance, for this particular screen you probably want to add sorting capabilities.  Just provide a public Sort method on your controller which takes a sort expression and sort direction as parameters.  Then you can write tests to verify that the controller indeed offers the view a properly sorted list of data when the Sort method is called.  In your view, you would then simply need to call the Sort method and pass the correct parameters when the user clicks on a column header.  Or if you want to provide a Delete link (or image, ideally) next to each product, you'd provide a public Delete method on the controller which takes the product's Id as the parameter.  Then you can start writing interesting tests, like verifying that the controller sends a DeleteProductRequest instance to the service, and perhaps retrieves an updated list of products to bind to the view. Or better yet, you can write a test where the mocked service throws a business exception when you try to delete a product, to verify that the controller displays the correct message on the view and doesn't remove the product from the view's list. 

Anything is pretty much possible, you just gotta make it work.
