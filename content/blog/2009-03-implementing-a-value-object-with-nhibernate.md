I've covered <a href="http://davybrion.com/blog/2007/07/implementing-a-value-object/">implementing a Value Object</a> before, but this post is about using Value Objects with NHibernate.  First, a little recap of what a Value Object is for those who don't know yet.

A Value Object (also known as Immutable Object) is basically an object without a conceptual identity. A Value Object is defined through its inner values, and not an identity like Entities. This means that a Value Object’s inner values can not be changed after object creation, hence the term Immutable Object. Should you need to change the inner values of the Value Object, you should actually create a new Value Object.

For some of you, this might seem odd. But you’ve actually used Value Objects on many occasions already. In .NET, strings are Value Objects. So are DateTime instances. If you create a string, you can’t modify its inner value. If you do, a new string is actually created. Same thing with a DateTime. The DateTime class provides methods to add days, hours, seconds, whatever… but those methods never modify the instance’s inner value. Instead, they return a new DateTime object because each DateTime instance is immutable.

This has interesting consequences on object equality. Two Value Objects holding the same data should be considered identical objects, even though they are 2 different instances.  A Value Object should therefore properly implement the Equals and GetHashCode methods.

For this example, we'll define a Name class, which is a Value Object consisting of 2 values: FirstName and LastName.  If 2 people have the same first and last name, you could say that they have the same 'name', right? We'll just ignore middle names here...  The combination of a FirstName and LastName would make for a good Value Object.  We need to make sure that once an instance of the Name class has been created, none of its values can be modified.  We also need to make sure that multiple instances of the Name class that have the same values can be safely considered equal to each other.  The code of the Name class looks like this:

<div>
[csharp]
    public class Name : IEquatable&lt;Name&gt;
    {
        private readonly string firstName;
        private readonly string lastName;
 
        public Name(string firstName, string lastName)
        {
            this.firstName = firstName;
            this.lastName = lastName;
        }
 
        // the default constructor is only here for NH (private is sufficient, it doesn't need to be public)
        private Name() : this(string.Empty, string.Empty) {}
 
        public string LastName
        {
            get { return lastName; }
        }
 
        public string FirstName
        {
            get { return firstName; }
        }
 
        public bool Equals(Name other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Equals(other.firstName, firstName) &amp;&amp; Equals(other.lastName, lastName);
        }
 
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != typeof(Name)) return false;
            return Equals((Name)obj);
        }
 
        public override int GetHashCode()
        {
            unchecked
            {
                return (firstName.GetHashCode() * 397) ^ lastName.GetHashCode();
            }
        }
 
        public static bool operator ==(Name left, Name right)
        {
            return Equals(left, right);
        }
 
        public static bool operator !=(Name left, Name right)
        {
            return !Equals(left, right);
        }
    }
[/csharp]
</div>

Take a close look at the constructors.  The public constructor takes both required values (firstName and lastName), and assigns them to the private fields.  The default constructor (which we've made private) merely calls the public constructor and passes String.Empty to the public constructor's parameters.  As you can gather from the comments on the private constructor, it's only reason for existence is because NHibernate requires classes to have a default constructor.  Well actually, that's not entirely accurate since it is possible to use classes without a default constructor but it's not trivial do so. 

Creating a private default constructor seems to be a reasonable alternative.  Developers can't create invalid Name instances (unless they cheat with reflection), and NHibernate can use the private constructor so it can create the instances before it fills the fields with the values from the database.  

Note: NHibernate allows a private default constructor for Value Objects, but for Entities you will need a default public or protected constructor as private is not sufficient.

We can now use this Value Object in every entity we want by simply adding a property to the entity like this:

<div>
[csharp]
        public virtual Name Name { get; set; }
[/csharp]
</div>

The mapping of the Value Object must be added to the mapping of the entity like this:

<div>
[xml]
    &lt;component name=&quot;Name&quot; class=&quot;NHibernateExamples.Values.Name&quot; insert=&quot;true&quot; update=&quot;true&quot;&gt;
      &lt;property name=&quot;FirstName&quot; column=&quot;FirstName&quot; type=&quot;string&quot; length=&quot;50&quot; not-null=&quot;true&quot; access=&quot;nosetter.camelcase&quot; /&gt;
      &lt;property name=&quot;LastName&quot; column=&quot;LastName&quot; type=&quot;string&quot; length=&quot;50&quot; not-null=&quot;true&quot; access=&quot;nosetter.camelcase&quot; /&gt;
    &lt;/component&gt;
[/xml]
</div>

Notice the value of the access attribute.  It's set to nosetter.camelcase.  That means that NHibernate will use the get property when reading the values, but it will use a camelcase private field to set the values when it's creating the object with values from the database.
