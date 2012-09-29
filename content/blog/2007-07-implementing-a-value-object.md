A Value Object (also known as Immutable Object) is basically an object without a conceptual identity. A Value Object is defined through its inner values, and not an identity like Entity Objects. This means that a Value Object's inner values can not be changed after object creation, hence the term Immutable Object.  Should you need to change the inner values of the Value Object, you should actually create a new Value Object.

For some of you, this might seem odd. But you've actually used Value Objects on many occassions already. In .NET, strings are Value Objects. So are DateTime instances. If you create a string, you can't modify its inner value. If you do, a new string is actually created. Same thing with a DateTime. The DateTime class provides methods to add days, hours, seconds, whatever... but those methods never modify the instance's inner value. Instead, they return a new DateTime object because each DateTime instance is immutable.

This also has interesting consequences on object equality. Two Value Objects holding the same data should be considered identical objects, even though they point to different memory locations.

Value Objects are great and can make a lot of things much easier for you. In this post, I'm going to show you how you can easily create a true Value Object while avoiding some common pitfalls.

For this example, I'm going to create an Address class. An Address instance needs to hold some data (street, city, region, postalcode, country, phone number). This data can often be identical between other Entity objects.  Maybe I have a Person class which needs to hold an Address. Two people living in the same house would have the same physical address. Both Person instances should then have the same Address too.  Husband.Address.Equals(Wife.Address) should return true without having to worry about some unnecessary identity of the Address instances.

This is how our first implementation of the Address class would look like:

<script src="https://gist.github.com/3611056.js?file=s1.cs"></script>

So now an Address instance's values can not be changed after object creation. By the way, in case you're wondering why I assign string.Empty instead of allowing null references to my fields: it's because I want to avoid checking for null every time I want to use my fields later on in my code. This is known as the <a href="http://en.wikipedia.org/wiki/Null_Object_pattern">Null Object Pattern</a>.

Now we have to make sure that 2 Address instances holding the same data are actually recognized as equal objects.  First, we'll override the Equals method:

<script src="https://gist.github.com/3611056.js?file=s2.cs"></script>

Thanks to the Null Object Pattern, I don't need null checks when accessing my fields which is great because I really dislike null checks all over the place.  Anyway, back to the subject at hand: Equals will return true if the values of the instances are identical.

If you override the Equals method, you really should override the GetHashCode() method as well:

<script src="https://gist.github.com/3611056.js?file=s3.cs"></script>

Ok, our Address instances will behave properly when they are compared with the Equals method. A lot of developers prefer to use the == and != operators though, so we should make sure that these operators also compare the actual values instead of the default reference equality check.  For that, we'll simply overload the operators:

<script src="https://gist.github.com/3611056.js?file=s4.cs"></script>

In the == overload, we need to check if the values we've received aren't null. If one of them is null, and the other isn't, then they obviously can't be equal.  But how are you going to check if they're null? You can't use address1 == null or adress2 == null because then you'd go straight back into this == overload which would cause a stack overflow. So we use Object.ReferenceEquals() to do the check for us. If neither of the objects is null, we simply return the result of calling the Equals method.

We now have a true Value Object. Its values are immutable, and the instances will behave properly when checked for equality.  The following tests all pass:

<script src="https://gist.github.com/3611056.js?file=s5.cs"></script>