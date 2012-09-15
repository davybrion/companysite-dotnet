As some of you already know, i've been experimenting with getting NHibernate and Ruby (through IronRuby) to play nice together.  In this post, i'll go over what already works and how i got it working.  

Suppose we have the following 2 NHibernate mappings:

<script src="https://gist.github.com/3728233.js?file=s1.xml"></script>

And suppose we have the following 2 classes:

<script src="https://gist.github.com/3728233.js?file=s2.rb"></script>

The only atypical thing about that Ruby code is the usage of System::Collections::ArrayList.  That's something i haven't been able to workaround yet: if you want to use collections, you'll need to use the .NET ones for now.

I'm relying on 2 things to get everything working.  One is NHibernate's Map EntityMode, the other is my own Ruby magic which i'll cover later.  The important thing to know is that the Map EntityMode basically works without classes, but with dictionaries.  Instead of instances of entity classes, NHibernate will return or accept dictionaries where the keys correspond to property names and the values correspond to their respective property's value.  Though the goal was that the developer need not use the dictionaries directly, as the above 2 Ruby classes show.  I'll get into the details of the Ruby magic later on in this post, but for now it's important to know that there's an ObjectFactory class which takes care of transforming the dictionaries that i get from NHibernate to either real instances of entity classes, or proxies of them.

First, let's take a look at transitive persistence:

<script src="https://gist.github.com/3728233.js?file=s3.cs"></script>

The output of running that code is this:

<script src="https://gist.github.com/3728233.js?file=s4.sql"></script>

As you can see, transitive persistence is working nicely, even with collections.  Now let's see how we can retrieve that data from the database and into our Ruby objects.  First i need to show the following 2 helper methods for displaying the data:

<script src="https://gist.github.com/3728233.js?file=s5.cs"></script>

Now we can get the artist we just created with a simple call to session.Get:

<script src="https://gist.github.com/3728233.js?file=s6.cs"></script>

And here's the output of that in the console:

<script src="https://gist.github.com/3728233.js?file=s7.sql"></script>

As you can see, the lazy loading of the albums collection works just as you'd expect it to.  Speaking of lazy-loading, we can do the same thing with a call to session.Load instead of session.Get:

<script src="https://gist.github.com/3728233.js?file=s8.cs"></script>

As you may or may not know, session.Load returns a proxy of an entity instead of actually fetching it from the database immediately (unless the instance is already in the session cache, which my current ruby code can't handle yet).  NHibernate doesn't hit the database until you access any of the properties of the entity outside of the identifier, which the output of this code clearly shows:

<script src="https://gist.github.com/3728233.js?file=s9.sql"></script>

Notice that the select statement is outputted right before we access the name of the artist, instead of immediately as in the previous example.  

We've got lazy-loading covered, but what about eager loading? Well, take a look at the following code:

<script src="https://gist.github.com/3728258.js?file=s1.cs"></script>

This fetches our artist and immediately joins its albums in the same query.  When we access the albums of the artist, it no longer needs to go to the database:

<script src="https://gist.github.com/3728258.js?file=s2.sql"></script>

Obviously, if we omit setting the fetchmode of the albums association we get the same output as we would get from using session.Get:

<script src="https://gist.github.com/3728258.js?file=s3.cs"></script>

<script src="https://gist.github.com/3728258.js?file=s4.sql"></script>

Eager fetching also works in the other direction, when fetching albums with their artist included automatically:

<script src="https://gist.github.com/3728258.js?file=s5.cs"></script>

This results in the following output:

<script src="https://gist.github.com/3728258.js?file=s6.sql"></script>

Finally, we'll retrieve our artist and modify some of its data:

<script src="https://gist.github.com/3728258.js?file=s7.cs"></script>

If we then run the following code again:

<script src="https://gist.github.com/3728258.js?file=s8.cs"></script>

We can see that the data has indeed been changed as it should:

<script src="https://gist.github.com/3728258.js?file=s9.sql"></script>

Ok, so how does this all work? After all, NHibernate returns and expects dictionaries and as you can see in the code of the ruby classes, there are no dictionaries being used.  The answer is actually pretty simple.  NHibernate returns and expects dictionaries.  I return and expect entity instances.  Clearly, all we need to do is make sure that our entities pretend to be dictionaries and NHibernate will never need to know what on earth we're doing.

The first thing we need to do is to modify the implementation of the ruby classes that we have created for our entities.  Obviously, i wouldn't want anyone to have to do that manually, so my ruby magic just does this at runtime.  The only limit that is placed on the code you write in ruby is that within the entity classes, you can never touch the private instance fields of the attributes that you've defined.  You always have to go through the accessors.  Because of that limit, i can just replace all of the accessor methods with implementations that use the dictionary that NHibernate gives me as the backing store of the data instead of using instance fields.  I also make sure that all equality checks are based on the underlying dictionary instead of the actual object.  This passes everything but a straight-up reference check.  Finally, we need to make sure that our objects can be cast to an IDictionary and that we implement the indexer property of the IDictionary interface because NHibernate will use that when we pass it transient instances to insert into the database.

First, let's take a look at the ObjectFactory class, which has a couple of class methods that we use from our .NET code to create entities based on the dictionaries that we get from NHibernate:

<script src="https://gist.github.com/3728289.js?file=s1.rb"></script>

(as you can see from the TODO statement, this whole thing is still a work in progress)

Pretty simple stuff so far... We either create a new instance of the entity class, or of a proxy class for that entity type (i'll cover the creation of proxy classes soon).  We then call its hydrate_from method, which is also added to each entity class dynamically.  There's another (temporary) limitation here... i search for the class name constant in Object, which means that our current approach doesn't work when our entities have namespaces.  Not really a problem for this example, and is easy to add later on when i actually need it.  That's it for the ObjectFactory... the real magic is all contained in the NHibernator module.  And no, i couldn't come up with a better name.  Long-time readers should know by now that i absolutely suck at coming up with good names so that's why we ended up with the NHibernator module.

The NHibernator module does 2 things: it offers a method that you need to use when initializing your application so we can create the proxy classes based on NHibernate's metadata, and it also modifies the accessor methods and adds some new methods whenever it is mixed in to another class.  I'm going to show the code of the NHibernator module in multiple steps to hopefully keep everything as clear as possible.  First of all, i'm gonna show the declaration of a constant and a simple helper method that we're going to need:

<script src="https://gist.github.com/3728289.js?file=s2.rb"></script>

The TYPE_KEY_NAME constant contains the string that NHibernate uses as the key in its dictionaries for the value which returns the current entity's type name.  And the each_writeable_accessor_of method executes the given block for each writeable acessor that a class contains.

And this is how we initialize everything:

<script src="https://gist.github.com/3728289.js?file=s3.rb"></script>

The initialize class method takes an NHibernate ISessionFactory instance and retrieves each mapped entity with the information that we need about it.  Each mapped entity's class is sent the include message with the NHibernator module as a parameter.  This basically mixes in the functionality of the NHibernator module into each entity's class.  I'll discuss this in the next part of the post.  After we've mixed the module into the entity classes, we call the create_proxy_class_for method for each class.  As you can see, creating the proxy classes is very easy stuff.  Any proxy class that we create inherits from the class of the entity, and overrides the accessor method to retrieve the identifier value so that it immediately returns the identifier value.  If we would've kept the default implementation, it would access the dictionary that we got from NHibernate, which would cause a select statement for this proxy to be issued, which we obviously don't want.  Again, this is a work in progress and one limitation that this current proxy implementation has is that you'll get a reference to a dictionary instead of an entity when you access a reference-property of a proxy.  That too will be easy to fix :)

Next up, we need to cover what happens when the NHibernator module is mixed into an entity class.  Ruby has a great hook method for that, which is this:

<script src="https://gist.github.com/3728289.js?file=s4.rb"></script>

I'm doing quite a bit within that method and i want to cover each item in detail.  So, the next couple of pieces of code are all part of the self.include(base) method implementation.  The first thing we do when this module gets included in a class is this:

<script src="https://gist.github.com/3728289.js?file=s5.rb"></script>

This is pretty simple, we're just getting rid of all of the original accessor methods and replacing them with our own implementations that use the dictionary we get from NHibernate as the backing store.  Note that i will discuss the WrappedList class that you see in those getters soon.  The setter methods will also instantiate a new Hashtable if we don't already have a dictionary.  This is necessary for transient instances since NHibernate will treat them as IDictionary instances when we pass them to the session.  Speaking of which, this is the next thing we do:

<script src="https://gist.github.com/3728289.js?file=s6.rb"></script>

This single line enables any piece of .NET code to cast our instances to an IDictionary reference.  Note that we haven't even implemented any of the IDictionary interface's methods yet.  We don't need to implement all of them anyway, just the ones that we know will be used.

Finally, we add all of the following methods to each class that included this module:

<script src="https://gist.github.com/3728289.js?file=s7.rb"></script>

I think that code speaks for itself, except for the Equals and GetHashCode methods... those are just there because i had some issues with IronRuby mapping calls to Equals or GetHashCode to their corresponding ruby alternatives (== and hash).  I eventually upgraded to the latest IronRuby revision from GitHub, because i didn't get correct results with the IronRuby 1.1 alpha 1 to get the equality checks working correctly.

Finally, i needed the following 2 helper classes to make the albums bag work correctly:

<script src="https://gist.github.com/3728289.js?file=s8.rb"></script>

And that's all there is to it.  This is probably the longest blog post i've ever written, but the amount of code involved in getting this working really isn't that much.  Granted, there are still limitations to this approach so some stuff will need to be added to it.  I'm also not saying that this is actually a great idea or that you should start doing this from now on, but well, at least this is possible now :)