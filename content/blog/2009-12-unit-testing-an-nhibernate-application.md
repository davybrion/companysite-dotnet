<a href="http://grantpalin.com/blog/" target="_blank">Grant Palin</a> recently asked me for an in-depth article on TDD’ing an NHibernate application. While this post won’t be very in-depth, it might be helpful already. There are basically two approaches that I've seen used with good results, though there are obviously more approaches that you can use. I'm going to limit the scope of this post to the following two approaches though, and I'll also discuss exactly what we test.

## Creating a new database for each test (or testfixture)

This approach creates the database at the beginning of each test (or testfixture), runs the test (or tests in the fixture), and then destroys the database after the test (or testfixture) completed. The easiest way to do this is to create a base test class that all of you data access test class should inherit from. Here’s a simple example:

<script src="https://gist.github.com/3685715.js?file=s1.cs"></script>

This class will create the database from scratch once for each TestFixture, which means that each test in the fixture will use the same database. It also destroys the database at the end of the fixture. It creates the database based on your mappings, and as you can see, you really don’t have to do a lot for this. If you want the database to be recreated and dropped for each test, then you obviously need to move the code in the TestFixtureSetup and TestFixtureTearDown methods to your regular SetUp and TearDown methods. If you go that route, I'd advise you to include empty template methods before and after the setup and teardown so you can plug in some extra code before and after these operations in your derived test classes.

The biggest benefit of this approach is that you don’t have any possibly present state in the database that can influence your tests. The downside is that you can’t rely on certain data (eg reference data) to be present and you have to recreate it whenever you need it. You can also use multiple transactions in your tests, though you are also responsible for cleaning any data that is left in the database at the end of each test. You also need to guarantee that this always happens because any data that is left by one test might influence another one. Also, if you leave data in the database, that might lead to problems when dropping the database, so you really need to be careful with this.

Another problem with this example is that there is no automatic way to push the ISession instance to your data access components. That could easily be added though, depending on how your data access components retrieve a reference to a valid ISession.  

## Tests that automatically roll back their transactions

This is the approach that we always use at work. These tests require your database to be set up in a valid manner before your tests begin. Each test uses one transaction, which is automatically rolled back at the end of the test to prevent the possibility of any test data remaining in the database. With this way of testing, it’s also possible to provide some kind of ‘known state’ in the database (eg reference data) that you can use from within your code.

Here’s the NHibernateTest class that our NHibernate test classes all inherit from:

<script src="https://gist.github.com/3685715.js?file=s2.cs"></script>

Now, this example uses our UnitOfWork and ActiveSessionManager classes to make sure that our data access components can access the current ISession instance. Each test has a valid ISession present, which has already created a transaction and we can create/modify/delete data, run our queries, modify some stuff again, run our queries again and perform our assertions, all in the same transaction. After the test is completed, the transaction is never committed (and thus, automatically rolled back) so none of that data ever remains in the database.

## What Exactly Do We Test?

Well, we test everything basically. We test all of our CRUD operations (again, with a simple base class which only requires you to implement the BuildEntity, ModifyEntity and AssertEqual methods and does all of the operations and checks automatically) for each entity. That’s right, <em>for each entity.</em> The extra work that this requires really doesn’t take a lot of time and it lets us know for a fact that our mappings are valid.

We also test every custom query that we write, always using the following approach: create some test data, flush the session, perform your query and verify that the expected data has indeed been returned by the query, and also that data that shouldn’t have been returned isn’t there.

And that’s pretty much it. We mock the data layer in all of our other tests, so our CRUD and query tests are the only ones that actually use NHibernate and the database. But CRUD actions and queries are tested very extensively.