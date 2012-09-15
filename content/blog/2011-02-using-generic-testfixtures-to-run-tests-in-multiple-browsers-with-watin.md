Just ran into something that i thought was pretty cool. If you're using <a href="http://watin.sourceforge.net/">WatiN</a>, it's relatively easy to write browser-based automated tests without resorting to recorded tests.  And since WatiN supports multiple browsers, you can write those tests in a browser-agnostic manner.  And if you make use of NUnit's Generic Fixtures (introduced in NUnit 2.5), you can very easily run those tests in multiple browsers as well.  Suppose you have the following base test fixture:

<script src="https://gist.github.com/3728675.js?file=s1.cs"></script>

You can then write a test fixture like this:

<script src="https://gist.github.com/3728675.js?file=s2.cs"></script>

And when you run your tests, it will run this test once in IE, and once in Firefox.

Resharper's TestRunner has issues with this though... it does run the tests, but it doesn't report any feedback on them. The normal NUnit testrunner does show the feedback correctly though.