I just released Agatha 1.0 Beta 2... while the changes aren't spectacular, i considered them important enough to release a new version.

Here's the changelog:

<ul>
	<li>Logging in the servicelayer now happens through the Common.Logging instead of using log4net directly... which means you can use whatever logging library you prefer</li>
	<li>(Breaking Change): If you want to pass your container instance to an Agatha container wrapper, you have to pass it to the constructor of the wrapper, and then pass that IContainer instance to your ServiceLayerConfiguration or ClientConfiguration instance before initializing</li>
	<li>Added Agatha.Unity and Agatha.Unity.Silverlight</li>
	<li>Added simple example of Agatha in use, with a small service layer, an IIS host for the service layer, a .NET client which performs a synchronous and an asynchronous client, and a Silverlight client which also performs an asynchronous call</li>
</ul>

The 2 most important changes are obviously that you can now integrate your own IOC container more easily and cleanly, and that you can use whatever logging library that you prefer.  If you download the source of beta 2, you'll also get the example solution which shows you how easily you can start using Agatha in your projects.

You can download the bits <a href="http://code.google.com/p/agatha-rrsl/downloads/list">here</a>.