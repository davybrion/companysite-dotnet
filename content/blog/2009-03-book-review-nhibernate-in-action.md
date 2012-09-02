As far as Object Relational Mapping (ORM) in the .NET world goes, NHibernate is one of the more mature, powerful and popular options available.  But learning how to use it properly can take some time.  <a href="http://www.amazon.com/NHibernate-Action-Pierre-Henri-Kuat%C3%A9/dp/1932394923/ref=pd_bbs_sr_1?ie=UTF8&s=books&qid=1237130779&sr=8-1">NHibernate In Action</a> aims to lower that learning curve significantly.

The first 2 chapters offer a general introduction to ORM, some reasons why NHibernate is the authors' preferred ORM, and a step-by-step guide to creating your first NHibernate application.  Topics such as NHibernate's (high-level) architecture and configuration are also covered here.

The book then proceeds to cover some important NHibernate concepts in depth.  First up, writing and mapping classes.  This particular chapter doesn't cover really advanced mapping scenarios (a later chapter takes care of that), but it does cover most of your options very thoroughly.  This isn't just basic mapping though.  You'll already learn about various types of associations you can have and the three available strategies for inheritance mapping.

Once you've learned how you can map your objects, you can start working with them, which is what chapter 4 covers.  You'll learn about the persistence lifecycle, working with transient/persistent/detached objects, dealing with object identity depending on the current state of your objects, transitive persistence (cascading options), and you are introduced to some of the possibilities for object retrieval.

Chapter 5 teaches you how to deal with transactions, the various options to deal with concurrency and it also covers NHibernate's caching features very thoroughly.  All 3 of those topics should be very clear to you after this chapter :)

After that, the authors revisit the subject of object mapping again in Chapter 6, this time covering some more advanced options.  The stuff you'll learn about in this chapter is very interesting, but in most cases you won't need these features.  But it's good to know that they are there in case you need to handle more exotic mapping situations ;)

Chapter 7 is probably one of the most important chapters of the book, as it deals with various ways and methods to retrieve objects efficiently.  You'll get great coverage of HQL in this chapter, and you'd be surprised how powerful it is.  In most cases, you'll see the equivalent of the HQL statements using the Criteria API of NHibernate, but (and this is one the few gripes i have with this book) for some of the advanced concepts (namely projections and grouping) they completely ignore the Criteria API and focus solely on HQL.  I'm a pretty big fan of the Criteria API so i obviously find that a little bit disappointing.  It does cover pretty much everything else you want to know about querying with NHibernate though.  

The final 3 chapters deal with using NHibernate in the real world.  These chapters might be of use to people with little experience in (proper) software development, but i don't really think most people will really get a lot of value out of these last 3 chapters.  Topics covered here: layered architecture, common design goals you should aim to achieve, some options with regard to generating code/mappings/database, dealing with legacy database structures and more.  Again, this might be interesting to some, but definitely not to all.

Overall, i would say that everyone working with NHibernate would benefit greatly from reading this book.  If you're just getting started with NHibernate, it will indeed lower your learning curve significantly.  If you already have experience with NHibernate, you will most likely learn some tricks that you didn't know about yet.  It's a good book to read cover to cover, but at the same time it will be of great service to you as a reference book.  Chapter 3 through 7 especially are chapters you'll probably revisit frequently while you are working with NHibernate.

There is one thing to keep in mind though... this book targets version 1.2 of NHibernate, which is a bit of an old release.  This is rather unfortunate since NHibernate 2.0 and 2.1 introduced a lot of great new features.  Obviously, these great new features aren't covered here, but the most important things of working with NHibernate are all covered thoroughly so don't let the 1.2 version number make you think that this book isn't relevant to the current versions.  This book is by far the best source of information on how to get the most out of NHibernate. 

  






