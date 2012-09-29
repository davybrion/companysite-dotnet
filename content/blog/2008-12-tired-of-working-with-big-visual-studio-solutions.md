Ever noticed how Visual Studio can be painfully slow when it comes to working with big solutions? It starts using large amounts of RAM, building the project takes way too long, and with practically every change you make it has to rebuild a lot of the projects in the solution which can waste a tremendous amount of time if you add it all up.

Consider the following solution:

<img src="/postcontent/full_solution_tree.png" alt="full_solution_tree" title="full_solution_tree" width="238" height="344" />

Now, I'm not going to get into the specifics of each project in this solution... most of these projects were created before I ever got involved with this project, and I'm not really happy with the entire structure.  It's a pretty big application, and a while ago we decided to move to a new architecture.  But since we can't just rewrite the whole thing, we put the new stuff (using the new architecture) in the same application and we're going to gradually rewrite the old parts using the new architecture.  

I wanted to keep the new stuff completely separated from the old stuff, so I added more projects to it (the EMS.* projects).  Before I added the new projects to the solution, it was already painfully slow to use this solution with Visual Studio.  After adding the new projects, it obviously only got worse.  Since we're spending most of our development work in the new projects, I wanted to see if I could simply create a new solution which would contain only the projects we usually need.  That new solution looks like this:

<img src="/postcontent/smaller_solution_tree.png" alt="smaller_solution_tree" title="smaller_solution_tree" width="267" height="131"  />

Much better.... but now you're probably thinking: doesn't CMS.WebApplication reference any of the other projects? It does reference a few of them actually:

<img src="/postcontent/dependencies.png" alt="dependencies" title="dependencies" width="165" height="134" />

Visual Studio indicates that it can't resolve these references.  So this new solution isn't usable, right? Well, it is actually.  You just have to make sure that you've done a full build of the entire solution (the one that has all of the projects in it) before you build the small one.  If you use the small solution after you've built the big one, Visual Studio is smart enough to remember where it got those compiled dependencies from in the first place.

So is this really usable? It sure is... we do most of our work in the smaller solution, and we can modify and recompile as much as we want without problems and without wasting huge amounts of time just waiting for Visual Studio and the compiler.  The only issue we have with this approach is when we need to make changes in some of the older projects that aren't in the small solution.  Whenever someone makes a change there that requires a recompile of the CMS.WebApplication project, every teammember needs to recompile the entire big solution.  But to avoid having to load the entire solution in Visual Studio, you can just run the following command in a Visual Studio 2008 Command Prompt:

<code>
msbuild ems.sln
</code>

and it builds the entire solution without using Visual Studio.  After that, your smaller solution will work again.

If you're working with big Visual Studio solutions and the slowness of this bothers you, be sure to give this a shot.  You can create as many of these small solutions as you like, depending on which parts of the codebase you typically need to work with.  It can easily save you a lot of time, and avoid unnecessary frustration as well :)

For new solutions, I think it's better to just keep the number of projects to a minimum which I've explained previously <a href="/blog/2008/07/many-projects-dont-lead-to-a-good-solution/">here</a>