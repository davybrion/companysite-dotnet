Patrick Smacchia wrote an <a href="http://codebetter.com/blogs/patricksmacchia/archive/2008/12/08/advices-on-partitioning-code-through-net-assemblies.aspx">excellent post</a> on how you should partition your code in multiple assemblies.  Because this is one of those subjects that i'm <a href="/blog/2008/07/many-projects-dont-lead-to-a-good-solution/">pretty opinionated about</a>, i figured i'd quickly recap the valid and invalid reasons for creating assemblies that Patrick highlighted.

Valid reasons for creating a new assembly:

- Tier separation: separating the code you will run in different processes.  For instance: having an assembly for client-side only stuff, and one for server-side only stuff.
- Avoid premature loading of large pieces of code that aren't always necessary: if you put the 'optional' code in a separate assembly, that assembly won't be loaded until you actually need it.
- Framework feature separation: if your framework offers types that will never be used together (for instance: types for web development vs types for windows development) then it doesn't really make sense to put them in the same assembly.
- AddIn/Plugin model: there are many valid reasons for putting plugins in their own separate assemblies.
- Tests: i personally don't like to put my test code next to my production code, so i always put that in a separate assembly.
- Shared types: all 'common' types that you would like to use in different tiers.

Invalid reasons for creating a new assembly:

- Assemblies as units of development: there's not a single source control management system that doesn't make it easy for multiple people to work on the same assembly.
- Assemblies as units of reusability: this one wasn't on Patrick's list, but it is a pet peeve of mine.  I've often seen people separate things into multiple assemblies so each part could be reused separately.  Which is a prime example of Intellectual Masturbation Syndrome if there's no actual need to reuse each assembly separately.
- Automatic dependency cycle detection: when assemblies have cyclic dependencies on each other, Visual Studio automatically notifies you of the problem.  Patrick obviously recommends using NDepend to prevent such problems.  I'd recommend thinking before/during/after you write the code.
- Using internal visibility to hide implementation details: if you want to hide implementation details you can use the internal access modifier so the 'internal' parts are only available to the types contained in the owning assembly.  I seriously dislike this approach.  Using the internal access modifier is often a good sign that there is something wrong with the design.  Hiding implementation details is not always the same thing as 'proper encapsulation'!
- Using internal visibility to prevent usage from the rest of the code: Yet another approach that i seriously dislike.  I prefer to use the <a href="http://ayende.com/Blog/archive/2008/06/25/Public-vs.-Published.aspx">Published vs Public approach</a>.
