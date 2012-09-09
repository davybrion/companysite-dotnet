I recently had to look into a minor issue one of the team members had with NHibernate.  Since i couldn't quickly find the solution online, i'm posting it here for future reference.

He had a criteria where he wanted to fetch all of the entities of a specific type, and have it joined with one of its associations.  Basically something like this:

<script src="https://gist.github.com/3684075.js?file=s1.cs"></script>

Then he needed to apply ordering on one of the joined association's properties.  I said "no problem, it can do that" and i changed the code to this:

<script src="https://gist.github.com/3684075.js?file=s2.cs"></script>

Executing that criteria gave the following error:

NHibernate.QueryException: could not resolve property: Category.Name of: Northwind.Domain.Entities.Product

Which is weird, because Product has a property called Category, which in turns has a property called Name. That should just work, right?  Apparently not... SetFetchMode merely instructs NHibernate how to fetch an association, but its usage does not mean you can just start adding extra options in the criteria for the entity's associations. If you want to do that, you need to add a subcriteria for the association to the first criteria (which you can then consider a parent criteria):

<script src="https://gist.github.com/3684075.js?file=s3.cs"></script>

In this version, we've added a second criteria to the first criteria.  Which means we can do everything with the second criteria (which is set to the given association) that we could normally do with a criteria.  So now, applying a sort Order to an association's property can be done like this:

<script src="https://gist.github.com/3684075.js?file=s4.cs"></script>

Or, if you want to be more explicit in the usage of your property names (to avoid confusion with the Name property of the Product entity for instance), you could assign an alias to the second criteria, and then use that alias in the AddOrder call:

<script src="https://gist.github.com/3684075.js?file=s5.cs"></script>