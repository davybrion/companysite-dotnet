I've been thinking about how <a href="http://code.google.com/p/agatha-rrsl/">Agatha</a> should be packaged for <a href="http://nuget.org/">Nuget</a> and i just can't come up with something that i like. The problem is that Nuget misses a concept that is necessary for packages which depend on one of many possible dependencies.  Agatha requires an Inversion Of Control container, but it allows you to use the container you prefer.  I've talked before about the <a href="http://davybrion.com/blog/2010/02/when-it-comes-to-ioc-containers-we-seem-to-be-pretty-loyal/">loyalty</a> that many of us show to our preferred container, and i'm pretty sure that forcing users to use a specific container would severely limit the number of possible users Agatha could attract.  So right now, Agatha users need to pick which container they want to use.

Now how do you make that fit into the Nuget packaging system? Ideally, Nuget would someday support a scenario where i can define that my package requires the user to also select one package from a list of suitable packages but until that's possible i can't really come up with a solution that i like.  Here's what i specifically don't want:

<ul>
	<li>I don't want the package to include something that the user might not want.</li>
	<li>I don't want to force a particular container on users</li>
	<li>I don't want to ILMerge any container assemblies because that would cause problems if users themselves make use of a container already.</li>
	<li>I don't want to do it like the current <a href="http://nuget.org/List/Packages/NHibernate">NHibernate 3.1 package</a> because that pretty much guarantees nobody will install it. NHibernate can get away with something like that because it's already an established project.</li>
</ul>

Ideally, i'd be able to define the following package structure:
<ul>
	<li>Agatha-RRSL, requires one of the following packages to be installed (if no selection is made, go with Agatha-Windsor by default):
<ul>
	<li>Agatha-Windsor, depends on Agatha-RRSL and Castle Windsor</li>
	<li>Agatha-StructureMap, depends on Agatha-RRSL and StructureMap</li>
	<li>Agatha-NInject, depends on Agatha-RRSL and NInject</li>
	<li>... (one for each supported container)</li>
</ul>
</li>
</ul>

If that were possible, users wouldn't have to include anything they don't like and it could work together nicely with the packages of the various IOC containers. 

As for how it should be packaged for now, i still have no idea. Suggestions are welcome :)