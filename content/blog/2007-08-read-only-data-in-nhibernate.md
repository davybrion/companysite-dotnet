Was just browsing the NHibernate forum and read something i didn't know yet... if you have read-only data, you can declare the class as immutable:

<pre>
&lt;class name="Foo" table="foo_table" mutable="false"&gt;
</pre>

from the NHibernate documentation:

<blockquote>
Immutable classes, mutable="false", may not be updated or deleted by the application. This allows NHibernate to make some minor performance optimizations.
</blockquote>

Even if it's only a minor optimization, it's still a nice feature :)
