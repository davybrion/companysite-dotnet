Was just browsing the NHibernate forum and read something I didn't know yet... if you have read-only data, you can declare the class as immutable:

<script src="https://gist.github.com/3611495.js?file=s1.xml"></script>

from the NHibernate documentation:

> Immutable classes, mutable="false", may not be updated or deleted by the application. This allows NHibernate to make some minor performance optimizations.

Even if it's only a minor optimization, it's still a nice feature :)
