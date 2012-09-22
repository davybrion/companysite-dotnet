How often do you see entities mapped with getters and setters for every property, and only a default constructor (either added implicitly by the compiler or explicitly by a developer)?  It's not really the best way to map entities, so I just wanted to show a better way of doing this.

Consider the OrderLine entity.  It has 4 required properties: Order, Product, UnitPrice and Quantity.  It also has one optional property called DiscountPercentage.  The Order and Product properties should never be changed after the OrderLine was created.  It also has a database Id property which should never be changed either.

This is how the code of the OrderLine class would look like:

<script src="https://gist.github.com/3684391.js?file=s1.cs"></script>

There is only one public constructor, which takes all of the required properties as parameters.  The protected constructor is only there because NHibernate needs it to create run-time proxies (which enable all of the lazy-loading magic).  In theory, you can't create instances of the OrderLine entity without its required data.

Also, notice how the Id, Order and Product properties only have a getter, and no setter.  These values can no longer be changed by developers once the object is constructed.  The UnitPrice and Quantity properties do have setters, because these values can be modified after the entity is created.

The mapping for this class looks like this:

<script src="https://gist.github.com/3684391.js?file=s2.xml"></script>

It's pretty easy... each property that shouldn't be changed after creation is mapped with the nosetter.camelcase access strategy.  That means NHibernate uses the private field to set the value directly after creation,  but will use the getters whenever it needs to read the data from the entity.

As you can see, without too much trouble you can make sure that your entities always have their required data, and that properties that shouldn't change after creation can't be modified either.