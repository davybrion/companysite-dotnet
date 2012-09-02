Dependency Injection (DI) is an incredibly useful and easy technique which makes your code a lot easier to test (that's not the only benefit though).  But i've noticed that there are still plenty of developers who don't know what it is, or have heard of it but don't know how to use it, etc... Hopefully, it'll be somewhat clear after reading this :)

I like to use 'real' examples so i'll try to explain DI based on some code i wrote for Noma yesterday. I have a SqlMetaDataProvider class which needs to provide me with meta data coming from a SQL Server database.  It retrieves the meta data from the database in a relational structure (a DataSet) and then converts it to an easy to use object model.  Obviously, i want to be able to test this class without actually going to the database because that would make my tests slow.  So how can we test if the relational data is being converted to the object model without going to the database?

Well, let's look at what the class does. First of all, it retrieves sql server meta data. Then it converts it to an object model.  But retrieving the meta data doesn't really belong here... it should be functionality that's offered by another class.  So we create a SqlDataRetriever class.  All it will do is return meta data in the relational structure. Nothing more. So now, our SqlMetaDataProvider class can simply use the SqlDataRetriever class to retrieve the meta data.  So basically, SqlDataRetriever is now a dependency of the SqlMetaDataProvider class because SqlMetaDataProvider is depending on SqlDataRetriever to return the relational meta data.

At this point, our class could look like this:

<div>
[csharp]
public class SqlMetaDataProvider : IMetaDataProvider
{
	private readonly string _connectionString;
	private readonly SqlDataRetriever _sqlDataRetriever;
 
	public SqlMetaDataProvider(string connectionString)
	{
		_connectionString = connectionString;
		_sqlDataRetriever = new SqlDataRetriever();
	}
 
	public MetaDataStore GetMetaDataStore()
	{
		SqlMetaData sqlMetaData = _sqlDataRetriever.GetMetaData(_connectionString);

		return ConvertToMetaDataStore(sqlMetaData);
	}

	private MetaDataStore ConvertToMetaDataStore(SqlMetaData sqlMetaData)
	{
		MetaDataStore store = new MetaDataStore();

		AddTablesToStore(sqlMetaData.TableInfo, store);
		AddColumnsToTablesInStore(sqlMetaData.ColumnInfo, store);
		CreateRelationshipsBetweenTables(sqlMetaData.RelationshipInfo, store);

		return store;
	}
}
[/csharp]
</div>

Note: I left out the code for the AddTablesToStore, AddColumnsToTablesInStore and CreateRelationshipsBetweenTables methods because they aren't relevant to this specific topic.

Now we need to make sure we can replace the instance of SqlDataRetriever during testing with one we can supply ourselves. That test instance could then simply return a DataSet that was created in-memory, thus keeping our tests running fast. Notice how SqlMetaDataProvider has a reference of the type SqlDataRetriever. The type is essentially fixed, which creates a strong dependency on the SqlDataRetriever class.  If we were to replace the type of the reference with an interface, it would at least make it easier to use another type for our required dependency, one that simply implements the interface.

So we create the ISqlDataRetriever interface:

<div>
[csharp]
public interface ISqlDataRetriever
{
	SqlMetaData GetMetaData(string connectionString);
}
[/csharp]
</div>

And then we modify the definition of SqlDataRetriever to implement the interface:

<div>
[csharp]
public class SqlDataRetriever : ISqlDataRetriever
[/csharp]
</div>

Now we need to modify our SqlMetaDataProvider class so it holds a reference to the interface type, instead of the class type:

<div>
[csharp]
	private readonly ISqlDataRetriever _sqlDataRetriever;
[/csharp]
</div>

We still need to find a way to inject our dependency into our SqlMetaDataProvider so we'll modify the constructor:

<div>
[csharp]
public SqlMetaDataProvider(string connectionString, ISqlDataRetriever sqlDataRetriever)
{
    _connectionString = connectionString;
    _sqlDataRetriever = sqlDataRetriever;
}
[/csharp]
</div>

The only downside to this is that it now takes more work to create an instance of SqlMetaDataProvider... work that clients shouldn't need to do if they just want to use the default ISqlDataRetriever implementation.  If you're using an Inversion Of Control (IoC) container, you can simply request an instance of SqlMetaDataProvider and the IoC container would also create the necessary dependency for you.  Using an IoC container however is outside of the scope for this post, so we won't do that. In fact, if you know that your production code will always use the SqlDataRetriever implementation, you could also provide a simpler  constructor which takes care of that for you:

<div>
[csharp]
public SqlMetaDataProvider(string connectionString)
        : this(connectionString, new SqlDataRetriever()) {}
[/csharp]
</div>

So you could use the simpler constructor in your production code, and the other one in your test code.  Speaking of test code, we still need to write that test which tests the conversion without hitting the database.  First, we need to create an implementation of ISqlDataRetriever which allows us to pass a DataSet to it which the ISqlDataRetriever instance should return to its consumer (our SqlMetaDataProvider):

<div>
[csharp]
    public class SqlDataProviderStub : ISqlDataRetriever
    {
        private SqlMetaData _sqlMetaData;
 
        public SqlMetaData SqlMetaData
        {
            set { _sqlMetaData = value; }
        }
 
        SqlMetaData ISqlDataRetriever.GetMetaData(string connectionString)
        {
            return _sqlMetaData;
        }
    }            
[/csharp]
</div>

And finally, the test:

<div>
[csharp]
        [Test]
        public void GetMetaDataStore_ProvideDataSetWithTwoTablesAndRelationship_MetaDataStoreIsCorrect()
        {
            SqlMetaData sqlMetaData = PrepareMetaDataSetInMemoryWithTestData();
 
            SqlDataProviderStub sqlDataProvider = new SqlDataProviderStub();
            sqlDataProvider.SqlMetaData = sqlMetaData;
 
            // pass null as the connectionString, and pass our SqlDataProviderStub
            IMetaDataProvider metaDataProvider = new SqlMetaDataProvider(null, sqlDataProvider);
 
            MetaDataStore store = metaDataProvider.GetMetaDataStore();
 
            AssertStoreContainsOurTestData(store);
        }
[/csharp]
</div>

Mission accomplished :)
