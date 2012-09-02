Note: This post is part of a series.  Be sure to read the introduction <a href="http://davybrion.com/blog/2009/08/build-your-own-data-access-layer-series/">here</a>.

One thing that i consider an absolute must-have in any data access layer is the ability to perform CRUD operations out-of-the-box without having to write any code to enable these operations.  Once your data access layer knows about your classes and your tables, CRUD operations should 'just work'.

As you've seen in the <a href="http://davybrion.com/blog/2009/08/build-your-own-data-access-layer-mapping-classes-to-tables/">previous post</a> of this series, the TableInfo class offers a couple of methods to automatically build the required SQL statements for CRUD actions.  With these statements, we can easily create SqlCommand instances for all CRUD operations.

First of all, i use the following helper method to easily add a SqlParameter to a SqlCommand:

<div>
[csharp]
        public static void CreateAndAddInputParameter(this SqlCommand command, DbType type, string name, object value)
        {
            var parameter = command.CreateParameter();
            parameter.Direction = ParameterDirection.Input;
            parameter.DbType = type;
            parameter.ParameterName = name;
 
            if (value == null)
            {
                parameter.IsNullable = true;
                parameter.Value = DBNull.Value;
            }
            else
            {
                parameter.Value = value;
            }
 
            command.Parameters.Add(parameter);
        }
[/csharp]
</div>

I also have the following abstract DatabaseAction class which has a few properties that are used by most of the CRUD actions:

<div>
[csharp]
    public abstract class DatabaseAction
    {
        private readonly SqlConnection connection;
        private readonly SqlTransaction transaction;
        protected MetaDataStore MetaDataStore { get; private set; }
        protected EntityHydrater Hydrater { get; private set; }
        protected SessionLevelCache SessionLevelCache { get; private set; }
 
        protected DatabaseAction(SqlConnection connection, SqlTransaction transaction, MetaDataStore metaDataStore,
                                 EntityHydrater hydrater, SessionLevelCache sessionLevelCache)
        {
            this.connection = connection;
            this.transaction = transaction;
            MetaDataStore = metaDataStore;
            Hydrater = hydrater;
            SessionLevelCache = sessionLevelCache;
        }
 
        protected SqlCommand CreateCommand()
        {
            var command = connection.CreateCommand();
            command.Transaction = transaction;
            return command;
        }
    }
[/csharp]
</div>

Did you notice the EntityHydrater and SessionLevelCache? I'm going to ignore those as much as possible for now, since they will be covered in depth in the following two posts in these series.  The important thing to note is that each derived DatabaseAction will have a reference to the MetaDataStore.

And now we can easily start implementing our CRUD actions.  Let's start with the GetByIdAction:

<div>
[csharp]
    public class GetByIdAction : DatabaseAction
    {
        public GetByIdAction(SqlConnection connection, SqlTransaction transaction, MetaDataStore metaDataStore,
            EntityHydrater hydrater, SessionLevelCache sessionLevelCache)
            : base(connection, transaction, metaDataStore, hydrater, sessionLevelCache)
        {
        }
 
        public TEntity Get&lt;TEntity&gt;(object id)
        {
            var cachedEntity = SessionLevelCache.TryToFind(typeof(TEntity), id);
            if (cachedEntity != null) return (TEntity)cachedEntity;
 
            using (var command = CreateCommand())
            {
                var tableInfo = MetaDataStore.GetTableInfoFor&lt;TEntity&gt;();
 
                var query = tableInfo.GetSelectStatementForAllFields();
                tableInfo.AddWhereByIdClause(query);
 
                command.CommandText = query.ToString();
                command.CreateAndAddInputParameter(tableInfo.PrimaryKey.DbType, tableInfo.GetPrimaryKeyParameterName(), id);
                return Hydrater.HydrateEntity&lt;TEntity&gt;(command);
            }
        }
    }
[/csharp]
</div>

Pretty simple stuff, right?  This will first check the session level cache to see if this instance has already been retrieved in the current session (i'll discuss the session in a later post) and if so, it will return that instance.  If it's not in the cache, it will create a SqlCommand and fill its CommandText property with a SQL string that is provided by the relevant TableInfo class.   After that, it passes the SqlCommand to the EntityHydrater so it can return an actual entity instance.

The details of EntityHydration will be fully explored in the next post of this series, so for now you only need to know that it can transform the results from the SqlCommand to an instance of TEntity.

It's always useful to get a collection of all instances of a certain entity class, so we also have this very simple FindAllAction:

<div>
[csharp]
    public class FindAllAction : DatabaseAction
    {
        public FindAllAction(SqlConnection connection, SqlTransaction transaction, MetaDataStore metaDataStore,
            EntityHydrater hydrater, SessionLevelCache sessionLevelCache)
            : base(connection, transaction, metaDataStore, hydrater, sessionLevelCache)
        {
        }
 
        public IEnumerable&lt;TEntity&gt; FindAll&lt;TEntity&gt;()
        {
            using (var command = CreateCommand())
            {
                command.CommandText = MetaDataStore.GetTableInfoFor&lt;TEntity&gt;().GetSelectStatementForAllFields().ToString();
                return Hydrater.HydrateEntities&lt;TEntity&gt;(command);
            }
        }
    }

[/csharp]
</div>

We also need an InsertAction:

<div>
[csharp]
    public class InsertAction : DatabaseAction
    {
        public InsertAction(SqlConnection connection, SqlTransaction transaction, MetaDataStore metaDataStore,
            EntityHydrater hydrater, SessionLevelCache sessionLevelCache)
            : base(connection, transaction, metaDataStore, hydrater, sessionLevelCache)
        {
        }
 
        public TEntity Insert&lt;TEntity&gt;(TEntity entity)
        {
            using (var command = CreateCommand())
            {
                var tableInfo = MetaDataStore.GetTableInfoFor&lt;TEntity&gt;();
 
                command.CommandText = tableInfo.GetInsertStatement();
 
                foreach (var parameterInfo in tableInfo.GetParametersForInsert(entity))
                {
                    command.CreateAndAddInputParameter(parameterInfo.DbType, parameterInfo.Name, parameterInfo.Value);
                }
 
                object id = Convert.ChangeType(command.ExecuteScalar(), tableInfo.PrimaryKey.DotNetType);
                tableInfo.PrimaryKey.PropertyInfo.SetValue(entity, id, null);
                SessionLevelCache.Store(typeof(TEntity), id, entity);
                return entity;
            }
        }
    }
[/csharp]
</div>

There's not a lot to this one either... The actual insert statement is once again retrieved through the TableInfo class, as are the parameter values (including their values for this specific entity).  You can go back to the previous post to look at the implementation of TableInfo's GetParametersForInsert method :)

Keep in mind that there is a limitation here that i only support SQL Server's Identity-style generators.  Again, if you want to support multiple identifier strategies like NHibernate does, you'll have to deal with a lot more complexity in the InsertAction class.

The UpdateAction is very similar:

<div>
[csharp]
    public class UpdateAction : DatabaseAction
    {
        public UpdateAction(SqlConnection connection, SqlTransaction transaction, MetaDataStore metaDataStore,
            EntityHydrater hydrater, SessionLevelCache sessionLevelCache)
            : base(connection, transaction, metaDataStore, hydrater, sessionLevelCache)
        {
        }
 
        public TEntity Update&lt;TEntity&gt;(TEntity entity)
        {
            using (var command = CreateCommand())
            {
                var tableInfo = MetaDataStore.GetTableInfoFor&lt;TEntity&gt;();
 
                command.CommandText = tableInfo.GetUpdateStatement();
 
                foreach (var parameterInfo in tableInfo.GetParametersForUpdate(entity))
                {
                    command.CreateAndAddInputParameter(parameterInfo.DbType, parameterInfo.Name, parameterInfo.Value);
                }
 
                command.ExecuteNonQuery();
                return entity;
            }
        }
    }
[/csharp]
</div>

And finally, we have the DeleteAction:

<div>
[csharp]
    public class DeleteAction : DatabaseAction
    {
        public DeleteAction(SqlConnection connection, SqlTransaction transaction, MetaDataStore metaDataStore,
            EntityHydrater hydrater, SessionLevelCache sessionLevelCache)
            : base(connection, transaction, metaDataStore, hydrater, sessionLevelCache)
        {
        }
 
        public void Delete&lt;TEntity&gt;(TEntity entity)
        {
            using (var command = CreateCommand())
            {
                var tableInfo = MetaDataStore.GetTableInfoFor&lt;TEntity&gt;();
                command.CommandText = tableInfo.GetDeleteStatement();
                object id = tableInfo.PrimaryKey.PropertyInfo.GetValue(entity, null);
                command.CreateAndAddInputParameter(tableInfo.PrimaryKey.DbType, tableInfo.GetPrimaryKeyParameterName(), id);
                command.ExecuteNonQuery();
                SessionLevelCache.Remove(entity);
            }
        }
    }
[/csharp]
</div>

And that's all there is to it.  We now have some classes that will give us out-of-the-box CRUD functionality for all of the mapped entity classes.  Obviously, you will still need some way of actually accessing this functionality from your application code and you certainly don't want to instantiate and use these DatabaseAction classes directly.  All of that will be covered in the "Bringing It All Together" post, so stay tuned ;)