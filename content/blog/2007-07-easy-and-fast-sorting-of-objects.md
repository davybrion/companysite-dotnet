A <a href="http://peterbauwens.wordpress.com">coworker</a> of mine recently pointed me to a <a href="http://musingmarc.blogspot.com/2006/02/dynamic-sorting-of-objects-using.html">blogpost</a> by Marc Brooks which contains a fantastic library for sorting objects. The objects don't have to implement an interface or inherit from a base class, it pretty much works with whatever you throw at it.  And it's <strong>very</strong> fast.

Suppose you have the following class:

<div>
[csharp]
public class MyClass
{
  private int _intValue;
  private double _doubleValue;
  private string _stringValue;
  private decimal _decimalValue;
  private DateTime _dateTimeValue;

  public int IntValue
  {
    get { return _intValue; }
    set { _intValue = value; }
  }

  public double DoubleValue
  {
    get { return _doubleValue; }
    set { _doubleValue = value; }
  }

  public string StringValue
  {
    get { return _stringValue; }
    set { _stringValue = value; }
  }

  public decimal DecimalValue
  {
    get { return _decimalValue; }
    set { _decimalValue = value; }
  }

  public DateTime DateTimeValue
  {
    get { return _dateTimeValue; }
    set { _dateTimeValue = value; }
  }
}
[/csharp]
</div>

And suppose we have a method which creates a big list filled with instances of this class for us:

<div>
[csharp]
private static List&amp;lt;MyClass&amp;gt; CreateBigList()
{
  List&lt;MyClass&gt; list = new List&lt;MyClass&gt;();

  for (int a = 0; a &lt; 10; a++)
  {
    for (int b = 0; b &lt; 10; b++)
    {
      for (int c = 0; c &lt; 10; c++)
      {
        for (int d = 0; d &lt; 10; d++)
        {
          for (int e = 0; e &lt; 10; e++)
          {
            MyClass myObject = new MyClass();
            myObject.IntValue = a;
            myObject.DoubleValue = b;
            myObject.StringValue = &quot;string&quot; + c;
            myObject.DecimalValue = d;
            myObject.DateTimeValue = DateTime.Now.AddSeconds(e);

            list.Add(myObject);
          }
        }
      }
    }
  }

  return list;
}
[/csharp]
</div>
The list will contain 100,000 instances, ordered ascending.

Using Marc Brooks' DynamicComparer, it's incredibly easy to sort this list differently:

<div>
[csharp]
static void Main(string[] args)
{
  List&lt;MyClass&gt; list = CreateBigList();

  string sortExpression = &quot;IntValue DESC, DoubleValue DESC, &quot;
      + &quot;StringValue DESC, DecimalValue DESC, DateTimeValue DESC&quot;;

  DynamicComparer&lt;MyClass&gt; dynamicComparer =
      new DynamicComparer&lt;MyClass&gt;(sortExpression);

  DateTime before = DateTime.Now;
  list.Sort(dynamicComparer.Comparer);
  DateTime after = DateTime.Now;

  Console.WriteLine(&quot;Elapsed Time: &quot; + after.Subtract(before));
  Console.ReadLine();
}
[/csharp]
</div>

Based on the sortExpression, the list will now be sorted in the reversed order. On my computer, the output of this code was:

Elapsed Time: 00:00:00.5908496

I'd say that's pretty fast for sorting 100,000 instances on 5 properties. And incredibly easy.
