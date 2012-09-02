Just ran into something that i thought was pretty cool. If you're using <a href="http://watin.sourceforge.net/">WatiN</a>, it's relatively easy to write browser-based automated tests without resorting to recorded tests.  And since WatiN supports multiple browsers, you can write those tests in a browser-agnostic manner.  And if you make use of NUnit's Generic Fixtures (introduced in NUnit 2.5), you can very easily run those tests in multiple browsers as well.  Suppose you have the following base test fixture:

<div>
[csharp]
	public abstract class ViewTest&lt;TBrowser&gt; where TBrowser : Browser, new()
	{
		[TestFixtureSetUp]
		public void FixtureSetUp()
		{
			Browser = new TBrowser();
			Browser.GoTo(RootUrl);
		}

		[TestFixtureTearDown]
		public void FixtureTearDown()
		{
			if (Browser != null)
			{
				Browser.Dispose();
			}
		}

		[SetUp]
		public void SetUp()
		{
			DoSetUp();
		}

		protected TBrowser Browser { get; private set; }

		protected string RootUrl { get { return ConfigurationManager.AppSettings[&quot;RootUrl&quot;]; } }

		protected virtual void DoSetUp() { }
	}
[/csharp]
</div>

You can then write a test fixture like this:

<div>
[csharp]
	[TestFixture(typeof(IE))]
	[TestFixture(typeof(FireFox))]
	public class ListCustomersTests&lt;TBrowser&gt; : ViewTest&lt;TBrowser&gt; where TBrowser : Browser, new()
	{
		protected override void DoSetUp()
		{
			Browser.GoTo(RootUrl + &quot;Customers&quot;);
		}

		[Test]
		public void ShowsErrorMessageWhenClickingProceedWithoutSelectingAnItemFromGrid()
		{
			var proceedButton = Browser.Button(button =&gt; button.Value == &quot;Proceed&quot;);
			Assert.IsFalse(Browser.Para(&quot;selection_required&quot;).Exists);
			proceedButton.Click();
			Assert.That(Browser.Para(&quot;selection_required&quot;).Exists);
		}
	}
[/csharp]
</div>

And when you run your tests, it will run this test once in IE, and once in Firefox.

Resharper's TestRunner has issues with this though... it does run the tests, but it doesn't report any feedback on them. The normal NUnit testrunner does show the feedback correctly though.