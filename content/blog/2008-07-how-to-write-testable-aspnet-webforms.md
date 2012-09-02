Microsoft's upcoming ASP.NET MVC framework makes it easy to write tests for your application layer logic. But what about those of us who are stuck with ASP.NET WebForms?  You can still write highly testable ASP.NET WebForms with only a little bit of extra effort. But that extra effort really pays off in the long run.  In this post, i'll give a detailed description of one approach that has worked for me really well.  I first started using this approach for a project at work sometime last year. It allowed me to cover my application logic with a lot of tests which weren't a hassle to write or run, so naturally i've always wanted to write a detailed post on this subject.  I hope you'll like it :) (Btw, if you're new to mocking this might serve as an introduction to that as well)

This is the screen we're going to create:

<a href='http://davybrion.com/blog/wp-content/uploads/2008/07/searchproducts.png'><img src="http://davybrion.com/blog/wp-content/uploads/2008/07/searchproducts.png" alt="" title="searchproducts" width="500" height="243" class="alignleft size-full wp-image-161" /></a>

The very first thing you'll notice is that i completely suck at graphic design. So try to ignore the crappy look, and lets focus on what this screen should do. A user can perform a search on products based on the name, the product category and the supplier of the product. After clicking the Search button, the user is presented with a list of products that match the search criteria.  Next to each product is an Edit link (because i was too lazy to find a nice image for this). When clicked, the application should navigate to an Edit screen where the chosen product can be edited.  To keep this example short (this post will be long enough already!), that's all for this screen.

So what are the things that we would want to test for a simple screen like this? For starters, i want to be sure that when this page is loaded, it retrieves the list of product categories and suppliers, and that it displays them.  Another thing i want to test is that when the search button is clicked, the screen has to retrieve all the matching products and display them. And when the Edit link is clicked, i want to make sure that this screen navigates to the Edit screen with the correct parameters.

I'm going to do this using an MVP (Model-View-Presenter) approach, the <a href="http://www.martinfowler.com/eaaDev/SupervisingPresenter.html">Supervising Controller variant</a> to be more specific.  As with any pattern, i believe you should use it in a pragmatic way. In this implementation i don't follow every rule strictly, i will just try to provide an approach that offers you all the advantages of the pattern, while trying to make it as easy as possible to implement.

I'm also going to use a couple of techniques that will allow me to write fast tests which should be easy to maintain as well.  I'll use a mocking framework (Rhino Mocks), Dependency Injection and an Inversion of Control container (Castle Windsor). Don't worry if you're unfamiliar with these topics, when needed i'll try to explain everything. And of course, you're always welcome to ask questions :)

Anyway, enough talk... let's get started shall we?

First, we need an abstract way to define a View (which corresponds to a page basically):

<code>
<div style="font-family: Consolas; font-size: 10pt; color: black; background: white;">
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; <span style="color: blue;">public</span> <span style="color: blue;">interface</span> <span style="color: #2b91af;">IView</span></p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; {</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span style="color: blue;">bool</span> IsPostBack { <span style="color: blue;">get</span>; }</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span style="color: blue;">bool</span> IsValid { <span style="color: blue;">get</span>; }</p>
<p style="margin: 0px;">&nbsp;</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span style="color: blue;">void</span> DataBind();</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span style="color: blue;">void</span> DisplayErrorMessage(<span style="color: blue;">string</span> message);</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; }</p>
</div>
</code>

Every page in this application implements the IView interface, although each page also implements a more specific interface.  Each view will have a controller, which has to be able to communicate with the view.  This communication is usually limited to providing data and telling the view to perform a DataBind operation, or telling it to display a certain message.  But the controller can sometimes also request information from the view, like asking if the view is currently in a PostBack, or if the view is currently valid, or whatever else you might need.

This is the interface of the page shown above:

<code>
<div style="font-family: Consolas; font-size: 10pt; color: black; background: white;">
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; <span style="color: blue;">public</span> <span style="color: blue;">interface</span> <span style="color: #2b91af;">IProductList</span> : <span style="color: #2b91af;">IView</span></p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; {</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span style="color: #2b91af;">IEnumerable</span>&lt;<span style="color: #2b91af;">ProductCategoryDTO</span>&gt; ProductCategories { <span style="color: blue;">get</span>; <span style="color: blue;">set</span>; }</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span style="color: #2b91af;">IEnumerable</span>&lt;<span style="color: #2b91af;">ProductOverviewDTO</span>&gt; Products { <span style="color: blue;">get</span>; <span style="color: blue;">set</span>; }</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span style="color: #2b91af;">IEnumerable</span>&lt;<span style="color: #2b91af;">SupplierDTO</span>&gt; Suppliers { <span style="color: blue;">get</span>; <span style="color: blue;">set</span>; }</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; }</p>
</div>
</code>

As you can see, this interface merely provides a few properties on top of what the IView interface provides. This is one example of where i deviate from the typical implementations of this pattern.  Most people define events in the view's interface for each user action that can occur.  The controller then subscribes to these events when it is bound to the view, and it handles those events.  While that is theoretically a nice approach, i found it to be somewhat cumbersome, both in writing more code than you really need and making the tests a bit more cumbersome to write. So in my implementation, the Controller actually offers public methods for each user action. The view then simply calls the controller's public methods when these actions occur.  This means that both the view and the controller know about each other. A lot of purists will not like this, but i believe the (mostly theoretical) downsides to the view and the controller knowing about each other don't match up to simpler implementation.

Anyways, you probably want to know what the controller looks like. We'll get to that soon, but first we define a base Controller type that each controller will inherit from:

<code>
<div style="font-family: Consolas; font-size: 10pt; color: black; background: white;">
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; <span style="color: blue;">public</span> <span style="color: blue;">abstract</span> <span style="color: blue;">class</span> <span style="color: #2b91af;">Controller</span>&lt;T&gt; : <span style="color: #2b91af;">Disposable</span>, <span style="color: #2b91af;">IController</span> <span style="color: blue;">where</span> T : <span style="color: #2b91af;">IView</span></p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; {</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span style="color: blue;">protected</span> Controller(T view)</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; {</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; View = view;</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; }</p>
<p style="margin: 0px;">&nbsp;</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span style="color: blue;">protected</span> T View { <span style="color: blue;">get</span>; <span style="color: blue;">set</span>; }</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; }</p>
</div>
</code>

Right now, this is a pretty simple class, but as you implement more screens, you will most likely refactor common controller methods to this base class.  In this application, the controller will usually communicate with a proxy to a remote service. That proxy is actually the model in this implementation. Obviously, if you don't need a service layer you can simply use the real Model objects in the controller.  But since a proxy to a remote service is an expensive object that needs to be cleaned up properly, i made the controller inherit from the <a href="http://davybrion.com/blog/2008/06/disposing-of-the-idisposable-implementation/">Disposable class</a>.  Each derived controller will need to provide a method to clean up its expensive resources.

The specific controller for this application looks like this:

<code>
<div style="font-family: Consolas; font-size: 10pt; color: black; background: white;">
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; <span style="color: blue;">public</span> <span style="color: blue;">class</span> <span style="color: #2b91af;">ProductListController</span> : <span style="color: #2b91af;">Controller</span>&lt;<span style="color: #2b91af;">IProductList</span>&gt;</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; {</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span style="color: blue;">private</span> <span style="color: #2b91af;">IProductManagementService</span> service;</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span style="color: blue;">private</span> <span style="color: blue;">readonly</span> <span style="color: #2b91af;">IProductsNavigator</span> navigator;</p>
<p style="margin: 0px;">&nbsp;</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span style="color: blue;">public</span> ProductListController(<span style="color: #2b91af;">IProductList</span> view, <span style="color: #2b91af;">IProductManagementService</span> service, </p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span style="color: #2b91af;">IProductsNavigator</span> navigator) : <span style="color: blue;">base</span>(view)</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; {</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span style="color: blue;">this</span>.service = service;</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span style="color: blue;">this</span>.navigator = navigator;</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; }</p>
<p style="margin: 0px;">&nbsp;</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span style="color: blue;">protected</span> <span style="color: blue;">override</span> <span style="color: blue;">void</span> DisposeObjects()</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; {</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span style="color: blue;">if</span> (service != <span style="color: blue;">null</span>) service.Dispose();</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; }</p>
<p style="margin: 0px;">&nbsp;</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span style="color: blue;">protected</span> <span style="color: blue;">override</span> <span style="color: blue;">void</span> ClearReferences()</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; {</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; View = <span style="color: blue;">null</span>;</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; service = <span style="color: blue;">null</span>;</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; }</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; }</p>
</div>
</code>

We'll add the methods to handle the user actions later on, so this class is not complete yet.  You can see that the controller has 3 dependencies... the first being the view, the second is the service and the third one is an instance of the IProductsNavigator interface.  I use small navigator classes to perform all of my navigation because it makes it easy to test that a navigation has occurred without actually having to move to another page. 

The IProductsNavigator interface looks like this:

<code>
<div style="font-family: Consolas; font-size: 10pt; color: black; background: white;">
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; <span style="color: blue;">public</span> <span style="color: blue;">interface</span> <span style="color: #2b91af;">IProductsNavigator</span> </p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; {</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span style="color: blue;">void</span> GoToEdit(<span style="color: blue;">int</span>? productId);</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span style="color: blue;">void</span> GoToSearch();</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; }</p>
</div>
</code>

Nothing special here, just a method to move to the edit screen with an optional product Id (the edit screen is also used to edit a new product's data, and then the productId parameter will be null) and another method to move to the Search screen.  The code of the class that implements this interface merely does a redirect to the correct page.  But it's important to get that code out of the controller because it would lower testability.

Anyways, let's get to the whole writing tests part. Because we're going to write as much code as possible in the controller instead of the view, we will simply test the controller with a fake view and a fake model (service). That's right, we're going to test our application code for this page without an actual page.  We will mock the view and the service, and we'll pass those mocked dependencies to the controller. In our tests we will then instruct the mocks to behave like their real versions, depending on what we're trying to test.

First of all, we'll define a base controller test class:

<code>
<div style="font-family: Consolas; font-size: 10pt; color: black; background: white;">
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; <span style="color: blue;">public</span> <span style="color: blue;">abstract</span> <span style="color: blue;">class</span> <span style="color: #2b91af;">ControllerTest</span></p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; {</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span style="color: blue;">protected</span> <span style="color: blue;">static</span> <span style="color: blue;">void</span> PrepareServiceToReturnResponses(<span style="color: #2b91af;">IService</span> service, <span style="color: #2b91af;">ServiceRequestResponseSpy</span> spy,</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span style="color: blue;">params</span> <span style="color: #2b91af;">Response</span>[] responses)</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; {</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; spy.SetResponsesToReturn(responses);</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; service.Stub(s =&gt; s.Process(<span style="color: blue;">null</span>))</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; .IgnoreArguments()</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; .Do(<span style="color: blue;">new</span> <span style="color: #2b91af;">Func</span>&lt;<span style="color: #2b91af;">Request</span>[], <span style="color: #2b91af;">Response</span>[]&gt;(spy.GrabRequestsAndReturnGivenResponses));</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; }</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; }</p>
</div>
</code>

This class will simply provide some helper methods that will be common to our controller tests. The method that is already there can be ignored for now, but if you want to know what it does you can look <a href="http://davybrion.com/blog/2008/06/testing-batched-service-calls/">here</a>.  I'll also (briefly) explain it when it's used in a test.

Our test class needs to set up the mocked dependencies and provide a way to create the controller with those mocks so we already have the following code:

<code>
<div style="font-family: Consolas; font-size: 10pt; color: black; background: white;">
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span style="color: blue;">private</span> <span style="color: #2b91af;">MockRepository</span> mocks;</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span style="color: blue;">private</span> <span style="color: #2b91af;">IProductManagementService</span> service;</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span style="color: blue;">private</span> <span style="color: #2b91af;">IProductList</span> view;</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span style="color: blue;">private</span> <span style="color: #2b91af;">IProductsNavigator</span> navigator;</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span style="color: blue;">private</span> <span style="color: #2b91af;">ProductListController</span> controller;</p>
<p style="margin: 0px;">&nbsp;</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; [<span style="color: #2b91af;">SetUp</span>]</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span style="color: blue;">public</span> <span style="color: blue;">void</span> SetUp()</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; {</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; mocks = <span style="color: blue;">new</span> <span style="color: #2b91af;">MockRepository</span>();</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; service = mocks.DynamicMock&lt;<span style="color: #2b91af;">IProductManagementService</span>&gt;();</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; view = mocks.DynamicMock&lt;<span style="color: #2b91af;">IProductList</span>&gt;();</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; navigator = mocks.DynamicMock&lt;<span style="color: #2b91af;">IProductsNavigator</span>&gt;();</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; }</p>
<p style="margin: 0px;">&nbsp;</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span style="color: blue;">private</span> <span style="color: #2b91af;">ProductListController</span> CreateController()</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; {</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; controller = <span style="color: blue;">new</span> <span style="color: #2b91af;">ProductListController</span>(view, service, navigator);</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span style="color: blue;">return</span> controller;</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; }</p>
</div>
</code>

Nothing special here... the mocks are create before each test in the SetUp method, and we have helper method which creates the controller with the mocks so we don't have to do this ourselves in each test.

Ok, now we can finally get to our first test. I don't know about you, but i really hate it when a page performs code that it really doesn't have to do in a PostBack.  So we'll guard against that with the following test:

<code>
<div style="font-family: Consolas; font-size: 10pt; color: black; background: white;">
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; [<span style="color: #2b91af;">Test</span>]</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span style="color: blue;">public</span> <span style="color: blue;">void</span> DoesNotRetrieveCategoriesAndSuppliersOnLoadIfPostBack()</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; {</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; view.Stub(v =&gt; v.IsPostBack).Return(<span style="color: blue;">true</span>);</p>
<p style="margin: 0px;">&nbsp;</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; mocks.ReplayAll();</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; CreateController().Load();</p>
<p style="margin: 0px;">&nbsp;</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; service.AssertWasNotCalled(s =&gt; s.Process(<span style="color: blue;">null</span>), options =&gt; options.IgnoreArguments());</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; }</p>
</div>
</code>

We instruct the mocked view to return true for the IsPostBack property. Then we create the controller, call its Load method and we verify that the Service's Process method was not called in any way. Pretty simple, right? It does get a bit more complicated when we want to test that the correct data is retrieved when the page is initially loaded:

<code>
<div style="font-family: Consolas; font-size: 10pt; color: black; background: white;">
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; [<span style="color: #2b91af;">Test</span>]</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span style="color: blue;">public</span> <span style="color: blue;">void</span> RetrievesCategoriesAndSuppliersOnLoad()</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; {</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span style="color: blue;">var</span> categoriesToReturn = <span style="color: blue;">new</span> <span style="color: #2b91af;">ProductCategoryDTO</span>[0];</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span style="color: blue;">var</span> suppliersToReturn = <span style="color: blue;">new</span> <span style="color: #2b91af;">SupplierDTO</span>[0];</p>
<p style="margin: 0px;">&nbsp;</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span style="color: blue;">var</span> spy = <span style="color: blue;">new</span> <span style="color: #2b91af;">ServiceRequestResponseSpy</span>();</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; PrepareServiceToReturnResponses(service, spy, <span style="color: blue;">new</span> <span style="color: #2b91af;">GetProductCategoriesResponse</span>(categoriesToReturn),</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span style="color: blue;">new</span> <span style="color: #2b91af;">GetSuppliersResponse</span>(suppliersToReturn));</p>
<p style="margin: 0px;">&nbsp;</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; view.Expect(v =&gt; v.ProductCategories = categoriesToReturn);</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; view.Expect(v =&gt; v.Suppliers = suppliersToReturn);</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; view.Expect(v =&gt; v.DataBind());</p>
<p style="margin: 0px;">&nbsp;</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; mocks.ReplayAll();</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; CreateController().Load();</p>
<p style="margin: 0px;">&nbsp;</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; view.VerifyAllExpectations();</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span style="color: #2b91af;">Assert</span>.IsNotNull(spy.GetRequest&lt;<span style="color: #2b91af;">GetProductCategoriesRequest</span>&gt;());</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span style="color: #2b91af;">Assert</span>.IsNotNull(spy.GetRequest&lt;<span style="color: #2b91af;">GetSuppliersRequest</span>&gt;());</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; }</p>
</div>
</code>

First, we create two empty arrays of objects that we'll instruct the mocked service to return when its Process method is called.  We're using the PrepareServiceToReturnResponses method here, which you've seen listed in the ControllerTest class. It basically allows you to provide Response instances and it uses the ServiceRequestResponseSpy class to hook into the mocked service.  If you want to know the details behind this technique, go <a href="http://davybrion.com/blog/2008/06/testing-batched-service-calls/">here</a>.  

Then we set some expectations on the view. We expect that its ProductCategories property will be set to the value that we've instructed the mocked service to return.  Same thing for the Suppliers property. Then we define an expectation that the view's DataBind method should be called.  After that, we create the controller, call the Load method and we verify that all expectations on the view were met. We also assert that the service indeed received the proper requests.

So what code did we just test? Well, the Load method of the controller, which now looks like this:

<code>
<div style="font-family: Consolas; font-size: 10pt; color: black; background: white;">
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span style="color: blue;">public</span> <span style="color: blue;">void</span> Load()</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; {</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span style="color: blue;">if</span> (!View.IsPostBack)</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; {</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span style="color: blue;">var</span> batcher = <span style="color: blue;">new</span> <span style="color: #2b91af;">ServiceCallBatcher</span>(service);</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; batcher.Add(<span style="color: blue;">new</span> <span style="color: #2b91af;">GetProductCategoriesRequest</span>());</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; batcher.Add(<span style="color: blue;">new</span> <span style="color: #2b91af;">GetSuppliersRequest</span>());</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; View.ProductCategories = batcher.Get&lt;<span style="color: #2b91af;">GetProductCategoriesResponse</span>&gt;().ProductCategories;</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; View.Suppliers = batcher.Get&lt;<span style="color: #2b91af;">GetSuppliersResponse</span>&gt;().Suppliers;</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; View.DataBind();</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; }</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; }</p>
</div>
</code>

The load method uses the service to retrieve the product categories and the suppliers, in one remote call.  You can find more information on that <a href="http://davybrion.com/blog/2008/06/batching-wcf-calls/">here</a> and <a href="http://davybrion.com/blog/2008/06/the-service-call-batcher/">here</a>.

Now we can write a test to make sure that the controller behaves correctly when the user presses the Search button:

<code>
<div style="font-family: Consolas; font-size: 10pt; color: black; background: white;">
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; [<span style="color: #2b91af;">Test</span>]</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span style="color: blue;">public</span> <span style="color: blue;">void</span> CallsTheServiceToSearchForProductsAndBindsResultsToView()</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; {</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span style="color: blue;">const</span> <span style="color: blue;">string</span> productPattern = <span style="color: #a31515;">"whatever"</span>;</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span style="color: blue;">const</span> <span style="color: blue;">int</span> categoryId = 4;</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span style="color: blue;">const</span> <span style="color: blue;">int</span> supplierId = 7;</p>
<p style="margin: 0px;">&nbsp;</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span style="color: blue;">var</span> productsToReturn = <span style="color: blue;">new</span> <span style="color: #2b91af;">ProductOverviewDTO</span>[0];</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span style="color: blue;">var</span> spy = <span style="color: blue;">new</span> <span style="color: #2b91af;">ServiceRequestResponseSpy</span>();</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; PrepareServiceToReturnResponses(service, spy, <span style="color: blue;">new</span> <span style="color: #2b91af;">GetProductOverviewsResponse</span>(productsToReturn));</p>
<p style="margin: 0px;">&nbsp;</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; view.Expect(v =&gt; v.Products = productsToReturn);</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; view.Expect(v =&gt; v.DataBind());</p>
<p style="margin: 0px;">&nbsp;</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; mocks.ReplayAll();</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; CreateController().Search(productPattern, categoryId, supplierId);</p>
<p style="margin: 0px;">&nbsp;</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; view.VerifyAllExpectations();</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span style="color: blue;">var</span> request = spy.GetRequest&lt;<span style="color: #2b91af;">GetProductOverviewsRequest</span>&gt;();</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span style="color: #2b91af;">Assert</span>.AreEqual(productPattern, request.NamePattern);</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span style="color: #2b91af;">Assert</span>.AreEqual(categoryId, request.CategoryId);</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span style="color: #2b91af;">Assert</span>.AreEqual(supplierId, request.SupplierId);</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; }</p>
</div>
</code>

This should look somewhat familiar by now. We set up an empty array of ProductOverview instances that we want the service to return when it receives a request.  We then set the expectations on the view, just like we did in the previous test.  Then we create the controller and call its Search method with some search parameters. Then we verify that the view's expectations were met, and we use the service spy to retrieve the request that it received.  We then verify that the request parameters are the same as the ones we sent to the controller.

The Search method of the controller looks like this:

<code>
<div style="font-family: Consolas; font-size: 10pt; color: black; background: white;">
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span style="color: blue;">public</span> <span style="color: blue;">void</span> Search(<span style="color: blue;">string</span> name, <span style="color: blue;">int</span>? productCategoryId, <span style="color: blue;">int</span>? supplierId)</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; {</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span style="color: blue;">var</span> response = service.GetProductOverviews(</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span style="color: blue;">new</span> <span style="color: #2b91af;">GetProductOverviewsRequest</span>(name, productCategoryId, supplierId));</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; View.Products = response.Products;</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; View.DataBind();</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; }</p>
</div>
</code>

Our final test is very simple. We just need to make sure that the page navigates to another page with correct parameter when the user presses the Edit link next to a product:

<code>
<div style="font-family: Consolas; font-size: 10pt; color: black; background: white;">
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; [<span style="color: #2b91af;">Test</span>]</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span style="color: blue;">public</span> <span style="color: blue;">void</span> NavigatesToEditProductScreenWhenEditProductIsTriggered()</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; {</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span style="color: blue;">const</span> <span style="color: blue;">int</span> productId = 5;</p>
<p style="margin: 0px;">&nbsp;</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; navigator.Expect(n =&gt; n.GoToEdit(productId));</p>
<p style="margin: 0px;">&nbsp;</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; mocks.ReplayAll();</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; CreateController().EditProduct(productId);</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; navigator.VerifyAllExpectations();</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; }</p>
</div>
</code>

This really doesn't need any explanation right? :)

And the code in the controller looks like this:

<code>
<div style="font-family: Consolas; font-size: 10pt; color: black; background: white;">
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span style="color: blue;">public</span> <span style="color: blue;">void</span> EditProduct(<span style="color: blue;">int</span> productId)</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; {</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; navigator.GoToEdit(productId);&nbsp;&nbsp; </p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; }</p>
</div>
</code>

We've already implemented all of the logic we need for this page, and we haven't even started working on the page yet! First we provide a bit of plumbing code to make sure our pages are capable of correctly creating the correct controller and making sure it gets disposed properly when the page has been rendered.  So we have the following base page:

<code>
<div style="font-family: Consolas; font-size: 10pt; color: black; background: white;">
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; <span style="color: blue;">public</span> <span style="color: blue;">abstract</span> <span style="color: blue;">class</span> <span style="color: #2b91af;">NorthwindPage</span>&lt;T&gt; : <span style="color: #2b91af;">Page</span> <span style="color: blue;">where</span> T : <span style="color: #2b91af;">IController</span></p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; {</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span style="color: blue;">protected</span> NorthwindPage()</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; {</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; Controller = CreateController();</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; }</p>
<p style="margin: 0px;">&nbsp;</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span style="color: blue;">protected</span> T Controller { <span style="color: blue;">get</span>; <span style="color: blue;">private</span> <span style="color: blue;">set</span>; }</p>
<p style="margin: 0px;">&nbsp;</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span style="color: blue;">protected</span> <span style="color: blue;">abstract</span> T CreateController();</p>
<p style="margin: 0px;">&nbsp;</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span style="color: blue;">protected</span> <span style="color: blue;">override</span> <span style="color: blue;">void</span> OnPreRenderComplete(<span style="color: #2b91af;">EventArgs</span> e)</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; {</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span style="color: blue;">base</span>.OnPreRenderComplete(e);</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; Controller.Dispose();</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; }</p>
<p style="margin: 0px;">&nbsp;</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span style="color: green;">// there are also some small helper methods in here that aren't really</span></p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span style="color: green;">// relevate to the example so i left them out</span></p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; }</p>
</div>
</code>

And the code of the real page looks like this:

<code>
<div style="font-family: Consolas; font-size: 10pt; color: black; background: white;">
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; <span style="color: blue;">public</span> <span style="color: blue;">partial</span> <span style="color: blue;">class</span> <span style="color: #2b91af;">ProductList</span> : <span style="color: #2b91af;">NorthwindPage</span>&lt;<span style="color: #2b91af;">ProductListController</span>&gt;, <span style="color: #2b91af;">IProductList</span></p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; {</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span style="color: blue;">public</span> <span style="color: #2b91af;">IEnumerable</span>&lt;<span style="color: #2b91af;">ProductCategoryDTO</span>&gt; ProductCategories { <span style="color: blue;">get</span>; <span style="color: blue;">set</span>; }</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span style="color: blue;">public</span> <span style="color: #2b91af;">IEnumerable</span>&lt;<span style="color: #2b91af;">ProductOverviewDTO</span>&gt; Products { <span style="color: blue;">get</span>; <span style="color: blue;">set</span>; }</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span style="color: blue;">public</span> <span style="color: #2b91af;">IEnumerable</span>&lt;<span style="color: #2b91af;">SupplierDTO</span>&gt; Suppliers { <span style="color: blue;">get</span>; <span style="color: blue;">set</span>; }</p>
<p style="margin: 0px;">&nbsp;</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span style="color: blue;">protected</span> <span style="color: blue;">void</span> Page_Load(<span style="color: blue;">object</span> sender, <span style="color: #2b91af;">EventArgs</span> e)</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; {</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; Controller.Load();</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; }</p>
<p style="margin: 0px;">&nbsp;</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span style="color: blue;">protected</span> <span style="color: blue;">override</span> <span style="color: #2b91af;">ProductListController</span> CreateController()</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; {</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span style="color: blue;">return</span> <span style="color: #2b91af;">Container</span>.Resolve&lt;<span style="color: #2b91af;">ProductListController</span>&gt;(<span style="color: blue;">new</span> { view = <span style="color: blue;">this</span> });</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; }</p>
<p style="margin: 0px;">&nbsp;</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span style="color: blue;">protected</span> <span style="color: blue;">void</span> SearchButton_Click(<span style="color: blue;">object</span> sender, <span style="color: #2b91af;">EventArgs</span> e)</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; {</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; Controller.Search(NameTextBox.Text, GetSelectedId(ProductCategoryList), GetSelectedId(SupplierList));</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; }</p>
<p style="margin: 0px;">&nbsp;</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span style="color: blue;">protected</span> <span style="color: blue;">void</span> EditProductLink_Click(<span style="color: blue;">object</span> sender, <span style="color: #2b91af;">EventArgs</span> e)</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; {</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; Controller.EditProduct(GetIdForCurrentRow(sender <span style="color: blue;">as</span> <span style="color: #2b91af;">IButtonControl</span>));</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; }</p>
<p style="margin: 0px;">&nbsp;</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span style="color: blue;">public</span> <span style="color: blue;">override</span> <span style="color: blue;">void</span> DataBind()</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; {</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span style="color: blue;">if</span> (ProductCategories != <span style="color: blue;">null</span> &amp;&amp; Suppliers != <span style="color: blue;">null</span>)</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; {</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span style="color: blue;">var</span> categories = <span style="color: blue;">new</span>[] { <span style="color: blue;">new</span> <span style="color: #2b91af;">ProductCategoryDTO</span> { Id = -1, Name = <span style="color: #a31515;">"All"</span> } }.Union(ProductCategories);</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span style="color: blue;">var</span> suppliers = <span style="color: blue;">new</span>[] { <span style="color: blue;">new</span> <span style="color: #2b91af;">SupplierDTO</span> { Id = -1, CompanyName = <span style="color: #a31515;">"All"</span> } }.Union(Suppliers);</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; PrepareDropDownList(ProductCategoryList, categories, <span style="color: #a31515;">"Name"</span>, <span style="color: #a31515;">"Id"</span>);</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; PrepareDropDownList(SupplierList, suppliers, <span style="color: #a31515;">"CompanyName"</span>, <span style="color: #a31515;">"Id"</span>);</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; }</p>
<p style="margin: 0px;">&nbsp;</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span style="color: blue;">if</span> (Products != <span style="color: blue;">null</span>)</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; {</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; ProductsGrid.DataSource = Products;</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; }</p>
<p style="margin: 0px;">&nbsp;</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span style="color: blue;">base</span>.DataBind();</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; }</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; }</p>
</div>
</code>

Most of this is pretty straightforward, except maybe the code in the CreateController method. I don't like to repeat myself (although i'm aware that i often do that anyway) so you can find the story behind that line of code <a href="http://davybrion.com/blog/2008/06/automanual-dependency-injection/">here</a>.

So we've minimized the code in the actual page, and the important parts are all covered with tests. You're probably thinking "that is a lot of test code for so little real code", and you're right... this approach does lead to a lot of test code.  But it also leads to a lot less debugging :)

This was a pretty simple example... but you can of course use this approach on complex screens as well.  You just need to provide a public method for each kind of action on your controller, and try to do as much as possible in the controller.  You really want the view to remain as "dumb" as possible.  It should delegate all logic to the controller, and then simply focus on data binding, and in screens where you can edit data, client-side input validation.  The more code you can push to your controller, the more you can cover it with tests.

For instance, for this particular screen you probably want to add sorting capabilities.  Just provide a public Sort method on your controller which takes a sort expression and sort direction as parameters.  Then you can write tests to verify that the controller indeed offers the view a properly sorted list of data when the Sort method is called.  In your view, you would then simply need to call the Sort method and pass the correct parameters when the user clicks on a column header.  Or if you want to provide a Delete link (or image, ideally) next to each product, you'd provide a public Delete method on the controller which takes the product's Id as the parameter.  Then you can start writing interesting tests, like verifying that the controller sends a DeleteProductRequest instance to the service, and perhaps retrieves an updated list of products to bind to the view. Or better yet, you can write a test where the mocked service throws a business exception when you try to delete a product, to verify that the controller displays the correct message on the view and doesn't remove the product from the view's list. 

Anything is pretty much possible, you just gotta make it work :)

Hope you enjoyed this post, i for one am very happy to have finally written it since it's been on my TODO list for months now :)

<a href="http://www.dotnetkicks.com/kick/?url=http%3a%2f%2fdavybrion.com%2fblog%2f2008%2f07%2fhow-to-write-testable-aspnet-webforms%2f"><img src="http://www.dotnetkicks.com/Services/Images/KickItImageGenerator.ashx?url=http%3a%2f%2fdavybrion.com%2fblog%2f2008%2f07%2fhow-to-write-testable-aspnet-webforms%2f" border="0" alt="kick it on DotNetKicks.com" /></a>