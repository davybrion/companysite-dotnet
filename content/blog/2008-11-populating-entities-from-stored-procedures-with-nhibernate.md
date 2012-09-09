A short while ago we needed to fetch the data for some entities through a stored procedure for performance reasons.  We already use NHibernate in the typical way to fetch and modify the data of this entity type, but we just wanted something so we could also use the resultset of the stored procedure to populate the entities.  One of my team members spent some time figuring out how to get the data returned by the stored procedure into the entities without actually having to write the code ourselves.  Turns out this was pretty easy to do.  Let's go over the solution with a very simple example.

The stored procedure i'll use for the example is extremely simple, and you'd never need to use this technique for such a stupid procedure.  But in the situation we faced at work, the stored procedure was obviously a lot more complicated.  So the stored procedure for this example is just this:

<script src="https://gist.github.com/3684087.js?file=s1.sql"></script>

This just returns the product rows for the given CategoryId parameter.  Again, you'd never do this in real life but this simple procedure is just used as an example.

Now, the structure of the resultset that this procedure returns is identical to the structure that the Product entity is mapped to.  This makes it really easy to get this data into the Product entities.  Just add a named query to your mapping like this:

<script src="https://gist.github.com/3684087.js?file=s2.xml"></script>

And this is all you need to do in code to get your list of entities from this stored procedure:

<script src="https://gist.github.com/3684087.js?file=s3.cs"></script>

Is that easy or what?

Now, suppose that the stored procedure returns more columns than you've got mapped to the entity.  You can still use this approach as well, but then you'll need to specify which return values map to which properties in the entity like this:

<script src="https://gist.github.com/3684087.js?file=s4.xml"></script>

Notice how the CategoryID and SupplierID columns are mapped to Category and Supplier properties, which in Product's mapping are mapped as Category and Supplier many-to-one types, so basically references of type Category and Supplier respectively.  NHibernate basically just takes care of all of the dirty work.