A couple of months ago, i knew very little about REST and Restful services. Since it was increasingly getting more attention in the developer community, i wanted to find out what it was about. Luckily for me, O'Reilly had recently released <a href="http://oreilly.com/catalog/9780596805838/">REST In Practice</a> and when it was listed as an <a href="http://feeds.feedburner.com/oreilly/ebookdealoftheday">O'Reilly Deal Of The Day</a>, i bought it without thinking twice. Unfortunately, i temporarily stopped reading it about halfway through because i needed to finish some other books first, but i recently picked up where i left off.

Coming from the Microsoft world were SOAP services have for a long time been the norm in most projects, it's very interesting to see what REST is all about and how these Restful services are built.  If you don't know anything about REST, you might want to check out this <a href="http://martinfowler.com/articles/richardsonMaturityModel.html">article</a> by Martin Fowler first. Obviously, you don't need to read that before you can read the book but it might pique your interest. The first chapter does a great job of introducing to the architecture of the Web and how the REST architectural style fits within it.  After that, you're introduced to the Restbucks example, which is used throughout the book to show you how Restful services can work.

The book then gradually starts diving deeper and deeper into the implementation of the Restful services for the Restbucks example. Everything is explained very clearly, and the authors continuously show both the requests and the responses that are going over the wire to illustrate what is going on at the HTTP level, which makes it even easier to understand everything that's being discussed.  After the explanations and request/response examples, there's typically a code-based example in either Java or .NET to show how you can implement these systems. After a while i just started skipping these examples entirely because i thought they didn't really bring any added benefit.  Though i'm sure some people will appreciate those examples being included as well.

Another great thing about this book is that it remains very clear and easy to follow, even though it covers <em>a lot</em> of ground. Here's a brief overview of the things you'll see implemented in the next couple of chapters:
<ul>
	<li>CRUD</li>
	<li>Hypermedia (the engine of application state transformations)</li>
	<li>Everything you need to know about caching</li>
	<li>Event-driven services with Atom</li>
	<li>Atom publishing</li>
	<li>Security: authentication as well as authorization</li>
</ul>

After that, there's a chapter on the semantic web and microformats, which i didn't find very interesting but others likely will. The final 2 chapters make for a great conclusion of the book. First, there's a pretty extensive comparison between the benefits and drawbacks of SOAP versus REST.  And finally, the last chapter covers when it makes sense to use REST and when it doesn't and ends with a recap of the major selling points of using the Web as a central building block of your architecture.

I grok the REST architectural style now, and definitely like it.  If you read this book with an open mind, i'd bet you'll like it a lot as well.