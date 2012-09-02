Note: This post is part of a series.  Be sure to read the introduction <a href="http://davybrion.com/blog/2009/08/build-your-own-data-access-layer-series/">here</a>.

By now we've already covered everything that this DAL has to offer, which admittedly isn't all that much.  All of the classes you've seen so far are pretty good at whey they should do, but nobody in their right mind would want to use any of these things directly in application code.  Any easy-to-use DAL should offer a simple facade which sits on top of the underlying system and makes it very easy to perform the most typical tasks that you need it to perform for you.  You shouldn't need to know about specific classes to be able to use it (that goes for most good frameworks and libraries btw).

So once again, i based my approach on what NHibernate does, and with that the ISession interface was born:

<div>
[csharp]
    public interface ISession : IDisposable
    {
        void Commit();
        void Rollback();
 
        IQuery CreateQuery(string sql);
        IQuery CreateQuery&lt;TEntity&gt;(string whereClause);
 
        TEntity Get&lt;TEntity&gt;(object id);
        IEnumerable&lt;TEntity&gt; FindAll&lt;TEntity&gt;();
 
        TEntity Insert&lt;TEntity&gt;(TEntity entity);
        TEntity Update&lt;TEntity&gt;(TEntity entity);
        void Delete&lt;TEntity&gt;(TEntity entity);
 
        TableInfo GetTableInfoFor&lt;TEntity&gt;();
 
        void ClearCache();
        void RemoveFromCache(object entity);
        void RemoveAllInstancesFromCache&lt;TEntity&gt;();
 
        SqlConnection GetConnection();
        SqlTransaction GetTransaction();
    }
[/csharp]
</div>

Everything that you need to be able to do with this DAL is provided by this single interface.  And the implementation of the Session class is very easy as well, since we can simply delegate pretty much everything to each of the classes we've covered in the other posts in the series:

<div>
[csharp]
    public class Session : ISession
    {
        private readonly string connectionString;
        private SqlConnection connection;
        private SqlTransaction transaction;
        private readonly MetaDataStore metaDataStore;
        private readonly EntityHydrater hydrater;
        private readonly SessionLevelCache sessionLevelCache;
 
        public Session(string connectionString, MetaDataStore metaDataStore)
        {
            this.connectionString = connectionString;
            this.metaDataStore = metaDataStore;
            sessionLevelCache = new SessionLevelCache();
            hydrater = new EntityHydrater(metaDataStore, this, sessionLevelCache);
        }
 
        private void InitializeConnection()
        {
            connection = new SqlConnection(connectionString);
            connection.Open();
            transaction = connection.BeginTransaction();
        }
 
        public SqlConnection GetConnection()
        {
            if (connection == null)
            {
                InitializeConnection();
            }
 
            return connection;
        }
 
        public SqlTransaction GetTransaction()
        {
            if (transaction == null)
            {
                InitializeConnection();
            }
 
            return transaction;
        }
 
        public IQuery CreateQuery(string sql)
        {
            var command = GetConnection().CreateCommand();
            command.Transaction = GetTransaction();
            command.CommandText = sql;
            return new Query(command, metaDataStore, hydrater);
        }
 
        public IQuery CreateQuery&lt;TEntity&gt;(string whereClause)
        {
            return CreateQuery(metaDataStore.GetTableInfoFor&lt;TEntity&gt;().GetSelectStatementForAllFields() + &quot; &quot; + whereClause);
        }
 
        public TableInfo GetTableInfoFor&lt;TEntity&gt;()
        {
            return metaDataStore.GetTableInfoFor&lt;TEntity&gt;();
        }
 
        public void Commit()
        {
            transaction.Commit();
        }
 
        public void Rollback()
        {
            transaction.Rollback();
        }
 
        public void Dispose()
        {
            if (transaction != null) transaction.Dispose();
            if (connection != null) connection.Dispose();
        }
 
        private TAction CreateAction&lt;TAction&gt;()
            where TAction : DatabaseAction
        {
            return (TAction)Activator.CreateInstance(typeof(TAction), GetConnection(), GetTransaction(),
                metaDataStore, hydrater, sessionLevelCache);
        }
 
        public TEntity Get&lt;TEntity&gt;(object id)
        {
            return CreateAction&lt;GetByIdAction&gt;().Get&lt;TEntity&gt;(id);
        }
 
        public IEnumerable&lt;TEntity&gt; FindAll&lt;TEntity&gt;()
        {
            return CreateAction&lt;FindAllAction&gt;().FindAll&lt;TEntity&gt;();
        }
 
        public TEntity Insert&lt;TEntity&gt;(TEntity entity)
        {
            return CreateAction&lt;InsertAction&gt;().Insert(entity);
        }
 
        public TEntity Update&lt;TEntity&gt;(TEntity entity)
        {
            return CreateAction&lt;UpdateAction&gt;().Update(entity);
        }
 
        public void Delete&lt;TEntity&gt;(TEntity entity)
        {
            CreateAction&lt;DeleteAction&gt;().Delete(entity);
        }
 
        public void InitializeProxy(object proxy, Type targetType)
        {
            CreateAction&lt;InitializeProxyAction&gt;().InitializeProxy(proxy, targetType);
        }
 
        public void ClearCache()
        {
            sessionLevelCache.ClearAll();
        }
 
        public void RemoveFromCache(object entity)
        {
            sessionLevelCache.Remove(entity);
        }
 
        public void RemoveAllInstancesFromCache&lt;TEntity&gt;()
        {
            sessionLevelCache.RemoveAllInstancesOf(typeof(TEntity));
        }
    }
[/csharp]
</div>

As you can see, there's nothing special here and it's all very straightforward.  Application code can now perform database operations pretty easily once it has a reference to an ISession instance.  And obtaining a reference to an ISession instance can be done through the ISessionFactory interface:

<div>
[csharp]
    public interface ISessionFactory
    {
        ISession CreateSession();
    }
[/csharp]
</div>

And its implementation:

<div>
[csharp]
    public class SessionFactory : ISessionFactory
    {
        private string connectionString;
        private MetaDataStore metaDataStore;
 
        public static ISessionFactory Create(Assembly assembly, string connectionString)
        {
            var sessionFactory = new SessionFactory {connectionString = connectionString, metaDataStore = new MetaDataStore()};
            sessionFactory.metaDataStore.BuildMetaDataFor(assembly);
            return sessionFactory;
        }
 
        private SessionFactory() {}
 
        public ISession CreateSession()
        {
            return new Session(connectionString, metaDataStore);
        }
    }
[/csharp]
</div>

The static Create method takes an assembly and a connection string.  The assembly (containing your entity types) will be used to to build up the metadata model as covered in the <a href="http://davybrion.com/blog/2009/08/build-your-own-data-access-layer-mapping-classes-to-tables/">Mapping Classes To Tables post</a>.  You would typically call the Create method in your application's startup code, and then you'd have to store a reference to the ISessionFactory somewhere.  Your application code can then simply call the ISessionFactory's CreateSession method and that's all there is to it.