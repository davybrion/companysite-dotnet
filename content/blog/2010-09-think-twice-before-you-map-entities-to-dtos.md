One thing that i see a lot, and that i have largely started to avoid, is that people fetch entities with NHibernate, only to transform them to DTO's so you can send them back to the client so they can be displayed in a a grid or some kind of list or whatever.  This is usually pretty easy to do, especially if you already have a DTO mapper or are using something like Automapper.  But this comes with a bit of overhead (both performance and memory) that you can often avoid quite easily.

Suppose i have a screen where i need to display entries based on the following DTO class:

<script src="https://gist.github.com/3728197.js?file=s1.cs"></script>

This DTO basically contains data from 4 entities: Application, UserGroup, User and Permission.  I could easily do something like this with NHibernate:

<script src="https://gist.github.com/3728197.js?file=s2.cs"></script>

As you can see, that's very easy to do.  Unfortunately, this code really does a lot of stuff that you might not realize.  For starters, it retrieves all of the Authorization instances with its related Application, User, UserGroup and Permission instances.  It also fetches those entities in their entirety, which means it's retrieving all of their properties while i only need their Id and Name properties really.  And finally, NHibernate will set up all of the things that enable its magic for all of these entity instances.  That takes a few more CPU cycles and uses more memory than you truly need for the scenario of fetching entities merely to return DTO's.  This extra cost obviously becomes worse depending on the size of the resultset that you're fetching.

A better way to do this, is to simply let NHibernate fetch only the data (columns) that you need, and to let it populate the DTO's itself.  You can easily do this using projections and the AliasToBeanResultTransformer class.  The code would look like this:

<script src="https://gist.github.com/3728197.js?file=s3.cs"></script>

Granted, you need to write a bit more code.  But that code will do far less than the first version.