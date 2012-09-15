I have an older post where i discuss <a href="/blog/2009/03/implementing-a-value-object-with-nhibernate">how you can implement a Value Object with NHibernate</a>.  In that post i mentioned the following:

<blockquote>NHibernate allows a private default constructor for Value Objects, but for Entities you will need a default public or protected constructor as private is not sufficient.</blockquote>

I got the following comment from someone:

<blockquote>
I too am trying to determine how well NHibernate lives up to the promise of persistence ignorance. I can definitely live with unnecessary private constructors, but I’m dubious about adding protected constructors just to support an ORM.

At any rate, I was surprised by the sentence I quoted, because I didn’t realize there were any circumstances in which NHibernate required protected default constructors.
</blockquote>

Once again, the answer is related to the dynamic proxies that NHibernate uses.  Value Objects will never be proxied by NHibernate, so NHibernate only needs a private default constructor to create the instances.  If an entity is eligible for lazy loading however, then NHibernate will create a type which inherits from your entity (this is described in depth <a href="/blog/2009/03/must-everything-be-virtual-with-nhibernate/">here</a> and <a href="/blog/2009/09/must-everything-be-virtual-with-nhibernate-part-iii/">here</a>).  Which means that we really need either a public or protected constructor in entity classes that are eligible for lazy loading.  Consider the following class:

<script src="https://gist.github.com/3685257.js?file=s1.cs"></script>

If we try to create the following derived class:

<script src="https://gist.github.com/3685257.js?file=s2.cs"></script>

We get the following compiler error:

Error	1	'ConsoleApplication1.SomeEntity.SomeEntity()' is inaccessible due to its protection level	

It's a silly example, but it does show why entity types need at least a public or a protected default constructor and why a private one isn't sufficient.