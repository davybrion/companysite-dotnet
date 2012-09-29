Creating a DSL seems like a hard thing to do, right? While there are various interesting challenges that you'll need to deal with if you want to build and use a real DSL, the initial step of getting it working is actually a lot easier than you might think it is.  I'm gonna walk you through the creation of a simple DSL, suitable for a domain that all of us have experience with.  The domain is quite simple: describing entities, their properties and their relationships.  Forget for a second that you could obviously get the exact same information from a set of class definitions.  It's merely a technical exercise using a domain that we all know :)

One of the key questions that we need to ask ourselves is: what kind of concepts do we want to be able to describe with our DSL? In our case, we want to describe a model consisting of entities.  Disregarding behavior for now, we can say that each entity will consist of properties.  Properties could be regular properties, references to other Entities, or collections of other entities.  Here's one way (of many, obviously) to model that in Ruby (since that language makes it very easy to define a DSL):

<script src="https://gist.github.com/3728369.js?file=s1.rb"></script>

That gives us a simple, yet complete object model to describe entities, their properties and their relationships.  The next question is: how do we define the DSL?  Considering an Invoice entity, suppose we'd like to describe it in our DSL like this:

<script src="https://gist.github.com/3728369.js?file=s2.rb"></script>

This really tells us anything we need to know about this entity, and we could use this data for pretty much everything we want.  There is no explicit or implicit link to any specific kind of technology, like say, a relational database, a document database, or some kind of databinding technology.  We could transform or extend this data to suit whichever purpose we deem fit.

So now that we know how we want to describe our entities and their relationships, we can implement the language.  As mentioned in the title of this post, this is the implementation of a <em>simple</em> DSL.  It's just to illustrate an idea, and not an approach that is guaranteed to stand up to the real-world requirements that a DSL could face (and that depends on a case by case basis). So in this case, we're going to go with an implementation where each entity is described in its own file, and its filename must end with '_def.rb'.  With that limitation in mind, we can do this:

<script src="https://gist.github.com/3728369.js?file=s3.rb"></script>

As you can see, we have 'global' method definitions (they're actually implicitly added to the Object class) which correspond with our language 'keywords'.  Those method implementations use the model that we defined earlier to build a nice object graph based on what we describe through our DSL.

You'll notice that after the method definitions, you can see the following code:

<script src="https://gist.github.com/3728369.js?file=s4.rb"></script>

And that's the clue to this simple DSL: it loops through each file that matches the '*_def.rb' pattern, sets an instance variable named @current_entity (implicitly added to the current Object instance in this case) to nil, and then <em>loads</em> the current file in the loop.  The load method (it might look like a keyword, but it's a method) executes the ruby code in the given file <em>in place</em>, meaning that it shares the same scope.  In other words, the methods that we've defined here are accessible to our DSL declarations since those are executed within the same scope.  And since those method implementations manipulate our domain model, we just built a simple 'language' to describe our entities, their properties and their relations.

Suppose we've got the following entity definitions (each would be in a separate file, but they are just listed all at once here):

<script src="https://gist.github.com/3728369.js?file=s5.rb"></script>

This describes a very small domain model consisting of 4 entities.  In the code listed above, you may have noticed the following piece at the end:

<script src="https://gist.github.com/3728369.js?file=s6.rb"></script>

Given the 4 described entities, running the code above results in the following output:

<script src="https://gist.github.com/3728369.js?file=s7.txt"></script>

So there you have it, we described our entities, their properties and their relationships in a very simple manner and those descriptions were interpreted and the data has been put into an object model that we can use for a variety of purposes if we wanted to.  And there really are a lot of interesting things we can do with this, especially when keeping IronRuby in mind :)