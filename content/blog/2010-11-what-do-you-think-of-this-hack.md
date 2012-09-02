I have a class which exposes a fluent interface to build something. Instances of this class contain some state based on the methods of the fluent interface you called and the arguments you passed to those methods.  Now, that state is currently private but it's not private in the "oh my god we need to encapsulate this so nobody can read it!11!!!1" sense.  In fact, i actually <em>want</em> to access that state from another class.  The only 'problem' is that i don't want to add methods or properties to the class to expose this state, because it sort-of pollutes the fluent interface.  I know, that's not really a huge issue but still, it'd be nice to keep the fluent interface clean and focused.

One way to expose this state without polluting the fluent interface is to create a separate interface which defines the methods/properties and then have the class implement that interface explicitly. That way you could only access those methods/properties when you cast the instance to the interface type. While there's nothing really wrong with doing that, i kinda have a bad feeling about that because it introduces an interface which is only there to support this little exercise in Intellectual Masturbation.

Instead, i tried this:

<div>
[csharp]
	public class MyClassWhichUsesAFluentInterface
	{
		private List&lt;string&gt; someState = new List&lt;string&gt;();

		public MyClassWhichUsesAFluentInterface SomethingFluent(string blah)
		{
			someState.Add(blah);
			return this;
		}

		// ...

		public static List&lt;string&gt; GetState(MyClassWhichUsesAFluentInterface constructedThingie)
		{
			return constructedThingie.someState;
		}
	}
[/csharp]
</div>

Is it fancy? nope.
Is it cool? nope.
Does it work? yup.
Is it simple? yup.

Good enough for me.