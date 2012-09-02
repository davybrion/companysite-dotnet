Note: This post is part of a series.  Be sure to read the introduction <a href="http://davybrion.com/blog/2009/08/build-your-own-data-access-layer-series/">here</a>.

In the previous 2 posts of this series, you may have noticed the usage of the SessionLevelCache class. This class is a simple implementation of the <a href="http://martinfowler.com/eaaCatalog/identityMap.html">Identity Map pattern</a>.  It basically offers us two important benefits:
<ol>
	<li>It ensures that we never load the same entity from the database twice.</li>
	<li>It ensures that we can't accidentally have two instances which point to the same database record.</li>
</ol>

Well actually... there is one situation where you could possibly load the same entity twice (which i'll show momentarily) but even then we can make sure that our users will always have the same instance of the entity.

The SessionLevelCache class is actually very simple:

<div>
[csharp]
    public class SessionLevelCache
    {
        private readonly Dictionary&lt;Type, Dictionary&lt;string, object&gt;&gt; cache = new Dictionary&lt;Type, Dictionary&lt;string, object&gt;&gt;();
 
        public object TryToFind(Type type, object id)
        {
            if (!cache.ContainsKey(type)) return null;
 
            string idAsString = id.ToString();
            if (!cache[type].ContainsKey(idAsString)) return null;
 
            return cache[type][idAsString];
        }
 
        public void Store(Type type, object id, object entity)
        {
            if (!cache.ContainsKey(type)) cache.Add(type, new Dictionary&lt;string, object&gt;());
 
            cache[type][id.ToString()] = entity;
        }
 
        public void ClearAll()
        {
            cache.Clear();
        }
 
        public void RemoveAllInstancesOf(Type type)
        {
            if (cache.ContainsKey(type))
            {
                cache.Remove(type);
            }
        }
 
        public void Remove(object entity)
        {
            var type = entity.GetType();
 
            if (!cache.ContainsKey(type)) return;
 
            string keyToRemove = null;
 
            foreach (var pair in cache[type])
            {
                if (pair.Value == entity)
                {
                    keyToRemove = pair.Key;
                }
            }
 
            if (keyToRemove != null)
            {
                cache[type].Remove(keyToRemove);
            }
        }
    }
[/csharp]
</div>

It uses a nested dictionary where the outer dictionary uses the entity type as its key value, and stores an inner dictionary for each entity type which uses the primary key value as the key for each entity instance.  I think the code is simple enough so there's not really any point to go over the implementation details.  Instead, let's focus on when and where the SessionLevelCache is used.

First, let's look back at the Get method of the GetByIdAction class:

<div>
[csharp]
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
[/csharp]
</div>

As you can see, when we enter this method we first check to see whether the entity instance is already present in the cache.  If it is, we obviously don't need to hit the database so we can simply return the cached entity.

But like i said, there is one situation where we could potentially retrieve the entity from the database even though we already have it in the cache, and that is when we are hydrating a list of entities that have been retrieved from the database.  You might remember the following method from the EntityHydrater class:

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

This method is used to hydrate every single entity that we create.  Obviously, when hydrating a single entity we've actually already performed the cache-look-up in the GetByIdAction.  However, when a custom query is executed, or when all instances are retrieved, there is no way for us to exclude already cached entity instances from the result of the query.  Well, theoretically speaking you could attempt to do this by adding a clause to the WHERE statement of each query that would prevent cached entities from being loaded.  But then you might have to add the cached entity instances to the resulting list of entities anyways if they would otherwise satisfy the other query conditions.  Obviously, trying to get this right is simply put insane and i don't think there's any DAL or ORM that actually does this (even if there was, i can't really imagine any of them getting this right in every corner case that will pop up).  

So a good compromise is to simply check for the existence of a specific instance in the cache before hydrating a new instance.  If it is there, we return it from the cache and we skip the hydration for that database record.  In this way, we avoid having to modify the original query, and while we could potentially return a few records that we already have in memory, at least we will be sure that our users will always have the same reference for any particular database record.

There is one more scenario that needs to be covered.  If an entity holds a reference through a foreign key to another entity instance, and that referenced entity is already present in the cache, we need to make sure that the entity we are hydrating will refer to the already cached referred-to-instance instead of creating a proxy by default.  After all, if we were to create a proxy object for an entity instance that is already in our cache, we will have failed to achieve our goal of avoiding the possibility of more than one instance representing the same database record.  Therefore, when hydrating the reference properties of an entity, we have the following piece of code:

<div>
[csharp]
                        var referencedEntity = sessionLevelCache.TryToFind(referenceInfo.ReferenceType, foreignKeyValue) ??
                                               CreateProxy(tableInfo, referenceInfo, foreignKeyValue);
 
                        referenceInfo.PropertyInfo.SetValue(entity, referencedEntity, null);
[/csharp]
</div>

This way, we are sure that a proxy object for a referenced entity will only be created if that entity is not already present in the SessionLevelCache.

Now we're pretty much covered when it comes to retrieving entity instances from the database.  But we obviously also need to update the SessionLevelCache whenever an entity is inserted, and whenever an entity is deleted.  In the InsertAction, you can find the following code at the end of the Insert method:

<div>
[csharp]
                SessionLevelCache.Store(typeof(TEntity), id, entity);
[/csharp]
</div>

And in the DeleteAction we can also spot the following line of code:

<div>
[csharp]
                SessionLevelCache.Remove(entity);
[/csharp]
</div>

There is still one problem however.  When a user executes a custom DELETE statement, there is no way for us to know which entities were actually removed from the database.  But if any of those deleted entities happen to remain in the SessionLevelCache, this could lead to buggy application code whenever a piece of code tries to retrieve a specific entity which has already been removed from the database, but is still present in the SessionLevelCache.  In order to deal with this scenario, the SessionLevelCache has a ClearAll and a RemoveAllInstancesOf method which you can use from your application code to either clear the entire SessionLevelCache, or to remove all instances of a specific entity type from the cache.  Calling these methods would obviously be the responsability of the application code, since we can't possibly take care of this automatically in such a simplistic DAL.  Actually, even for powerful ORM's this can be pretty difficult to get right.

Another thing i'd like to point out is that the SessionLevelCache is not threadsafe.  A session (which will be covered in a later post) is not threadsafe, so within each of the classes that are used by the session i take no care of thread-safety whatsoever.

The SessionLevelCache might not seem like much to you, but i do consider its to be an absolute must for any DAL.  Notice that this doesn't even come close to a proper second-level cache like NHibernate offers, but the complexity of implementing such a thing is way beyond the scope of both this series and probably most custom DAL's out there.