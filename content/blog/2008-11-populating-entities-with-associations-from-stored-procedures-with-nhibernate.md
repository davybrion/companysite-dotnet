In response to my last post where i showed how you could <a href="http://davybrion.com/blog/2008/11/populating-entities-from-stored-procedures-with-nhibernate/">fill entities with the resultset of a stored procedure</a>, i was asked if it was also possible to fill entities and their associations if the stored procedure returned all of the necessary data.  I looked into it, and it's possible, although it did take me some time to figure out how to actually do it.

First of all, here's the modified stored procedure:

<div>
[sql]
ALTER PROCEDURE [dbo].[GetProductsByCategoryId]
    @CategoryId int
AS
BEGIN
    SET NOCOUNT ON;
 
    SELECT [Products].[ProductID] as &quot;Product.ProductID&quot;
          ,[Products].[ProductName] as &quot;Product.ProductName&quot;
          ,[Products].[SupplierID] as &quot;Product.SupplierID&quot;
          ,[Products].[CategoryID] as &quot;Product.CategoryID&quot;
          ,[Products].[QuantityPerUnit] as &quot;Product.QuantityPerUnit&quot;
          ,[Products].[UnitPrice] as &quot;Product.UnitPrice&quot;
          ,[Products].[UnitsInStock] as &quot;Product.UnitsInStock&quot;
          ,[Products].[UnitsOnOrder] as &quot;Product.UnitsOnOrder&quot;
          ,[Products].[ReorderLevel] as &quot;Product.ReorderLevel&quot;
          ,[Products].[Discontinued] as &quot;Product.Discontinued&quot;
          ,[Categories].[CategoryID] as &quot;Category.CategoryID&quot;
          ,[Categories].[CategoryName] as &quot;Category.CategoryName&quot;
          ,[Categories].[Description] as &quot;Category.Description&quot;
      FROM [Northwind].[dbo].[Products]
            inner join [Northwind].[dbo].[Categories]
                on [Products].[CategoryID] = [Categories].[CategoryID]
     WHERE [Products].[CategoryId] = @CategoryId
END
[/sql]
</div>

As you can see, this returns all of the columns of the Products table, as well as the columns of the Categories table.  The goal is to let NHibernate execute this stored procedure, and use the returning data to give us a list of Product entities with a Category reference which is already set up with the proper data. 

The mapping of the named query now looks like this:

<div>
[xml]
  &lt;sql-query name=&quot;GetProductsByCategoryId&quot;&gt;
    &lt;return alias=&quot;Product&quot; class=&quot;Product&quot;&gt;
      &lt;return-property column=&quot;Product.ProductID&quot; name=&quot;Id&quot; /&gt;
      &lt;return-property column=&quot;Product.ProductName&quot; name=&quot;Name&quot; /&gt;
      &lt;return-property column=&quot;Product.CategoryId&quot; name=&quot;Category&quot; /&gt;
      &lt;return-property column=&quot;Product.SupplierID&quot; name=&quot;Supplier&quot; /&gt;
      &lt;return-property column=&quot;Product.QuantityPerUnit&quot; name=&quot;QuantityPerUnit&quot; /&gt;
      &lt;return-property column=&quot;Product.UnitPrice&quot; name=&quot;UnitPrice&quot; /&gt;
      &lt;return-property column=&quot;Product.UnitsInStock&quot; name=&quot;UnitsInStock&quot; /&gt;
      &lt;return-property column=&quot;Product.UnitsOnOrder&quot; name=&quot;UnitsOnOrder&quot; /&gt;
      &lt;return-property column=&quot;Product.ReorderLevel&quot; name=&quot;ReorderLevel&quot; /&gt;
      &lt;return-property column=&quot;Product.Discontinued&quot; name=&quot;Discontinued&quot; /&gt;
    &lt;/return&gt;
    &lt;return-join alias=&quot;Category&quot; property=&quot;Product.Category&quot;&gt;
      &lt;return-property column=&quot;Category.CategoryId&quot; name=&quot;Id&quot; /&gt;
      &lt;return-property column=&quot;Category.CategoryName&quot; name=&quot;Name&quot; /&gt;
      &lt;return-property column=&quot;Category.Description&quot; name=&quot;Description&quot; /&gt;
    &lt;/return-join&gt;
    exec dbo.GetProductsByCategoryId :CategoryId
  &lt;/sql-query&gt;
[/xml]
</div>

We map each column of the Product table to its correct property of the Product class.  Notice that we defined the 'Product' alias for this part of the data.  Then we use the return-join element to map the joined properties to the 'Product.Category' property.  This might look a bit weird at first.  You have to specify the alias of the owning object (which in our case is the 'Product' alias), and then you need to specify the name of the property of the owning object upon which the other part of the data should be mapped (in our case, the 'Category' property of the 'Product' object).

Now we can retrieve the data like this:

<div>
[csharp]
            IQuery query = Session.GetNamedQuery(&quot;GetProductsByCategoryId&quot;);
            query.SetInt32(&quot;CategoryId&quot;, 1);
            IList results = query.List();
[/csharp]
</div>

I first tried to use the IQuery's generic List of T method which i had hoped would give me a generic list of Product entities.  But i couldn't get that working. So i tried the regular List method, and it turns out that NHibernate doesn't just give me a list of Product entities... it gives me a list where each item in the list is an object array where the first item in the array is the Product entity, and the second item is the Category.  Each Product entity's Category property references the correct Category instance though.  So you can get the product instances like this:

<div>
[csharp]
            IEnumerable&lt;Product&gt; products = results.Cast&lt;Object[]&gt;().Select(i =&gt; (Product)i[0]);
[/csharp]
</div>

There's probably an easier way to just get the list of Product entities from the named query, but i haven't found it yet :)