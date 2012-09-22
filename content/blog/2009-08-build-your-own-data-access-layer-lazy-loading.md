Note: This post is part of a series.  Be sure to read the introduction <a href="/blog/2009/08/build-your-own-data-access-layer-series/">here</a>.

In the post about entity hydration, I mentioned the following:

> If we can’t find the referenced entity instance in the first level cache, what should we do? We obviously can’t load it automatically because that could in turn cause referenced entities’ references to be loaded automatically when they are hydrated. Which in turn could cause their referenced entities… Well, I'm sure you get the point. But those properties obviously can’t be set to a null reference either because the column actually does have a valid foreign key value in the database. Explicitly loading referenced properties leads to seriously ugly (and error-prone) code so that’s not an option I'm willing to consider either. The correct way to deal with this is to use lazy loading. To do that in an automated fashion, we need proxy classes. I'm not going to get into these proxy classes and the whole lazy loading thing just yet, since that will be covered in depth in a future post ;)

It's time to go over the implementation of the lazy loading of this DAL.  I honestly expected that this would be the part that would take the most time to get working.  It actually turned out to be the easiest and quickest part of the DAL to develop. 

As I mentioned, we are going to use proxy classes to achieve our goal of lazy loading.  

Consider the following simple entity type:

<script src="https://gist.github.com/3685057.js?file=s1.cs"></script>

If we want to avoid having to put any lazy loading logic within this class, we could inherit from it and add the lazy loading logic in the derived class.  First, we would have to make the properties of our entity virtual:

<script src="https://gist.github.com/3685057.js?file=s2.cs"></script>

Now we could create a proxy class like this:

<script src="https://gist.github.com/3685057.js?file=s3.cs"></script>

This could definitely work.  Whenever we need a proxy to avoid loading an entity, we can simply instantiate a proxy class like the one above, pass it a Session object (again, the Session implementation will be covered in a future post, though I will show the InitializeProxy later on in this post) and once we retrieve any of the overridden properties, the proxy instance will use the Session to initialize itself.  The Session would then use a DatabaseAction and the hydration process to make sure the proxy's properties are filled in with the values of its corresponding properties.  I didn't override the Id property because accessing the primary key of a proxied object should never result in a database call, so there is no reason to override it.  

But we really can't expect people to manually create proxy types like this.  For one, it's repetitive and thus, it is error prone.  Both are issues that we've aimed to avoid with this DAL from the start.  So how can we make this work automagically? The answer is simple: we can use Castle's excellent <a href="http://castleproject.org/dynamicproxy/index.html">DynamicProxy</a> library to generate the proxy classes at runtime for us.

DynamicProxy uses a concept known as an Interceptor.  The Interceptor basically intercepts method calls on proxied objects and allows you to add custom logic before and after the original method calls.  For our lazy loading purposes, we simply need the following LazyLoadingInterceptor class:

<script src="https://gist.github.com/3685057.js?file=s4.cs"></script>

Ok, so what does this class do? Like I said, an interceptor will intercept all method calls on virtual methods of a proxied object.  So basically, if we create a proxy through DynamicProxy, and tell DynamicProxy to add a new instance of this LazyLoadingInterceptor so we can add the behavior of this interceptor to our proxy object, and we then access the properties of our proxy object, it will actually show the same behavior as the manually created CustomerProxy class listed earlier.  And we get all of this for free, without having to modify our original Customer class, except for marking the properties as virtual obviously.

Now, when the LazyLoadingInterceptor calls the Session's InitializeProxy method, the session will delegate this call to our new InitializeProxyAction class:

<script src="https://gist.github.com/3685057.js?file=s5.cs"></script>

As you can see, this is extremely similar to the GetByIdAction.  In fact, I should probably put the common logic in another base DatabaseAction class that sits between DatabaseAction and GetByIdAction and InitializeProxyAction.  Anyways, when the entity has been retrieved, we ask the EntityHydrater to update this entity instance through its newly added UpdateEntity method:

<script src="https://gist.github.com/3685057.js?file=s6.cs"></script>

The Hydrate method is already shown in the post that covers the process of entity hydration, so there's no need to go over that again.

And now there's only one more missing piece in our lazy loading puzzle, which is the actual creation of the proxy in the EntityHydrater's CreateProxy method:

<script src="https://gist.github.com/3685057.js?file=s7.cs"></script>

When using an approach like this, it's best to make everything in your entity classes virtual... NHibernate has a similar restriction and I've tried to explain the reasons behind this in <a href="/blog/2009/03/must-everything-be-virtual-with-nhibernate/">this post</a>.

I'm not sure if I succeeded in explaining this topic in a simple and clear manner, but this technique really is pretty easy.  If you have any questions, I'd be glad to anser them in the comments :)