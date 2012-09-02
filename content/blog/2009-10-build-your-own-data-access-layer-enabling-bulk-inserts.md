Note: This post is part of a series. Be sure to read the introduction <a href="http://davybrion.com/blog/2009/08/build-your-own-data-access-layer-series/">here</a>.

I know i wrapped up the series already, but i just had to add the ability to do bulk inserts to this data layer so i figured i'd might as well post about it.  Ayende already talked about how to enable the ability to batch inserts (or updates and deletes) <a href="http://ayende.com/Blog/archive/2006/09/13/OpeningUpQueryBatching.aspx">here</a> and <a href="http://ayende.com/Blog/archive/2006/09/14/ThereBeDragonsRhinoCommonsSqlCommandSet.aspx">here</a> so i'm going to skip that part.  I used the exact same trick and created a PublicSqlCommandSet class which wraps the hidden SqlCommandSet class.  Again, if you have no idea what i'm talking about in that last sentence then you need to read Ayende's 2 posts that i just linked to ;)

After that, adding the bulk insert feature to the DAL was as simple as creating this class:

<div>
[csharp]
    public class BulkInsertAction : DatabaseAction
    {
        public BulkInsertAction(SqlConnection connection, SqlTransaction transaction, MetaDataStore metaDataStore,
            EntityHydrater hydrater, SessionLevelCache sessionLevelCache)
            : base(connection, transaction, metaDataStore, hydrater, sessionLevelCache) {}
 
        public void Insert&lt;TEntity&gt;(IEnumerable&lt;TEntity&gt; entities, int batchSize, int commandTimeOut)
        {
            var tableInfo = MetaDataStore.GetTableInfoFor&lt;TEntity&gt;();
            var insertStatement = tableInfo.GetInsertStatementWithoutReturningTheIdentityValue();
 
            var sqlCommandSet = new PublicSqlCommandSet { CommandTimeout = commandTimeOut, Connection = GetConnection(), Transaction = GetTransaction() };
 
            foreach (var entity in entities)
            {
                var currentCommand = CreateCommand();
                currentCommand.CommandText = insertStatement;
 
                foreach (var parameterInfo in tableInfo.GetParametersForInsert(entity))
                {
                    currentCommand.CreateAndAddInputParameter(parameterInfo.DbType, &quot;@&quot; + parameterInfo.Name, parameterInfo.Value);
                }
 
                sqlCommandSet.Append(currentCommand);
 
                if (sqlCommandSet.CommandCount == batchSize)
                {
                    ExecuteCurrentBatch(sqlCommandSet);
                    sqlCommandSet = new PublicSqlCommandSet { CommandTimeout = commandTimeOut, Connection = GetConnection(), Transaction = GetTransaction() };
                }
            }
 
            if (sqlCommandSet.CommandCount &gt; 0)
            {
                ExecuteCurrentBatch(sqlCommandSet);
            }
        }
 
        private void ExecuteCurrentBatch(PublicSqlCommandSet sqlCommandSet)
        {
            try
            {
                sqlCommandSet.ExecuteNonQuery();
            }
            finally
            {
                sqlCommandSet.Dispose();
            }
        }
    }
[/csharp]
</div>

And then adding this to the Session class:

<div>
[csharp]
        public void BulkInsert&lt;TEntity&gt;(IEnumerable&lt;TEntity&gt; entities)
        {
            CreateAction&lt;BulkInsertAction&gt;().Insert(entities, 50, 200);
        }
[/csharp]
</div>

Obviously, the method signature was also added to the ISession interface.  The batch-size and commandtimeout parameters are currently hardcoded but they should come from some kind of configuration file.

All in all, pretty easy stuff :)