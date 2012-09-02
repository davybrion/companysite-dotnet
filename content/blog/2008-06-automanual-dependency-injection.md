I apologize in advance for using the term 'automanual' but if you've been reading this blog for a while you already know i completely suck at coming up with good names.  So bear with me, and you'll probably understand what i mean as you work your way through this post.  It really is pretty cool... i promise :p

I'm playing around with some <a href="http://haacked.com/archive/2006/08/09/ASP.NETSupervisingControllerModelViewPresenterFromSchematicToUnitTestsToCode.aspx">supervising controller MVP</a> stuff using regular ASP.NET webforms.  Yes i know, i should be using ASP.NET MVC but i have a strict "i don't touch it before there's a final release"-policy when it comes to Microsoft products (as opposed to my "lemme just build the latest version of the trunk"-policy for various open source products). Anyways, what i'm trying to do is pretty simple.  I have a view (ProductList.aspx) which uses a supervising controller (ProductListController). The view (the aspx page) will notify the controller when it needs to do something through events. The controller will then do whatever it needs to do and it will send the data back to the view through properties of the view.  If that's not clear to you, read the post i linked to for a much better and detailed explanation.   

Since ASP.NET automatically instantiates your aspx page, the easiest thing to do is to let the view create the controller and then pass itself as a parameter to the controller.  But the controller also has other dependencies, such as a service which exposes the business logic that we need for this screen. So i have two options: i either create the controller myself and provide all the dependencies, or i use an inversion of control container to create the controller and to wire up all the dependencies.  I don't want to have to modify my view code whenever i add/remove a dependency of the controller, so i go with the inversion of control container.

Suppose the constructor of the controller looks like this:

<code>

<div style="font-family: Consolas; font-size: 10pt; color: black; background: white;">
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span style="color: blue;">public</span> ProductListController(<span style="color: #2b91af;">IProductList</span> view, <span style="color: #2b91af;">IProductsService</span> productsService)</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; {</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span style="color: blue;">this</span>.view = view;</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span style="color: blue;">this</span>.productsService = productsService;</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; }</p>
</div>

</code>

Here's where it gets tricky... since the view (which implements the IProductList interface) is asking the container to create a ProductListController, how can the container pass the correct IProductList dependency to the controller? We can't use the regular dependency look-up mechanisms because our controller actually needs the current view instance, but that view instance is asking the container to create the controller! By default, the container has no way whatsoever to resolve the IProductList dependency to the current view instance.  So basically, what we want to do in this case is to manually provide the view dependency, but still have the container automatically provide the IProductsService dependency (hence the term 'automanual' which you have to admit is starting to sound pretty good at this point, right?).  And of course, we want all of this to work automagically.

It turns out that Castle's Windsor actually does have some slick tricks to make this work. Here's how we can create the controller through the container from the view:

<code>

<div style="font-family: Consolas; font-size: 10pt; color: black; background: white;">
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span style="color: #2b91af;">IProductListController</span> controller = </p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span style="color: #2b91af;">Container</span>.Resolve&lt;<span style="color: #2b91af;">IProductListController</span>&gt;(<span style="color: blue;">new</span> { view = <span style="color: blue;">this</span> });</p>
</div>

</code>

Told you it was slick! It's pretty easy actually... the parameter we pass to the Resolve method is an instance of an anonymous type with a view property.  We set the view property to the current aspx instance (using the 'this' keyword obviously) and Windsor is smart enough to figure out that this value should be used to satisfy the view dependency instead of using it's normal look-up mechanisms.  The result is that our controller has a reference to the current view, and its IProductsService reference is resolved as the container would typically resolve dependencies.  Pretty sweet.

Btw, i googled the term 'automanual' and sure enough, it already exists... but it's not really used in the context of writing code so if this thing sticks, remember where you heard it first ;)
