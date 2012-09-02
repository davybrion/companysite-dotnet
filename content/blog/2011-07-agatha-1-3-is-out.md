It's been a while since the last release (september 2010) but there is a major new feature which warrants a new one. And since i've noticed an increased interest in Agatha in the past few weeks, the timing could not have been better. 

One of the few (IMO) problems that Agatha suffered from, was that it sorta pushed you down the inheritance-road if you wanted to deal with cross-cutting concerns. Well, not anymore. Earlier this year, Bart Deleye started working on a way to use interceptor classes to do the same thing. It enables you to write small interceptor classes which just focus on one thing and have them executed before and after requests are processed. It's sort of similar to global action filters in ASP.NET MVC3, except that it's not attribute based. The order of execution is also guaranteed to be in the order in which the interceptors were registered. Bart and his team have been using this for a while now in their own projects, and recently contributed the changes back to the project. You can read more about these interceptors <a href="https://github.com/davybrion/Agatha/wiki/RequestHandlerInterceptors" target="_blank">here</a> and you can find some real examples <a href="https://github.com/davybrion/Agatha/wiki/RequestHandlerInterceptorsExamples" target="_blank">here</a>.

Another problem that Agatha suffered from was that there was no way to definitively know which type of Response corresponded with a certain type of Request. Because that was important to the interceptors feature, Bart went ahead and implemented something for that as well, so you can now define request/response conventions which can be used in your code. You can read about those <a href="https://github.com/davybrion/Agatha/wiki/Conventions" target="_blank">here</a>.

Those are the two most important new features, but some of you will be glad to hear that we finally have nuget packages as well. Because of the problem i described <a href="http://davybrion.com/blog/2011/03/a-nuget-packaging-dilemma/" target="_blank">here</a>, i ended up with 8 nuget packages:
<ul>
	<li><a href="http://nuget.org/List/Packages/Agatha-Common-Castle-Windsor" target="_blank">Agatha-Common-Castle-Windsor</a></li>
	<li><a href="http://nuget.org/List/Packages/Agatha-ServiceLayer-Castle-Windsor" target="_blank">Agatha-ServiceLayer-Castle-Windsor</a></li>
	<li><a href="http://nuget.org/List/Packages/Agatha-Common-Ninject" target="_blank">Agatha-Common-Ninject</a></li>
	<li><a href="http://nuget.org/List/Packages/Agatha-ServiceLayer-Ninject" target="_blank">Agatha-ServiceLayer-Ninject</a></li>
	<li><a href="http://nuget.org/List/Packages/Agatha-Common-StructureMap" target="_blank">Agatha-Common-StructureMap</a></li>
	<li><a href="http://nuget.org/List/Packages/Agatha-ServiceLayer-StructureMap" target="_blank">Agatha-ServiceLayer-StructureMap</a></li>
	<li><a href="http://nuget.org/List/Packages/Agatha-Common-Unity" target="_blank">Agatha-Common-Unity</a></li>
	<li><a href="http://nuget.org/List/Packages/Agatha-ServiceLayer-Unity" target="_blank">Agatha-ServiceLayer-Unity</a></li>
</ul>

The packages for Castle Windsor, Unity and Ninject each support Silverlight as well. All container dependencies are now based on their latest Nuget packages. With Agatha 1.2, it was clear that the majority of people downloaded the source code instead of the binary release (i presume to build against whatever version of container they used) so you can still get the entire source package on <a href="http://code.google.com/p/agatha-rrsl/downloads/list" target="_blank">Google Code</a> or on <a href="https://github.com/davybrion/Agatha/zipball/agatha-1.3" target="_blank">Github</a>. I didn't create another binary package since i figured that most people who want that will be using Nuget anyway.

The two other changes are:
<ul>
	<li>now possible to POST to the IWcfRestJsonRequestProcessor (contributed by Andrew Rea)</li>
	<li>our container abstraction allows key-based resolving (contributed by Nikos Baxevanis)</li>
</ul>

My favorite part about this release is that all of the new code has been contributed by other people :)

For 1.4, the focus might be more about making Agatha as easy as possible to consume from JavaScript, which may or may not introduce a new REST based service interface.  Though none of that is certain at this point, so we'll see how it goes :)