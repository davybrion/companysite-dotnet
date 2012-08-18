# NHibernate Training

## Course description
NHibernate is the most powerful and advanced Object Relational Mapper available for the .NET platform. But with great power comes great responsibility and typically, a steep learning curve. This course aims to lower that learning curve to 3 intensive days where participants will learn all about the most important features of NHibernate, as well as be instructed on how to use those features responsibly to achieve the best possible results.

This course will be hands-on with exercises to be completed by the participants. Some experience with NHibernate is recommended, though not required for the fast learners amongst us. Experience with C# 3.0 or higher is required. This course currently targets NHibernate 3.2, and will be kept up to date with future NHibernate versions.

## What will you learn
- Thorough understanding of how an ORM works
- Typical and more advanced class mappings (both classic XML mapping as well as with Fluent NHibernate)
- Transitive persistence (aka 'persistance by reachability')
- Querying (HQL, Criteria, QueryOver and LINQ)
- Performance optimisations
- Dealing with concurrency
- Caching

## Who should attend
.NET developers who use NHibernate in their projects, or want to start using it. 

## Prerequisites
Developers should bring a laptop with either Visual Studio 2008 or Visual Studio 2010 installed. Note that express editions of Visual Studio won't be sufficient. Experience with C# 3.0 or higher is also required.

## NHibernate Training Course Syllabus

### Introduction to ORM/NHibernate
- Explains basic concepts that are key to how NHibernate works

### Configuring NHibernate
- Classic XML configuration
- Fluent NHibernate configuration

### Classes & Mappings
- Entity classes
- Value objects (components)
- Associations (one-to-one, one-to-many, many-to-one, many-to-many)
- Value type collections
- Component collections
- User types
- Transitive Persistence (fine-grained control over cascading changes)

### Identifier strategies
- Identity/Sequence
- Assigned Identities
- Guid.comb
- Hilo
- Overview of pro's and con's of each strategy

### Inheritance strategies
- Table per hierarchy
- Table per subclass
- Table per concrete class
- Overview of pro's and con's of each strategy

### Performance optimisations
- Leveraging proxies
- Paging
- Projecting
- Batching queries
- Fetching object graphs
- Leaning on session cache
- Batching inserts/updates/deletes
- Executable HQL
- Lazy collections
- Best Practices

### Session/Transaction management
- Best practices for dealing with sessions and transactions
- Optimistic concurrency strategies
- Pessimistic locking
- Stateless sessions

### Caching
- First level cache
- Second level cache