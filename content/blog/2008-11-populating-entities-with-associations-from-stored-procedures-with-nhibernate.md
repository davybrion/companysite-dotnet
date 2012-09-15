In response to my last post where i showed how you could <a href="/blog/2008/11/populating-entities-from-stored-procedures-with-nhibernate/">fill entities with the resultset of a stored procedure</a>, i was asked if it was also possible to fill entities and their associations if the stored procedure returned all of the necessary data.  I looked into it, and it's possible, although it did take me some time to figure out how to actually do it.

First of all, here's the modified stored procedure:

<script src="https://gist.github.com/3684093.js?file=s1.sql"></script>

As you can see, this returns all of the columns of the Products table, as well as the columns of the Categories table.  The goal is to let NHibernate execute this stored procedure, and use the returning data to give us a list of Product entities with a Category reference which is already set up with the proper data. 

The mapping of the named query now looks like this:

<script src="https://gist.github.com/3684093.js?file=s2.xml"></script>

We map each column of the Product table to its correct property of the Product class.  Notice that we defined the 'Product' alias for this part of the data.  Then we use the return-join element to map the joined properties to the 'Product.Category' property.  This might look a bit weird at first.  You have to specify the alias of the owning object (which in our case is the 'Product' alias), and then you need to specify the name of the property of the owning object upon which the other part of the data should be mapped (in our case, the 'Category' property of the 'Product' object).

Now we can retrieve the data like this:

<script src="https://gist.github.com/3684093.js?file=s3.cs"></script>

I first tried to use the IQuery's generic List of T method which i had hoped would give me a generic list of Product entities.  But i couldn't get that working. So i tried the regular List method, and it turns out that NHibernate doesn't just give me a list of Product entities... it gives me a list where each item in the list is an object array where the first item in the array is the Product entity, and the second item is the Category.  Each Product entity's Category property references the correct Category instance though.  So you can get the product instances like this:

<script src="https://gist.github.com/3684093.js?file=s4.cs"></script>

There's probably an easier way to just get the list of Product entities from the named query, but i haven't found it yet :)