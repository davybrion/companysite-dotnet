Note: This post is part of a series.  Be sure to read the introduction <a href="/blog/2009/08/build-your-own-data-access-layer-series/">here</a>.

When you need to populate entity instances with data from a database, you need to know which table the data needs to come from, which columns will map to which property on the entity class, and you'll need to deal with a variety of types.  The approach that I've chosen to use tries to make this as simple as possible.  The idea is basically to place an attribute with the name of the table on top of the entity class, and an attribute on each property with the name of the column it maps to.  For foreign keys, I wanted to be able to just use properties of the type of the referenced entity, instead of having foreign keys in my entities.  For these references, we will use an attribute with the name of the foreign key column.

First, we'll need to define these attributes:

<script src="https://gist.github.com/3684945.js?file=s1.cs"></script>

Notice how none of these properties have any indication of types to use.  The .NET type will be inferred automatically, and it will be mapped to a compatible DbType without having to specify these types all over the place.

We will use the following helper class to map .NET types to their respective DbTypes:

<script src="https://gist.github.com/3684945.js?file=s2.cs"></script>

Obviously, more type conversions can be added... these are just the ones I've needed so far.

Once you've placed all the attributes on top of your entities and properties, we can start building a model of all this metadata.  This will all be stored in a MetaDataStore class that I'll show later on in this post.  Having access to the MetaDataStore makes the implementation of some of these metadata types easier, so I have the following abstract class:

<script src="https://gist.github.com/3684945.js?file=s3.cs"></script>

Now we can go over each piece of metadata.  First, the ColumnInfo class:

<script src="https://gist.github.com/3684945.js?file=s4.cs"></script>

As you can see, we have all the information we need to be able to do something with this column.  We have its Name, the .NET type that is used in the mapped class, the DbType and a PropertyInfo reference to its respective property in the mapped class so we can get and set its value.

For references, we need to know something more:

<script src="https://gist.github.com/3684945.js?file=s5.cs"></script>

For a regular column, it's sufficient to know the .NET type of the property and the DbType.  But for a reference, you need to know the actual type of the referenced entity, as well as the .NET type of it's primary key column.  As you can see in the constructor, we retrieve the TableInfo of the referenced entity, and use the .NET type and the DbType of the primary key of the referenced entity.  The PrimaryKey property of a TableInfo class (which I'll show below) is also a ColumnInfo object.  We obviously also store the actual type of the referenced entity.  And of course, we again store a PropertyInfo so we can get/set the value of the reference.

The TableInfo class can now hold all of the information that we need.  We know all about its primary key (through the PrimaryKeyAttribute), its regular properties (through the ColumnAttribute) and its referenced properties (through the ReferenceAttribute).  With all of that information, the TableInfo class is able to build your typical default SQL statements for CRUD functionality:

<script src="https://gist.github.com/3684945.js?file=s6.cs"></script>

This is actually the biggest class in this DAL. I probably should move the building of the SQL statements and providing parameter info into some kind of helper class because this is a bit of a Single Responsability Principle violation.  Speaking of parameter info, I'm using the following helper class to store this information:

<script src="https://gist.github.com/3684945.js?file=s7.cs"></script>

One thing that you may have noticed is that the generated INSERT statement assumes that SQL Server identity-style generators are being used for primary key values.  Not only that, I'm not even trying to target any other database then SQL Server with this DAL.  Those are 2 rather significant shortcomings of this DAL.  First of all, dealing with multiple identifier strategies can become pretty complex pretty fast.  For this DAL, SQL Server Identity primary keys are sufficient but in a lot of cases you will probably want support for assigned identifier strategies, for GUIDs (preferably locally generated with a sequential GUID algorithm), HiLo and maybe even other ones.  If you really want to, you can do all of this yourself, but you'll quickly spend an entire week (or more) to properly implement all of these identifier strategies.

As for only targeting SQL Server, that is sufficient in our scenario but a proper DAL should be able to deal with multiple databases.  Of course, this has a direct impact on a lot of implementation details.  For starters, you'd never be able to just construct a SQL statement directly in your code and you will need something to make sure the correct statements are generated for your specific database.  NHibernate does a pretty nice job of this by providing a strategy-like implementation through its Dialect class and its derivatives.  Also, some of your identifier strategies will be different for each database that you need to support.  If you got a headache just from reading these last 2 paragraphs, just imagine implementing this and getting it all 'right' in a maintainable matter. 

Anyways, back to the topic at hand.  We now have the classes we need to build up our metadata model of all of the tables we need to provide data access functionality for.  Well, we still need something to hold all of this information and to actually build up this model:

<script src="https://gist.github.com/3684945.js?file=s8.cs"></script>

This class gives you the ability to retrieve the TableInfo class for a specfic entity type.  It also allows you to build the metadata model by passing in an assembly.  It will then loop through all of the types in the assembly to discover the types that have a TableAttribute, and it will then build the TableInfo objects with all of the information we need.

And that's all we need to create mappings between tables and our entities.  This wasn't hard, but it's not very powerful either.  We can't define custom user types that our DAL needs to be able to deal with, nor can we define any database inheritance strategies.  Our attributes are all inheritable, so you can use some inheritance with your entities, but you are essentially limited to the Table Per Class inheritance strategy.  Implementing support for the other inheritance strategies would obviously introduce a lot more complexity in the whole mapping aspect.

In the next post, I'll show you how this DAL will use TableInfo's methods to create CRUD statements to offer out-of-the-box CRUD functionality for each mapped entity.