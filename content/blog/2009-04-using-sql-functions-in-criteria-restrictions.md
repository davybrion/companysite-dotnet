A coworker needed to use a SQL function in the where clause of a query that he was creating with NHibernate's ICriteria API.  Most examples of this on the web use HQL instead of the ICriteria API and since we primarily use the ICriteria API we looked into how to do this.

Turns out it is pretty simple to do, though the syntax isn't really straightforward.  Suppose you want to query all of your employees who are born in a specific year.  You could mess around with some DateTime parameters, but most databases have SQL functions to get the year from a date.  Using the ICriteria API, this would look like this:

<div>
[csharp]
            var employeesBornIn81 = session.CreateCriteria&lt;Employee&gt;()
                .Add(Restrictions.Eq(Projections.SqlFunction(&quot;year&quot;, NHibernateUtil.DateTime, Projections.Property(&quot;BirthDate&quot;)), 1981))
                .List&lt;Employee&gt;();
[/csharp]
</div>

which adds the following where clause to the SQL statement (on SQL Server 2005):

WHERE datepart(year, this_.BirthDate) = @p0;