I always tell people to explicitly specify the lengths of their string properties in their NHibernate mappings for performance reasons. If you don't specify them, the ADO.NET parameter lengths of those strings will always be set to the length of the actual string value that's been assigned to the parameter. This is a problem for SQL Server, because it can't cache those statements as efficiently as it would if the parameter lengths were always the same for a given statement. Simply put, if you don't specify the lengths, SQL Server's statement cache gets polluted with a bunch of statements that are often the same, but they're considered to be different simply because of the lengths of those string parameters. And this can really hurt the performance of your application. 

Of course, not everyone remembers to set those lengths, so I thought it'd be great if I could force people to do this. With a little creative use of Fluent NHibernate's conventions, it's quite easy to enforce this:

<div>
[csharp]
public class StringsMustHaveLengthConvention: IPropertyConvention, IPropertyConventionAcceptance
{
	public void Apply(IPropertyInstance instance)
	{
		var msg = string.Format(&quot;The string property '{0}' of type '{1}' does not have a length value specified, &quot; +
			&quot;which is required for performance reasons. Add something like this to your mapping override:\r\n&quot; + 
			&quot;\tmapping.Map(e =&gt; e.{0}).Length(50); // with an appropriate length for this property&quot;,
			instance.Property.Name, instance.EntityType.Name);

		throw new MappingException(msg);
	}

	public void Accept(IAcceptanceCriteria&lt;IPropertyInspector&gt; criteria)
	{
		criteria.Expect(x =&gt; x.Type == typeof(string)).Expect(x =&gt; x.Length == 0);
	}
}
[/csharp]
</div>

With that convention in place, you won't even be able to run your code until you've specified the string lengths :)