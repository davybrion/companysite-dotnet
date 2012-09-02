How often do you see entities mapped with getters and setters for every property, and only a default constructor (either added implicitly by the compiler or explicitly by a developer)?  It's not really the best way to map entities, so i just wanted to show a better way of doing this.

Consider the OrderLine entity.  It has 4 required properties: Order, Product, UnitPrice and Quantity.  It also has one optional property called DiscountPercentage.  The Order and Product properties should never be changed after the OrderLine was created.  It also has a database Id property which should never be changed either.

This is how the code of the OrderLine class would look like:

<div>
[csharp]
    public class OrderLine : IIdentifiable&lt;int&gt;
    {
        public OrderLine(Order order, Product product, decimal unitPrice, int quantity)
        {
            if (order == null) throw new ArgumentNullException(&quot;order&quot;);
            if (product == null) throw new ArgumentNullException(&quot;product&quot;);
 
            this.order = order;
            this.product = product;
            UnitPrice = unitPrice;
            Quantity = quantity;
        }
 
        // required for NH
        protected OrderLine() {}
 
        private int id;
 
        public virtual int Id
        {
            get { return id; }
        }
 
        private Order order;
 
        public virtual Order Order
        {
            get { return order; }
        }
 
        private Product product;
 
        public virtual Product Product
        {
            get { return product; }
        }
 
        public virtual decimal UnitPrice { get; set; }
        public virtual int Quantity { get; set; }
        public virtual double? DiscountPercentage { get; set; }
    }
[/csharp]
</div>

There is only one public constructor, which takes all of the required properties as parameters.  The protected constructor is only there because NHibernate needs it to create run-time proxies (which enable all of the lazy-loading magic).  In theory, you can't create instances of the OrderLine entity without its required data.

Also, notice how the Id, Order and Product properties only have a getter, and no setter.  These values can no longer be changed by developers once the object is constructed.  The UnitPrice and Quantity properties do have setters, because these values can be modified after the entity is created.

The mapping for this class looks like this:

<div>
[xml]
  &lt;class name=&quot;OrderLine&quot; table=&quot;OrderLine&quot; &gt;
    &lt;id name=&quot;Id&quot; column=&quot;Id&quot; type=&quot;int&quot; access=&quot;nosetter.camelcase&quot; &gt;
      &lt;generator class=&quot;identity&quot; /&gt;
    &lt;/id&gt;
 
    &lt;many-to-one name=&quot;Order&quot; column=&quot;OrderId&quot; class=&quot;Order&quot; not-null=&quot;true&quot; access=&quot;nosetter.camelcase&quot; /&gt;
    &lt;many-to-one name=&quot;Product&quot; column=&quot;ProductId&quot; class=&quot;Product&quot; not-null=&quot;true&quot; access=&quot;nosetter.camelcase&quot; /&gt;
    &lt;property name=&quot;UnitPrice&quot; column=&quot;UnitPrice&quot; type=&quot;Decimal&quot; not-null=&quot;true&quot; /&gt;
    &lt;property name=&quot;Quantity&quot; column=&quot;Quantity&quot; type=&quot;int&quot; not-null=&quot;true&quot; /&gt;
    &lt;property name=&quot;DiscountPercentage&quot; column=&quot;DiscountPercentage&quot; type=&quot;double&quot; /&gt;
  &lt;/class&gt;
[/xml]
</div>

It's pretty easy... each property that shouldn't be changed after creation is mapped with the nosetter.camelcase access strategy.  That means NHibernate uses the private field to set the value directly after creation,  but will use the getters whenever it needs to read the data from the entity.

As you can see, without too much trouble you can make sure that your entities always have their required data, and that properties that shouldn't change after creation can't be modified either.