I'm working on some NHibernate examples (which will be publicly available in a few weeks) and i needed some random test data. Luckily for me, i could just use Mark Meyers' <a href="http://code.google.com/p/quickgenerate/">QuickGenerate</a> project to do the job for me.

Check out this code:

<div>
[csharp]
    public static class TestData
    {
        private static DomainGenerator WithAddress(DomainGenerator generator)
        {
            return generator
                .With&lt;Address&gt;(options =&gt; options.For(address =&gt; address.Street, new StringGenerator(1, 100)))
                .With&lt;Address&gt;(options =&gt; options.For(address =&gt; address.City, new StringGenerator(1, 100)))
                .With&lt;Address&gt;(options =&gt; options.For(address =&gt; address.Country, new StringGenerator(1, 100)));
        }

        private static DomainGenerator EmployeeGenerator(ISession session)
        {
            return WithAddress(new DomainGenerator())
                .With&lt;Employee&gt;(options =&gt; options.Ignore(employee =&gt; employee.Id))
                .OneToOne&lt;Employee, Address&gt;((employee, address) =&gt; employee.Address = address)
                .With&lt;Employee&gt;(options =&gt; options.For(employee =&gt; employee.FirstName, new StringGenerator(1, 50)))
                .With&lt;Employee&gt;(options =&gt; options.For(employee =&gt; employee.LastName, new StringGenerator(1, 75)))
                .With&lt;Employee&gt;(options =&gt; options.For(employee =&gt; employee.Title, new StringGenerator(1, 50)))
                .With&lt;Employee&gt;(options =&gt; options.For(employee =&gt; employee.Phone, new StringGenerator(1, 15)))
                .ForEach&lt;Employee&gt;(employee =&gt; session.Save(employee));
        }

        public static void Create(ISession session)
        {
            var customers = WithAddress(new DomainGenerator())
                .With&lt;Customer&gt;(options =&gt; options.Ignore(customer =&gt; customer.Id))
                .OneToOne&lt;Customer, Address&gt;((customer, address) =&gt; customer.Address = address)
                .With&lt;Customer&gt;(options =&gt; options.For(customer =&gt; customer.DiscountPercentage, new DoubleGenerator(0, 25)))
                .ForEach&lt;Customer&gt;(customer =&gt; session.Save(customer))
                .Many&lt;Customer&gt;(20, 40)
                .ToArray();

            var managers = EmployeeGenerator(session).Many&lt;Employee&gt;(2);

            var employees = EmployeeGenerator(session)
                .ForEach&lt;Employee&gt;(employee =&gt; Maybe.Do(() =&gt; managers.PickOne().AddSubordinate(employee)))
                .Many&lt;Employee&gt;(20)
                .ToArray();

            var suppliers = WithAddress(new DomainGenerator())
                .With&lt;Supplier&gt;(options =&gt; options.Ignore(supplier =&gt; supplier.Id))
                .OneToOne&lt;Supplier, Address&gt;((supplier, address) =&gt; supplier.Address = address)
                .With&lt;Supplier&gt;(options =&gt; options.For(supplier =&gt; supplier.Website, new StringGenerator(1, 100)))
                .Many&lt;Supplier&gt;(20)
                .ToArray();

            var products = new DomainGenerator()
                .With&lt;ProductSource&gt;(options =&gt; options.Ignore(productsource =&gt; productsource.Id))
                .ForEach&lt;ProductSource&gt;(productsource =&gt; session.Save(productsource))
                .With&lt;Product&gt;(options =&gt; options.Ignore(product =&gt; product.Id))
                .With&lt;Product&gt;(options =&gt; options.Ignore(product =&gt; product.Version))
                .With&lt;Product&gt;(options =&gt; options.For(
                    product =&gt; product.Category,
                    ProductCategory.Beverages,
                    ProductCategory.Condiments,
                    ProductCategory.DairyProducts,
                    ProductCategory.Produce))
                .With&lt;Product&gt;(g =&gt; g.Method&lt;double&gt;(1, 10, (product, d) =&gt; product.AddSource(suppliers.PickOne(), d)))
                .With&lt;Product&gt;(options =&gt; options.For(product =&gt; product.Name, new StringGenerator(1, 50)))
                .ForEach&lt;Product&gt;(product =&gt; session.Save(product))
                .Many&lt;Product&gt;(30)
                .ToArray();

            WithAddress(new DomainGenerator())
                .With&lt;OrderItem&gt;(options =&gt; options.Ignore(item =&gt; item.Id))
                .With&lt;OrderItem&gt;(options =&gt; options.For(item =&gt; item.Product, products))
                .With&lt;Order&gt;(options =&gt; options.Ignore(order =&gt; order.Id))
                .OneToMany&lt;Order, OrderItem&gt;(1, 20, (order, item) =&gt; order.AddItem(item))
                .With&lt;Order&gt;(options =&gt; options.For(order =&gt; order.Customer, customers))
                .With&lt;Order&gt;(options =&gt; options.For(order =&gt; order.Employee, employees))
                .OneToOne&lt;Order, Address&gt;((order, address) =&gt; order.DeliveryAddress = address)
                .ForEach&lt;Order&gt;(order =&gt; session.Save(order))
                .Many&lt;Order&gt;(200);

            session.Flush();
        }
    }
[/csharp]
</div>

This populates my test-database with:
<ul>
	<li>Between 20 and 40 customers, which have a DiscountPercentage value between 0 and 25</li>
	<li>2 Managers</li>
	<li>20 employees, some of them assigned to one of the managers, some of them without a manager</li>
	<li>20 suppliers</li>
	<li>30 products, each of which will have between 1 and 10 sources (= link with supplier and a cost)</li>
	<li>200 orders which are linked to the created customers and employees, and contain between 1 and 10 items using any of the 30 products that were also created</li>
</ul>

Pretty nifty and useful :)
Of course, the vast majority of the data is entirely random at this point, but it's <a href="http://kilfour.wordpress.com/2011/02/24/quickgenerate-0-4/">possible to use pre-defined values as well</a>.

Thanks to Mark for fixing both my code as well as his on such short notice ;) 