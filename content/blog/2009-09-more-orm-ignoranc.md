Tony Davis from <a href="http://www.simple-talk.com/">simple-talk</a> wrote a post about the so-called <a href="http://www.simple-talk.com/community/blogs/tony_davis/archive/2009/09/03/74643.aspx">ORM Brouhaha</a> that followed from the recent benchmarking fiasco.  I have some problems with some of the statements in his post, and since he loves to hear what we think i decided to share my thoughts on this.

He starts off the post with the following:

> A while back, Laila Lotfi wrote an editorial on the need for a standard benchmark for Object-Relational mappers, such as Entity Framework and nHibernate. By how much do they really slow down database applications?

ORM's slow down database applications? That's a generalization that i believe many of us can make about stored procedures or classic data access layers as well, after all, they are quite frequently used as incorrectly as ORM's are often misused.  First of all, the purpose of an ORM is not just to ease your development tasks or to keep your code free from repetitive cruft.  It is as much about optimizing your usage of the database as it is about ease of development.

Any decent ORM will aim to minimize overhead when it comes to communicating with the database.  Generally speaking, an ORM should try to send as few statements to the database as possible, either through usage of batching techniques, being able to generate good queries, and not executing unnecessary statements.  How many times have we seen data access code that doesn't have proper dirty checking and sends update statements for entities that haven't really been modified? I've seen plenty of implementations where doing something like myEntity.SomeProperty = myEntity.SomeProperty still resulted in an unnecessary update statement because a dirty flag was set in a naive manner.  A good ORM will not do this, and will try to keep the overhead of communicating with the database as low as possible.

> There many complaints to the effect that the benchmark tests are useless, because the tool "should never be used in that way". If it can be, it almost certainly will be; and it is up to the tool creator to make sure that it stands up as well as its competitors.

Is it the responsibility of a tool creator to make sure it performs as well as it possibly can in usage scenarios that are actively discouraged?  Should we hold everyone to this rule?  How about we apply that logic to databases or DBA's?  Using the same twisted logic, we could actually blame bad database performance on database implementations or DBA's because hey, we are able to use them badly, so it is their responsibility to make sure that it performs as well as it possibly can.  Seems like a bit of a ridiculous statement, no?  This kind of 'logic' actually conflicts with the problems that many people incorrectly associate with ORM's so i think we can safely ignore this.

> I imagine the sight of such a brawl sent a chill down the spine of managers who may have been planning to use ORM technology.

Pardon me for being blunt, but having to follow the technical decisions from a manager who holds value to the results of silly, unrealistic benchmarks is a situation that would send chills down my spine as well.

> The IT industry is increasingly coming to suspect that the performance and scalability issues that come from use of ORMs outweigh the benefits of ease and the speed of development.

I haven't really noticed this so called trend.  In fact, i'm under the impression that usage of ORM's is finally becoming more and more accepted, especially in the .NET world.  In the Java world, where i think we can objectively state that many large-scale and highly performant applications have been developed over the years, ORM usage is pretty prevalent. 

And seriously, how many of us have had to deal with applications or systems that didn't use an ORM but used either a classic data access layer or stored procedures and still suffered from bad database performance?  I think the large majority of the development community has experience with exactly this kind of situation, so i really don't think ORM's are the problem here...

> We are crying out for objective benchmarks and if the ORM industry itself cannot hope to agree on how to do it, then perhaps benchmarks will have to be imposed on them.

Impose all you want, but don't be surprised if those benchmarks will be ignored or questioned by those who hold more value to properly educating developers instead of basing their decisions on irrelevant benchmarks.