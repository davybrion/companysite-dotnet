This <a href="http://blogs.msdn.com/efdesign/archive/2009/03/16/foreign-keys-in-the-entity-framework.aspx">post</a> from the Entity Framework team recently caught my attention.  It discusses the ability to add actual foreign key values to your entities instead of just references to the referred entities.  One of the benefits of this ability is that you can assign foreign key values to an entity's properties without having to actually retrieve the entity you are referring to.  While i am no fan of this approach, i do want to point out that you can do this with NHibernate too, especially because some people don't know about this.

Take a look at the following code:

<script src="https://gist.github.com/3684452.js?file=s1.cs"></script>

This code changes the product's Category property, and to do that it retrieves the actual ProductCategory instance through the id value of the category.  This causes 2 database hits.  One to retrieve the ProductCategory, and one to persist the Product.

You could do this instead:

<script src="https://gist.github.com/3684452.js?file=s2.cs"></script>

Notice how we use ISession's Load method here, instead of the Get method to 'retrieve' the ProductCategory.  The Get method actually fetches the entity from the database if it's not already in the session cache.  The Load method however will return an uninitialized proxy to the ProductCategory entity if it's not present in the session cache.  The NHibernateUtil.IsInitialized() method will return false, because this proxy is indeed uninitialized.  It does not hit the database until you try to access any of the properties of the ProductCategory proxy, except for its identifier property.  So accessing product.Category.Id would not hit the database, but product.Category.Name or product.Category.Description would.  

If you want to avoid hitting the database to assign foreign keys, using a proxy might be an interesting alternative for you. 