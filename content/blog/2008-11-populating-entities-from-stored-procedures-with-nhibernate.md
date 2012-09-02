A short while ago we needed to fetch the data for some entities through a stored procedure for performance reasons.  We already use NHibernate in the typical way to fetch and modify the data of this entity type, but we just wanted something so we could also use the resultset of the stored procedure to populate the entities.  One of my team members spent some time figuring out how to get the data returned by the stored procedure into the entities without actually having to write the code ourselves.  Turns out this was pretty easy to do.  Let's go over the solution with a very simple example.

The stored procedure i'll use for the example is extremely simple, and you'd never need to use this technique for such a stupid procedure.  But in the situation we faced at work, the stored procedure was obviously a lot more complicated.  So the stored procedure for this example is just this:

<div>
[sql]
ALTER PROCEDURE [dbo].[GetProductsByCategoryId]
    @CategoryId int
AS
BEGIN
    SET NOCOUNT ON;
 
    SELECT [ProductID]
          ,[ProductName]
          ,[SupplierID]
          ,[CategoryID]
          ,[QuantityPerUnit]
          ,[UnitPrice]
          ,[UnitsInStock]
          ,[UnitsOnOrder]
          ,[ReorderLevel]
          ,[Discontinued]
      FROM [Northwind].[dbo].[Products]
     WHERE [CategoryId] = @CategoryId
END
[/sql]
</div>

This just returns the product rows for the given CategoryId parameter.  Again, you'd never do this in real life but this simple procedure is just used as an example.

Now, the structure of the resultset that this procedure returns is identical to the structure that the Product entity is mapped to.  This makes it really easy to get this data into the Product entities.  Just add a named query to your mapping like this:

<div>
[xml]
  &lt;sql-query name=&quot;GetProductsByCategoryId&quot;&gt;
    &lt;return class=&quot;Product&quot; /&gt;
    exec dbo.GetProductsByCategoryId :CategoryId
  &lt;/sql-query&gt;
[/xml]
</div>

And this is all you need to do in code to get your list of entities from this stored procedure:

<div>
[csharp]
            IQuery query = Session.GetNamedQuery(&quot;GetProductsByCategoryId&quot;);
            query.SetInt32(&quot;CategoryId&quot;, 1);
            IList&lt;Product&gt; products = query.List&lt;Product&gt;();
[/csharp]
</div>

Is that easy or what?

Now, suppose that the stored procedure returns more columns than you've got mapped to the entity.  You can still use this approach as well, but then you'll need to specify which return values map to which properties in the entity like this:

<div>
[xml]
  &lt;sql-query name=&quot;GetProductsByCategoryId&quot;&gt;
    &lt;return class=&quot;Product&quot;&gt;
      &lt;return-property column=&quot;ProductID&quot; name=&quot;Id&quot; /&gt;
      &lt;return-property column=&quot;ProductName&quot; name=&quot;Name&quot; /&gt;
      &lt;return-property column=&quot;SupplierID&quot; name=&quot;Supplier&quot; /&gt;
      &lt;return-property column=&quot;CategoryID&quot; name=&quot;Category&quot; /&gt;
      &lt;return-property column=&quot;QuantityPerUnit&quot; name=&quot;QuantityPerUnit&quot; /&gt;
      &lt;return-property column=&quot;UnitPrice&quot; name=&quot;UnitPrice&quot; /&gt;
      &lt;return-property column=&quot;UnitsInStock&quot; name=&quot;UnitsInStock&quot; /&gt;
      &lt;return-property column=&quot;UnitsOnOrder&quot; name=&quot;UnitsOnOrder&quot; /&gt;
      &lt;return-property column=&quot;ReorderLevel&quot; name=&quot;ReorderLevel&quot; /&gt;
      &lt;return-property column=&quot;Discontinued&quot; name=&quot;Discontinued&quot; /&gt;
    &lt;/return&gt;
    exec dbo.GetProductsByCategoryId :CategoryId
  &lt;/sql-query&gt;
[/xml]
</div>

Notice how the CategoryID and SupplierID columns are mapped to Category and Supplier properties, which in Product's mapping are mapped as Category and Supplier many-to-one types, so basically references of type Category and Supplier respectively.  NHibernate basically just takes care of all of the dirty work.