Lost some time yesterday trying to get something working with MS Test (not my choice, but that's what my client uses) that i had expected to be easy. After all, it was especially easy to get working with NUnit.  I wanted to create a base testing fixture which would instantiate one instance of Internet Explorer for the entire test run, and make that instance available to each test in the assembly. Sounds easy, no?

<strong>First problem: MS Test runs each test on a different thread. </strong>

When you use IE through WatiN, it uses COM behind the scenes. Accessing COM objects from different threads is not a safe thing to do and can lead to the following exception:
System.Runtime.InteropServices.InvalidComObjectException: COM object that has been separated from its underlying RCW cannot be used.

Running each test individually worked, but running the entire suit made every test except for the first one fail with that exception because MS Test uses a different thread for each test (i suppose the development team did that to make sure it was enterprisey).  Quite annoying, but luckily for me, the only other guy in the world who uses MS Test with WatiN also ran into the same problem and he described his workaround on his <a href="http://watinandmore.blogspot.com/2009/03/reusing-ie-instance-in-vs-test.html">blog</a>.

I made minor modifications to his IEStaticInstanceHelper class (basically just turned it into a static class) so my version looks like this:

<div>
[csharp]
	public static class IEStaticInstanceHelper
	{
		// TODO: move this to a config file
		public const string ROOT_URL = &quot;http://localhost:13834/&quot;;

		private static IE _ie;
		private static int _previouslyKnownIeThreadHashCode;
		private static string _ieHwnd;

		public static void Initialize()
		{
			IE = new IE(ROOT_URL);
		}

		public static IE IE
		{
			get
			{
				if (GetCurrentThreadHashCode() != _previouslyKnownIeThreadHashCode)
				{
					_ie = Browser.AttachTo&lt;IE&gt;(Find.By(&quot;hwnd&quot;, _ieHwnd));
					_previouslyKnownIeThreadHashCode = GetCurrentThreadHashCode();
				}
				return _ie;
			}
			private set
			{
				_ie = value;
				_ieHwnd = _ie.hWnd.ToString();
				_previouslyKnownIeThreadHashCode = GetCurrentThreadHashCode();
			}
		}

		private static int GetCurrentThreadHashCode()
		{
			return Thread.CurrentThread.GetHashCode();
		}
	}
[/csharp]
</div>

I also had the following AssemblyInitialize and AssemblyCleanup methods:

<div>
[csharp]
		[AssemblyInitialize]
		public static void AssemblyInitialize(TestContext testContext)
		{
			IEStaticInstanceHelper.Initialize();
		}

		[AssemblyCleanup]
		public static void AssemblyCleanup()
		{
			if (IEStaticInstanceHelper.IE != null)
			{
				IEStaticInstanceHelper.IE.Close();
				IEStaticInstanceHelper.IE.Dispose();
			}
		}
[/csharp]
</div>

MS Test will call the AssemblyInitialize method before any test in the assembly is executed, provided that you don't forget to add the TestContext parameter to your method or it will silently be ignored (WTF?!). It'll also call the AssemblyCleanup method after each test in the assembly has finished executing.

<strong>Second problem: MS Test runs the AssemblyCleanup method in an MTA thread, even though each test is executed in STA threads by default. </strong>

As you can see in my AssemblyCleanup method, i access the IE property of IEStaticInstanceHelper.  That property getter contains the following line:

<div>
[csharp]
	_ie = Browser.AttachTo&lt;IE&gt;(Find.By(&quot;hwnd&quot;, _ieHwnd));
[/csharp]
</div>

That line works perfectly during the execution of the tests.  When it is called from the AssemblyCleanup method, it times out after 30 seconds because it can't seem to find the IE window with the handle (_ieHwnd) that is known to be valid.  And this, apparently, is because the current thread is an MTA thread when we're within the AssemblyCleanup method instead of an STA thread.  I can't for the life of me figure out why they'd use an MTA thread for the AssemblyCleanup method while they use STA threads for the tests, but i will again assume it was done to keep up to the high enterprisey standard that people expect from something like MS Test.

The solution, while a horrible hack, is quite simple and works perfectly:

<div>
[csharp]
		[AssemblyCleanup]
		public static void AssemblyCleanup()
		{
			var thread = new Thread(() =&gt;
			{
				if (IEStaticInstanceHelper.IE != null)
				{
					IEStaticInstanceHelper.IE.Close();
					IEStaticInstanceHelper.IE.Dispose();
				}
			});

			thread.SetApartmentState(ApartmentState.STA);
			thread.Start();
			thread.Join();
		}
[/csharp]
</div>

There... nice and enterprisey.