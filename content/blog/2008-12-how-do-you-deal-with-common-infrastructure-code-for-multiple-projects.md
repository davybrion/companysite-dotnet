Here's the situation: a couple of months ago we started developing according to a new architecture.  Obviously, you need infrastructure code for this.  For the first project, we just put the infrastructure code into the project's solution and everything was easy.  We could make changes as we needed them, and it enabled us to 'grow' the infrastructure into what we really needed.  Then came the second project.  I was reluctant to extract the infrastructure code in a separate reusable assembly because i felt it would lead to less flexibility to make changes.  So i copied the infrastructure classes into the new project.  Obviously, some changes were made in the classes of the second project, which weren't ported to the classes of the first project.  Add another project or two, and you can see the problem :)

So now we're trying to figure out how best to move forward.  I've got 3 options in mind:
<ol>
	<li>Infrastructure code as a separate project, binary ‘framework’ dependency per 'client' project</li>
	<li>Infrastructure code as a separate project, ‘framework’ dependency (in source form) per project (as in: copying the code of a specific version of the 'framework' into the project’s own repository)</li>
	<li>Each project just contains the infrastructure code in their own project and there is no specific 'framework'</li>
</ol>

The way i see it, each approach has its pro's and con's:

<ol>
	<li>Infrastructure code as a separate project, binary ‘framework’ dependency per 'client' project</li>

<ul>
	<li>Pro's</li>
<ul>
	<li>The code only has to be maintained in one place</li>
	<li>Everybody can benefit from changes</li>
</ul>
	<li>Con's</li>
<ul>
	<li>Can make debugging harder because you can’t step into the framework code</li>
	<li>Requires a lot of discipline for versioning and distributing updates to ‘client’ projects</li>
	<li>The infrastructure code has to have a lot of extensibility points so each application can add extra functionality</li>
</ul>
</ul>

</br>

	<li>Infrastructure code as a separate project, ‘framework’ dependency (in source form) per project (as in: copying the code of a specific version of the 'framework' into the project’s own repository)</li>

<ul>
	<li>Pro's</li>
<ul>
	<li>Does not have the debugging issue</li>
	<li>Code only has to be maintained in one place (in theory)</li>
	<li>Everybody can benefit from changes</li>
</ul>
	<li>Con's</li>
<ul>
	<li>The infrastructure code has to have a lot of extensibility points so each application can add extra functionality</li>
	<li>If people change the infrastructure code in their project, all changes should be sent upstream to the ‘real’ infrastructure repository, or extension points need to be provided in the original infrastructure code so upgrades of the infrastructure library still offer the same possibilities for the specific project</li>
	<li>Still requires versioning discipline, although it probably wouldn’t need to be as strict as with Option 1</li>
</ul>
</ul>

</br>
	<li>Each project just contains the infrastructure code in their own project and there is no specific 'framework'</li>

<ul>
	<li>Pro's</li>
<ul>
	<li>Highly flexible… each project can freely make changes to make the infrastructure behave exactly as it needs to for the project</li>
</ul>
	<li>Con's</li>
<ul>
	<li>Leads to multiple ‘versions’ of many of the classes… when a new project starts, which versions of each class should be used?</li>
	<li>Starting a new project contains boring set-up work which is basically just copy/pasting existing classes from previous projects</li>
</ul>
</ul>

</ol>

The reason i'm posting this, is because i'd love to get your feedback on this... what other pros/cons can you think of for each approach? Which approach would you recommend? Is there another approach we haven't thought of?

