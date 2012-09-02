I wrote a <a href="http://davybrion.com/blog/2010/01/virtual-method-performance-penalty">post</a> about a year ago which discussed a test of the performance difference between calling virtual methods and non-virtual methods. This morning, someone added the following <a href="http://davybrion.com/blog/2010/01/virtual-method-performance-penalty/#comment-83122">comment</a> to that post:

<blockquote>if you have 100 subclasses of class A, and they all override a method a, it will take a lot longer for it to figure out which version of a to call. Think of it as a switch statement with one case label verses a switch statement with 100 case labels. Since you’re just testing it with one method it’s not surprising that the cost is negligible.</blockquote>

My Bullshit-detector started beeping while reading that, so i just had to see if the number of subclasses indeed had an impact.  I didn't go all the way up to 100 subclasses, but i went with 15.  If there is indeed a performance penalty that grows with the number of subclasses in play, then surely i'd have to see <em>some difference</em> when using 15 subclasses over just 1, right? 

In the original test, i had the following 2 classes:

<div>
[csharp]
	public class MyClass
	{
		public long someLong;

		public void IncreaseLong()
		{
			someLong++;
		}

		public virtual void VirtualIncreaseLong()
		{
			someLong++;
		}
	}

	public class MyDerivedClass : MyClass
	{
		public override void VirtualIncreaseLong()
		{
			someLong += 2;
		}
	}
[/csharp]
</div>

Now, i wasn't quite sure whether the commenter meant having a bunch of classes that inherited directly from MyClass, or having a set of inheriting classes in a deep inheritance tree.  Just to be sure, i tested both cases.

In the first case, i have classes like MyDerivedClass1, MyDerivedClass2, ... , MyDerivedClass15 that all inherit directly from MyClass.  In the second case, MyDerivedClass1 inherits from MyClass, MyDerivedClass2 inherits from MyDerivedClass1, ... , and MyDerivedClass15 inherits from MyDerivedClass14.

The code of the test is still largely the same as it was in the previous post, with just some minor modifications to make sure that more of the code to be executed has been JIT'ed prior to the actual test-run:

<div>
[csharp]
	class Program
	{
		const int iterations = 1000000000;

		static void Main(string[] args)
		{
			var myObject = new MyClass();
			var myDerivedObject = new MyDerivedClass15();

			// we do this so there's no first-time performance cost while timing
			EnsureThatEverythingHasBeenJitted(myObject);
			EnsureThatEverythingHasBeenJitted(myDerivedObject);

			TestNormalIncreaseMethod(myObject, iterations);
			TestVirtualIncreaseMethod(myObject, iterations);

			TestNormalIncreaseMethod(myDerivedObject, iterations);
			TestVirtualIncreaseMethod(myDerivedObject, iterations);

			Console.ReadLine();
		}

		static void EnsureThatEverythingHasBeenJitted(MyClass theObject)
		{
			theObject.IncreaseLong();
			theObject.VirtualIncreaseLong();
			TestNormalIncreaseMethod(theObject, 1, false);
			TestVirtualIncreaseMethod(theObject, 1, false);
		}

		static void TestNormalIncreaseMethod(MyClass theObject, int numberOfTimes, bool printToConsole = true)
		{
			if (printToConsole) Console.WriteLine(string.Format(&quot;calling the IncreaseLong method of type {0} {1} times&quot;, theObject.GetType().Name, numberOfTimes));
			
			var stopwatch = Stopwatch.StartNew();
			for (var i = 0; i &lt; numberOfTimes; i++)
			{
				theObject.IncreaseLong();
			}
			stopwatch.Stop();

			if (printToConsole) Console.WriteLine(&quot;Elapsed milliseconds: &quot; + stopwatch.ElapsedMilliseconds);
		}

		static void TestVirtualIncreaseMethod(MyClass theObject, int numberOfTimes, bool printToConsole = true)
		{
			if (printToConsole) Console.WriteLine(string.Format(&quot;calling the VirtualIncreaseLong method of type {0} {1} times&quot;, theObject.GetType().Name, numberOfTimes));

			var stopwatch = Stopwatch.StartNew();
			for (var i = 0; i &lt; numberOfTimes; i++)
			{
				theObject.VirtualIncreaseLong();
			}
			stopwatch.Stop();

			if (printToConsole) Console.WriteLine(&quot;Elapsed milliseconds: &quot; + stopwatch.ElapsedMilliseconds);
		}
	}
[/csharp]
</div>

In the first test (multiple direct subclasses of MyClass) i got the following result:

<a href="http://davybrion.com/blog/wp-content/uploads/2010/12/fifteen_subclasses.png"><img src="http://davybrion.com/blog/wp-content/uploads/2010/12/fifteen_subclasses.png" alt="fifteen subclasses" title="fifteen_subclasses" width="642" height="171" class="aligncenter size-full wp-image-2956" /></a>

(note: for this test, i used MyDerivedClass1 instead of MyDerivedClass15 as in the listed code)

In the second test (inheritance tree) i got the following result:

<a href="http://davybrion.com/blog/wp-content/uploads/2010/12/fifteen_nested_subclasses.png"><img src="http://davybrion.com/blog/wp-content/uploads/2010/12/fifteen_nested_subclasses.png" alt="fifteen nested subclasses" title="fifteen_nested_subclasses" width="651" height="173" class="aligncenter size-full wp-image-2957" /></a>

As you can once again see, the difference is completely negligible. So here's what i propose: until someone actually shows a case where a clear-cut performance penalty is shown that is even slightly relevant to real-world usage, we should just drop the whole "virtual methods are expensive!"-thing.