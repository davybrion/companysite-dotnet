Note: This post is part of a series.  Be sure to read the introduction <a href="/blog/2009/08/build-your-own-data-access-layer-series/">here</a>.

By now we've already covered everything that this DAL has to offer, which admittedly isn't all that much.  All of the classes you've seen so far are pretty good at whey they should do, but nobody in their right mind would want to use any of these things directly in application code.  Any easy-to-use DAL should offer a simple facade which sits on top of the underlying system and makes it very easy to perform the most typical tasks that you need it to perform for you.  You shouldn't need to know about specific classes to be able to use it (that goes for most good frameworks and libraries btw).

So once again, i based my approach on what NHibernate does, and with that the ISession interface was born:

<script src="https://gist.github.com/3685104.js?file=s1.cs"></script>

Everything that you need to be able to do with this DAL is provided by this single interface.  And the implementation of the Session class is very easy as well, since we can simply delegate pretty much everything to each of the classes we've covered in the other posts in the series:

<script src="https://gist.github.com/3685104.js?file=s2.cs"></script>

As you can see, there's nothing special here and it's all very straightforward.  Application code can now perform database operations pretty easily once it has a reference to an ISession instance.  And obtaining a reference to an ISession instance can be done through the ISessionFactory interface:

<script src="https://gist.github.com/3685104.js?file=s3.cs"></script>

And its implementation:

<script src="https://gist.github.com/3685104.js?file=s4.cs"></script>

The static Create method takes an assembly and a connection string.  The assembly (containing your entity types) will be used to to build up the metadata model as covered in the <a href="/blog/2009/08/build-your-own-data-access-layer-mapping-classes-to-tables/">Mapping Classes To Tables post</a>.  You would typically call the Create method in your application's startup code, and then you'd have to store a reference to the ISessionFactory somewhere.  Your application code can then simply call the ISessionFactory's CreateSession method and that's all there is to it.