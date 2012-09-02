Note: This post is part of a series.  Be sure to read the introduction <a href="http://davybrion.com/blog/2009/08/build-your-own-data-access-layer-series/">here</a>.

When you need to populate entity instances with data from a database, you need to know which table the data needs to come from, which columns will map to which property on the entity class, and you'll need to deal with a variety of types.  The approach that i've chosen to use tries to make this as simple as possible.  The idea is basically to place an attribute with the name of the table on top of the entity class, and an attribute on each property with the name of the column it maps to.  For foreign keys, i wanted to be able to just use properties of the type of the referenced entity, instead of having foreign keys in my entities.  For these references, we will use an attribute with the name of the foreign key column.

First, we'll need to define these attributes:

<div>
[csharp]
    [AttributeUsage(AttributeTargets.Class, Inherited = true)]
    public class TableAttribute : Attribute
    {
        public string TableName { get; private set; }
 
        public TableAttribute(string tableName)
        {
            TableName = tableName;
        }
    }

    [AttributeUsage(AttributeTargets.Property, Inherited = true)]
    public class PrimaryKeyAttribute : Attribute
    {
        public string ColumnName { get; private set; }
 
        public PrimaryKeyAttribute(string columnName)
        {
            ColumnName = columnName;
        }
    }

    [AttributeUsage(AttributeTargets.Property, Inherited = true)]
    public class ColumnAttribute : Attribute
    {
        public string ColumnName { get; private set; }
 
        public ColumnAttribute(string columnName)
        {
            ColumnName = columnName;
        }
    }

    [AttributeUsage(AttributeTargets.Property, Inherited = true)]
    public class ReferenceAttribute : Attribute
    {
        public string ColumnName { get; private set; }
 
        public ReferenceAttribute(string columnName)
        {
            ColumnName = columnName;
        }
    }
[/csharp]
</div>

Notice how none of these properties have any indication of types to use.  The .NET type will be inferred automatically, and it will be mapped to a compatible DbType without having to specify these types all over the place.

We will use the following helper class to map .NET types to their respective DbTypes:

<div>
[csharp]
    public static class TypeConverter
    {
        private static readonly Dictionary&lt;Type, DbType&gt; typeToDbType = new Dictionary&lt;Type, DbType&gt;
        {
            { typeof(string), DbType.String },
            { typeof(DateTime), DbType.DateTime },
            { typeof(DateTime?), DbType.DateTime },
            { typeof(int), DbType.Int32 },
            { typeof(int?), DbType.Int32 },
            { typeof(long), DbType.Int64 },
            { typeof(long?), DbType.Int64 },
            { typeof(bool), DbType.Boolean },
            { typeof(bool?), DbType.Boolean },
            { typeof(byte[]), DbType.Binary },
            { typeof(decimal), DbType.Decimal },
            { typeof(decimal?), DbType.Decimal },
            { typeof(double), DbType.Double },
            { typeof(double?), DbType.Double },
            { typeof(float), DbType.Single },
            { typeof(float?), DbType.Single },
            { typeof(Guid), DbType.Guid },
            { typeof(Guid?), DbType.Guid }
        };
 
        public static DbType ToDbType(Type type)
        {
            if (!typeToDbType.ContainsKey(type))
            {
                throw new InvalidOperationException(string.Format(&quot;Type {0} doesn't have a matching DbType configured&quot;, type.FullName));
            }
 
            return typeToDbType[type];
        }
    }
[/csharp]
</div>

Obviously, more type conversions can be added... these are just the ones i've needed so far.

Once you've placed all the attributes on top of your entities and properties, we can start building a model of all this metadata.  This will all be stored in a MetaDataStore class that i'll show later on in this post.  Having access to the MetaDataStore makes the implementation of some of these metadata types easier, so i have the following abstract class:

<div>
[csharp]
    public abstract class MetaData
    {
        protected MetaDataStore MetaDataStore { get; private set; }
 
        protected MetaData(MetaDataStore metaDataStore)
        {
            MetaDataStore = metaDataStore;
        }
    }
[/csharp]
</div>

Now we can go over each piece of metadata.  First, the ColumnInfo class:

<div>
[csharp]
    public class ColumnInfo : MetaData
    {
        public string Name { get; private set; }
        public Type DotNetType { get; private set; }
        public DbType DbType { get; private set; }
        public PropertyInfo PropertyInfo { get; private set; }
 
        public ColumnInfo(MetaDataStore store, string name, Type dotNetType, PropertyInfo propertyInfo)
            : this(store, name, dotNetType, TypeConverter.ToDbType(dotNetType), propertyInfo)
        {
        }
 
        public ColumnInfo(MetaDataStore store, string name, Type dotNetType, DbType dbType, PropertyInfo propertyInfo)
            : base(store)
        {
            Name = name;
            DotNetType = dotNetType;
            DbType = dbType;
            PropertyInfo = propertyInfo;
        }
    }
[/csharp]
</div>

As you can see, we have all the information we need to be able to do something with this column.  We have its Name, the .NET type that is used in the mapped class, the DbType and a PropertyInfo reference to its respective property in the mapped class so we can get and set its value.

For references, we need to know something more:

<div>
[csharp]
    public class ReferenceInfo : ColumnInfo
    {
        public Type ReferenceType { get; private set; }
 
        public ReferenceInfo(MetaDataStore store, string name, Type referenceType, PropertyInfo propertyInfo)
            : base(store, name, store.GetTableInfoFor(referenceType).PrimaryKey.DotNetType,
                    store.GetTableInfoFor(referenceType).PrimaryKey.DbType, propertyInfo)
        {
            ReferenceType = referenceType;
        }
    }
[/csharp]
</div>

For a regular column, it's sufficient to know the .NET type of the property and the DbType.  But for a reference, you need to know the actual type of the referenced entity, as well as the .NET type of it's primary key column.  As you can see in the constructor, we retrieve the TableInfo of the referenced entity, and use the .NET type and the DbType of the primary key of the referenced entity.  The PrimaryKey property of a TableInfo class (which i'll show below) is also a ColumnInfo object.  We obviously also store the actual type of the referenced entity.  And of course, we again store a PropertyInfo so we can get/set the value of the reference.

The TableInfo class can now hold all of the information that we need.  We know all about its primary key (through the PrimaryKeyAttribute), its regular properties (through the ColumnAttribute) and its referenced properties (through the ReferenceAttribute).  With all of that information, the TableInfo class is able to build your typical default SQL statements for CRUD functionality:

<div>
[csharp]
    public class TableInfo : MetaData
    {
        public string Name { get; private set; }
        public Type EntityType { get; private set; }
        public ColumnInfo PrimaryKey { get; set; }
        public IEnumerable&lt;ReferenceInfo&gt; References { get { return references.Values; } }
        public IEnumerable&lt;ColumnInfo&gt; Columns { get { return columns.Values; } }
 
        private readonly Dictionary&lt;string, ColumnInfo&gt; columns = new Dictionary&lt;string, ColumnInfo&gt;();
        private readonly Dictionary&lt;string, ReferenceInfo&gt; references = new Dictionary&lt;string, ReferenceInfo&gt;();
 
        public TableInfo(MetaDataStore store, string name, Type entityType)
            : base(store)
        {
            Name = name;
            EntityType = entityType;
        }
 
        public void AddColumn(ColumnInfo column)
        {
            if (columns.ContainsKey(column.Name))
            {
                throw new InvalidOperationException(string.Format(&quot;An item with key {0} has already been added&quot;, column.Name));
            }
 
            columns.Add(column.Name, column);
        }
 
        public void AddReference(ReferenceInfo reference)
        {
            if (references.ContainsKey(reference.Name))
            {
                throw new InvalidOperationException(string.Format(&quot;An item with key {0} has already been added&quot;, reference.Name));
            }
 
            references.Add(reference.Name, reference);
        }
 
        public ColumnInfo GetColumn(string columnName)
        {
            if (!columns.ContainsKey(columnName))
            {
                throw new InvalidOperationException(string.Format(&quot;The table '{0}' does not have a '{1}' column&quot;, Name, columnName));
            }
 
            return columns[columnName];
        }
 
        public StringBuilder GetSelectStatementForAllFields()
        {
            StringBuilder builder = new StringBuilder(&quot;SELECT &quot; + Escape(PrimaryKey.Name) + &quot;, &quot;);
 
            AddReferenceColumnNames(builder);
            AddRegularColumnNames(builder);
            RemoveLastCommaAndSpaceIfThereAreAnyColumns(builder);
            builder.Append(&quot; FROM &quot; + Escape(Name));
 
            return builder;
        }
 
        public string GetInsertStatement()
        {
            StringBuilder builder = new StringBuilder(&quot;INSERT INTO &quot; + Escape(Name) + &quot; (&quot;);
 
            AddReferenceColumnNames(builder);
            AddRegularColumnNames(builder);
            RemoveLastCommaAndSpaceIfThereAreAnyColumns(builder);
            builder.Append(&quot;) VALUES (&quot;);
            AddReferenceColumnParameterNames(builder);
            AddRegularColumnParameterNames(builder);
            RemoveLastCommaAndSpaceIfThereAreAnyColumns(builder);
            builder.Append(&quot;); SELECT SCOPE_IDENTITY();&quot;);
 
            return builder.ToString();
        }
 
        public string GetUpdateStatement()
        {
            StringBuilder builder = new StringBuilder(&quot;UPDATE &quot; + Escape(Name) + &quot; SET &quot;);
 
            AddReferenceColumnsNameWithParameterName(builder);
            AddRegularColumnsNameWithParameterName(builder);
            RemoveLastCommaAndSpaceIfThereAreAnyColumns(builder);
            AddWhereByIdClause(builder);
            builder.Append(&quot;;&quot;);
 
            return builder.ToString();
        }
 
        public string GetDeleteStatement()
        {
            StringBuilder builder = new StringBuilder(&quot;DELETE FROM &quot; + Escape(Name) + &quot; &quot;);
 
            AddWhereByIdClause(builder);
            builder.Append(&quot;;&quot;);
 
            return builder.ToString();
        }
 
        public IEnumerable&lt;AdoParameterInfo&gt; GetParametersForInsert(object entity)
        {
            return GetParametersForAllReferenceAndRegularColumns(entity);
        }
 
        public IEnumerable&lt;AdoParameterInfo&gt; GetParametersForUpdate(object entity)
        {
            var parameters = GetParametersForAllReferenceAndRegularColumns(entity);
            parameters.Add(new AdoParameterInfo(PrimaryKey.Name, PrimaryKey.DbType, PrimaryKey.PropertyInfo.GetValue(entity, null)));
            return parameters;
        }
 
        public StringBuilder AddWhereByIdClause(StringBuilder query)
        {
            query.Append(&quot; WHERE &quot; + Escape(PrimaryKey.Name) + &quot; = &quot; + GetPrimaryKeyParameterName());
            return query;
        }
 
        public string GetPrimaryKeyParameterName()
        {
            return &quot;@&quot; + PrimaryKey.Name;
        }
 
        private List&lt;AdoParameterInfo&gt; GetParametersForAllReferenceAndRegularColumns(object entity)
        {
            var parameters = new List&lt;AdoParameterInfo&gt;();
 
            foreach (var referenceInfo in References)
            {
                var referencedEntity = referenceInfo.PropertyInfo.GetValue(entity, null);
                var referencePrimaryKeyProperty = MetaDataStore.GetTableInfoFor(referenceInfo.ReferenceType).PrimaryKey.PropertyInfo;
 
                if (referencedEntity == null)
                {
                    parameters.Add(new AdoParameterInfo(referenceInfo.Name, referenceInfo.DbType, null));
                }
                else
                {
                    parameters.Add(new AdoParameterInfo(referenceInfo.Name, referenceInfo.DbType, referencePrimaryKeyProperty.GetValue(referencedEntity, null)));
                }
            }
 
            foreach (var columnInfo in Columns)
            {
                parameters.Add(new AdoParameterInfo(columnInfo.Name, columnInfo.DbType, columnInfo.PropertyInfo.GetValue(entity, null)));
            }
 
            return parameters;
        }
 
        private void RemoveLastCommaAndSpaceIfThereAreAnyColumns(StringBuilder builder)
        {
            if ((References.Count() + Columns.Count()) &gt; 0)
            {
                RemoveLastCharacters(builder, 2);
            }
        }
 
        private void AddReferenceColumnNames(StringBuilder builder)
        {
            foreach (var referenceInfo in References)
            {
                builder.Append(Escape(referenceInfo.Name) + &quot;, &quot;);
            }
        }
 
        private void AddReferenceColumnParameterNames(StringBuilder builder)
        {
            foreach (var referenceInfo in References)
            {
                builder.Append(&quot;@&quot; + referenceInfo.Name + &quot;, &quot;);
            }
        }
 
        private void AddReferenceColumnsNameWithParameterName(StringBuilder builder)
        {
            foreach (var referenceInfo in References)
            {
                builder.Append(Escape(referenceInfo.Name) + &quot; = @&quot; + referenceInfo.Name + &quot;, &quot;);
            }
        }
 
        private void AddRegularColumnNames(StringBuilder builder)
        {
            foreach (var columnInfo in Columns)
            {
                builder.Append(Escape(columnInfo.Name) + &quot;, &quot;);
            }
        }
 
        private void AddRegularColumnParameterNames(StringBuilder builder)
        {
            foreach (var columnInfo in Columns)
            {
                builder.Append(&quot;@&quot; + columnInfo.Name + &quot;, &quot;);
            }
        }
 
        private void AddRegularColumnsNameWithParameterName(StringBuilder builder)
        {
            foreach (var columnInfo in Columns)
            {
                builder.Append(Escape(columnInfo.Name) + &quot; = @&quot; + columnInfo.Name + &quot;, &quot;);
            }
        }
 
        private string Escape(string name)
        {
            return &quot;[&quot; + name + &quot;]&quot;;
        }
 
        private void RemoveLastCharacters(StringBuilder stringBuilder, int numberOfCharacters)
        {
            stringBuilder.Remove(stringBuilder.Length - numberOfCharacters, numberOfCharacters);
        }
    }
[/csharp]
</div>

This is actually the biggest class in this DAL. I probably should move the building of the SQL statements and providing parameter info into some kind of helper class because this is a bit of a Single Responsability Principle violation.  Speaking of parameter info, i'm using the following helper class to store this information:

<div>
[csharp]
    public class AdoParameterInfo
    {
        public DbType DbType { get; private set; }
        public string Name { get; private set; }
        public object Value { get; private set; }
 
        public AdoParameterInfo(string name, DbType dbType, object value)
        {
            Name = name;
            DbType = dbType;
            Value = value;
        }
    }
[/csharp]
</div>

One thing that you may have noticed is that the generated INSERT statement assumes that SQL Server identity-style generators are being used for primary key values.  Not only that, i'm not even trying to target any other database then SQL Server with this DAL.  Those are 2 rather significant shortcomings of this DAL.  First of all, dealing with multiple identifier strategies can become pretty complex pretty fast.  For this DAL, SQL Server Identity primary keys are sufficient but in a lot of cases you will probably want support for assigned identifier strategies, for GUIDs (preferably locally generated with a sequential GUID algorithm), HiLo and maybe even other ones.  If you really want to, you can do all of this yourself, but you'll quickly spend an entire week (or more) to properly implement all of these identifier strategies.

As for only targeting SQL Server, that is sufficient in our scenario but a proper DAL should be able to deal with multiple databases.  Of course, this has a direct impact on a lot of implementation details.  For starters, you'd never be able to just construct a SQL statement directly in your code and you will need something to make sure the correct statements are generated for your specific database.  NHibernate does a pretty nice job of this by providing a strategy-like implementation through its Dialect class and its derivatives.  Also, some of your identifier strategies will be different for each database that you need to support.  If you got a headache just from reading these last 2 paragraphs, just imagine implementing this and getting it all 'right' in a maintainable matter. 

Anyways, back to the topic at hand.  We now have the classes we need to build up our metadata model of all of the tables we need to provide data access functionality for.  Well, we still need something to hold all of this information and to actually build up this model:

<div>
[csharp]
    public class MetaDataStore
    {
        private readonly Dictionary&lt;Type, TableInfo&gt; typeToTableInfo = new Dictionary&lt;Type, TableInfo&gt;();
 
        public TableInfo GetTableInfoFor&lt;TEntity&gt;()
        {
            return GetTableInfoFor(typeof(TEntity));
        }
 
        public TableInfo GetTableInfoFor(Type entityType)
        {
            if (!typeToTableInfo.ContainsKey(entityType))
            {
                return null;
            }
 
            return typeToTableInfo[entityType];
        }
 
        public void BuildMetaDataFor(Assembly assembly)
        {
            BuildMapOfEntityTypesWithTheirTableInfo(assembly);
 
            foreach (KeyValuePair&lt;Type, TableInfo&gt; pair in typeToTableInfo)
            {
                // we need this info for each entity before we can deal with references to other entities
                LoopThroughPropertiesWith&lt;PrimaryKeyAttribute&gt;(pair.Key, pair.Value, SetPrimaryKeyInfo);
            }
 
            foreach (KeyValuePair&lt;Type, TableInfo&gt; pair in typeToTableInfo)
            {
                LoopThroughPropertiesWith&lt;ReferenceAttribute&gt;(pair.Key, pair.Value, AddReferenceInfo);
                LoopThroughPropertiesWith&lt;ColumnAttribute&gt;(pair.Key, pair.Value, AddColumnInfo);
            }
        }
 
        private void BuildMapOfEntityTypesWithTheirTableInfo(Assembly assembly)
        {
            foreach (var type in assembly.GetTypes())
            {
                var typeAttributes = Attribute.GetCustomAttributes(type, typeof(TableAttribute));
 
                if (typeAttributes.Length &gt; 0)
                {
                    var tableAttribute = (TableAttribute)typeAttributes[0];
                    var tableInfo = new TableInfo(this, tableAttribute.TableName, type);
                    typeToTableInfo.Add(type, tableInfo);
                }
            }
        }
 
        private void LoopThroughPropertiesWith&lt;TAttribute&gt;(Type entityType, TableInfo tableInfo,
            Action&lt;TableInfo, PropertyInfo, TAttribute&gt; andExecuteFollowingCode)
            where TAttribute : Attribute
        {
            foreach (var propertyInfo in entityType.GetProperties())
            {
                var attribute = GetAttribute&lt;TAttribute&gt;(propertyInfo);
 
                if (attribute != null)
                {
                    andExecuteFollowingCode(tableInfo, propertyInfo, attribute);
                }
            }
        }
 
        private void SetPrimaryKeyInfo(TableInfo tableInfo, PropertyInfo propertyInfo, PrimaryKeyAttribute primaryKeyAttribute)
        {
            tableInfo.PrimaryKey = new ColumnInfo(this, primaryKeyAttribute.ColumnName, propertyInfo.PropertyType, propertyInfo);
        }
 
        private void AddColumnInfo(TableInfo tableInfo, PropertyInfo propertyInfo, ColumnAttribute columnAttribute)
        {
            tableInfo.AddColumn(new ColumnInfo(this, columnAttribute.ColumnName, propertyInfo.PropertyType, propertyInfo));
        }
 
        private void AddReferenceInfo(TableInfo tableInfo, PropertyInfo propertyInfo, ReferenceAttribute referenceAttribute)
        {
            tableInfo.AddReference(new ReferenceInfo(this, referenceAttribute.ColumnName, propertyInfo.PropertyType, propertyInfo));
        }
 
        private TAttribute GetAttribute&lt;TAttribute&gt;(PropertyInfo propertyInfo) where TAttribute : Attribute
        {
            var attributes = Attribute.GetCustomAttributes(propertyInfo, typeof(TAttribute));
            if (attributes.Length == 0) return null;
            return (TAttribute)attributes[0];
        }
    }
[/csharp]
</div>

This class gives you the ability to retrieve the TableInfo class for a specfic entity type.  It also allows you to build the metadata model by passing in an assembly.  It will then loop through all of the types in the assembly to discover the types that have a TableAttribute, and it will then build the TableInfo objects with all of the information we need.

And that's all we need to create mappings between tables and our entities.  This wasn't hard, but it's not very powerful either.  We can't define custom user types that our DAL needs to be able to deal with, nor can we define any database inheritance strategies.  Our attributes are all inheritable, so you can use some inheritance with your entities, but you are essentially limited to the Table Per Class inheritance strategy.  Implementing support for the other inheritance strategies would obviously introduce a lot more complexity in the whole mapping aspect.

In the next post, i'll show you how this DAL will use TableInfo's methods to create CRUD statements to offer out-of-the-box CRUD functionality for each mapped entity.