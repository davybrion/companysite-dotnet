When I have classes that need to expose collections, I always try to make sure that consumers only get a read-only version of that collection.  Consumers should be able to easily use those collections, but if something would need to be added or removed from a collection, it should happen with a specific call to the owner of that collection instead of manipulating the collection directly.  In .NET 2.0 I often used the ReadOnlyCollection class for this, but now with .NET 3.5 there is a much easier way.  By simply exposing the collections as IEnumerable you prevent consumers from directly adding or removing items from your collection, but with the LINQ extension methods, they are still very usable to anyone that need to use them.

Small example:

<script src="https://gist.github.com/3611768.js?file=s1.cs"></script>

the Parents and Children collections are completely encapsulated, yet still highly usable to consumers:

<a href='/blog/wp-content/uploads/2008/03/usableencapsulatedcollection.png' title='usableencapsulatedcollection.png'><img src='/blog/wp-content/uploads/2008/03/usableencapsulatedcollection.png' alt='usableencapsulatedcollection.png' /></a>
