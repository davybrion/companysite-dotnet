Note: This post is part of a series.  Be sure to read the introduction <a href="http://davybrion.com/blog/2009/08/build-your-own-data-access-layer-series/">here</a>.

In the <a href="http://davybrion.com/blog/2009/08/build-your-own-data-access-layer-out-of-the-box-crud-functionality/">previous post</a> of this series, you saw that some of the DatabaseActions use the EntityHydrater to umm.. hydrate the entities with their values from the database.  In this post, we'll go over how this actually works.  First, i'm going to post the code of the entire class and then we'll go over the interesting parts.

So, here's the entire code of the EntityHydrater class:

<div>
[csharp]
    public class EntityHydrater
    {
        private readonly MetaDataStore metaDataStore;
        private readonly SessionLevelCache sessionLevelCache;
 
        public EntityHydrater(MetaDataStore metaDataStore, SessionLevelCache sessionLevelCache)
        {
            this.metaDataStore = metaDataStore;
            this.sessionLevelCache = sessionLevelCache;
        }
 
        public TEntity HydrateEntity&lt;TEntity&gt;(SqlCommand command)
        {
            IDictionary&lt;string, object&gt; values;
 
            using (var reader = command.ExecuteReader())
            {
                if (!reader.HasRows) return default(TEntity);
                reader.Read();
                values = GetValuesFromCurrentRow(reader);
            }
 
            return CreateEntityFromValues&lt;TEntity&gt;(values);
        }
 
        public IEnumerable&lt;TEntity&gt; HydrateEntities&lt;TEntity&gt;(SqlCommand command)
        {
            var rows = new List&lt;IDictionary&lt;string, object&gt;&gt;();
            var entities = new List&lt;TEntity&gt;();
 
            using (var reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    rows.Add(GetValuesFromCurrentRow(reader));
                }
            }
 
            foreach (var row in rows)
            {
                entities.Add(CreateEntityFromValues&lt;TEntity&gt;(row));
            }
 
            return entities;
        }
 
        private IDictionary&lt;string, object&gt; GetValuesFromCurrentRow(SqlDataReader dataReader)
        {
            var values = new Dictionary&lt;string, object&gt;();
 
            for (int i = 0; i &lt; dataReader.FieldCount; i++)
            {
                values.Add(dataReader.GetName(i), dataReader.GetValue(i));
            }
 
            return values;
        }
 
        private TEntity CreateEntityFromValues&lt;TEntity&gt;(IDictionary&lt;string, object&gt; values)
        {
            var tableInfo = metaDataStore.GetTableInfoFor&lt;TEntity&gt;();
 
            var cachedEntity = sessionLevelCache.TryToFind(typeof(TEntity), values[tableInfo.PrimaryKey.Name]);
            if (cachedEntity != null) return (TEntity)cachedEntity;
 
            var entity = Activator.CreateInstance&lt;TEntity&gt;();
            Hydrate(tableInfo, entity, values);
            sessionLevelCache.Store(typeof(TEntity), values[tableInfo.PrimaryKey.Name], entity);
            return entity;
        }
 
        private void Hydrate&lt;TEntity&gt;(TableInfo tableInfo, TEntity entity, IDictionary&lt;string, object&gt; values)
        {
            tableInfo.PrimaryKey.PropertyInfo.SetValue(entity, values[tableInfo.PrimaryKey.Name], null);
            SetRegularColumns(tableInfo, entity, values);
            SetReferenceProperties(tableInfo, entity, values);
        }
 
        private void SetRegularColumns&lt;TEntity&gt;(TableInfo tableInfo, TEntity entity, IDictionary&lt;string, object&gt; values)
        {
            foreach (var columnInfo in tableInfo.Columns)
            {
                if (columnInfo.PropertyInfo.CanWrite)
                {
                    object value = values[columnInfo.Name];
                    if (value is DBNull) value = null;
                    columnInfo.PropertyInfo.SetValue(entity, value, null);
                }
            }
        }
 
        private void SetReferenceProperties&lt;TEntity&gt;(TableInfo tableInfo, TEntity entity, IDictionary&lt;string, object&gt; values)
        {
            foreach (var referenceInfo in tableInfo.References)
            {
                if (referenceInfo.PropertyInfo.CanWrite)
                {
                    object foreignKeyValue = values[referenceInfo.Name];
 
                    if (foreignKeyValue is DBNull)
                    {
                        referenceInfo.PropertyInfo.SetValue(entity, null, null);
                    }
                    else
                    {
                        var referencedEntity = sessionLevelCache.TryToFind(referenceInfo.ReferenceType, foreignKeyValue) ??
                                               CreateProxy(tableInfo, referenceInfo, foreignKeyValue);
 
                        referenceInfo.PropertyInfo.SetValue(entity, referencedEntity, null);
                    }
                }
            }
        }
 
        private object CreateProxy(TableInfo tableInfo, ReferenceInfo referenceInfo, object foreignKeyValue)
        {
            // NOTE: this will be covered in a later post, so i took out the spoiler code 
            return null;
        }
    }
[/csharp]
</div>

As you can see, there are 2 public methods: HydrateEntity and HydrateEntities.  They are both pretty similar, except that the former only hydrates a single entity and the latter a list of entities.  They both retrieve the values from the current position in a DataReader and store them in a Dictionary with the name of the column being the key and the value of the column being the value in the dictionary:

<div>
[csharp]
        private IDictionary&lt;string, object&gt; GetValuesFromCurrentRow(SqlDataReader dataReader)
        {
            var values = new Dictionary&lt;string, object&gt;();
 
            for (int i = 0; i &lt; dataReader.FieldCount; i++)
            {
                values.Add(dataReader.GetName(i), dataReader.GetValue(i));
            }
 
            return values;
        }
[/csharp]
</div>

This dictionary is then passed to the CreateEntityFromValues method:

<div>
[csharp]
        private TEntity CreateEntityFromValues&lt;TEntity&gt;(IDictionary&lt;string, object&gt; values)
        {
            var tableInfo = metaDataStore.GetTableInfoFor&lt;TEntity&gt;();
 
            var cachedEntity = sessionLevelCache.TryToFind(typeof(TEntity), values[tableInfo.PrimaryKey.Name]);
            if (cachedEntity != null) return (TEntity)cachedEntity;
 
            var entity = Activator.CreateInstance&lt;TEntity&gt;();
            Hydrate(tableInfo, entity, values);
            sessionLevelCache.Store(typeof(TEntity), values[tableInfo.PrimaryKey.Name], entity);
            return entity;
        }
[/csharp]
</div>

This method will first check to see if an entity instance for the row in the values dictionary already exists.  If it exists already, it simply returns the entity instance and ignores the values in the dictionary.  I'm not sure yet whether this behavior is correct or not.  Theoretically speaking, it's possible when using ReadCommitted isolation level that the values in the dictionary will be more recent than the entity instance in the session level cache.  However, since this simple DAL has no change tracking, i also can't deduce whether the instance in the cache has already had one or more of its properties updated by application code.  Simply overwriting them with newly retrieved values of the database doesn't seem like the right thing to do here.  So the current options are to either ignore this possibility, or to rely on optimistic concurrency.  Oh, and i don't have support for optimistic concurrency either.  A good DAL should however provide some optimistic concurrency strategies here, or be able to track the changes in the entity and throw an exception if it notices that the database contains more recent values AND the local entity has already been modified.  If it hasn't been modified yet, it could overwrite the values of the instances though i'm not sure everyone would agree with this behavior.  Either way, this particular problem is definitely interesting enough to think about for a while ;)

Anyways, in this implementation, i'm completely ignoring this situation and i either return the already loaded entity of the session level cache, or i hydrate a new entity instance and return that instead.  In the latter case, i also store the entity instance in the session level cache.  Note that the actual implementation of the session level cache will be covered in the next post of this series.

The actual Hydrate method looks like this:

<div>
[csharp]
        private void Hydrate&lt;TEntity&gt;(TableInfo tableInfo, TEntity entity, IDictionary&lt;string, object&gt; values)
        {
            tableInfo.PrimaryKey.PropertyInfo.SetValue(entity, values[tableInfo.PrimaryKey.Name], null);
            SetRegularColumns(tableInfo, entity, values);
            SetReferenceProperties(tableInfo, entity, values);
        }
[/csharp]
</div>

First the primary key value of the record is set in the primary key property of the new instance, and then we proceed with putting the regular column values in their properties, and after that, the reference properties. Filling the regular column properties is very straightforward:

<div>
[csharp]
        private void SetRegularColumns&lt;TEntity&gt;(TableInfo tableInfo, TEntity entity, IDictionary&lt;string, object&gt; values)
        {
            foreach (var columnInfo in tableInfo.Columns)
            {
                if (columnInfo.PropertyInfo.CanWrite)
                {
                    object value = values[columnInfo.Name];
                    if (value is DBNull) value = null;
                    columnInfo.PropertyInfo.SetValue(entity, value, null);
                }
            }
        }
[/csharp]
</div>

Dealing with the references is a bit more interesting though.

<div>
[csharp]
        private void SetReferenceProperties&lt;TEntity&gt;(TableInfo tableInfo, TEntity entity, IDictionary&lt;string, object&gt; values)
        {
            foreach (var referenceInfo in tableInfo.References)
            {
                if (referenceInfo.PropertyInfo.CanWrite)
                {
                    object foreignKeyValue = values[referenceInfo.Name];
 
                    if (foreignKeyValue is DBNull)
                    {
                        referenceInfo.PropertyInfo.SetValue(entity, null, null);
                    }
                    else
                    {
                        var referencedEntity = sessionLevelCache.TryToFind(referenceInfo.ReferenceType, foreignKeyValue) ??
                                               CreateProxy(tableInfo, referenceInfo, foreignKeyValue);
 
                        referenceInfo.PropertyInfo.SetValue(entity, referencedEntity, null);
                    }
                }
            }
        }
[/csharp]
</div>

If we can't find the referenced entity instance in the first level cache, what should we do? We obviously can't load it automatically because that could in turn cause referenced entities' references to be loaded automatically when they are hydrated.  Which in turn could cause their referenced entities... Well, i'm sure you get the point.  But those properties obviously can't be set to a null reference either because the column actually does have a valid foreign key value in the database.  Explicitly loading referenced properties leads to seriously ugly (and error-prone) code so that's not an option i'm willing to consider either.  The correct way to deal with this is to use lazy loading.  To do that in an automated fashion, we need proxy classes.  I'm not going to get into these proxy classes and the whole lazy loading thing just yet, since that will be covered in depth in a future post ;)

So that's pretty much it (for now) for our EntityHydrater class.  As you can see, it's still relatively simple but then again, the use cases that it supports are extremely simple as well.  This current implementation is incapable of hydrating entities based on a SQL statement that selects data from more than just the entity's table.  And that is a pretty big shortcoming.  For instance, with NHibernate you can execute queries where you can instruct NHibernate to fetch some (or all) of the entity's references (associations in NHibernate) with just one SQL statement, using the join syntax.  NHibernate can then hydrate the root entity, and populate its reference properties with the other values that were returned by the sql statement.  While it wouldn't be that complex to add this capability to this EntityHydrater class, it wouldn't exactly be completely trivial either.  Again, this is a limitation that many (maybe even most?) custom DAL's have.  This one probably makes it easy enough to still add this feature though ;)