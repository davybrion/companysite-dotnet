There is a reported <a href="http://nhjira.koah.net/browse/NH-1079">performance issue</a> with NHibernate that I wanted to look into.  The reported issue was related to retrieving objects through a generically typed List or through an IList reference.  

The following code simulates the issue:

<script src="https://gist.github.com/3684498.js?file=s1.cs"></script>

The only difference between both Add operations is that in the first case, the typed Add method of the generically typed List reference is called.  In the second case, the untyped Add method of the generically typed List is called through the IList reference.

On my slow Macbook, the first Add operation typically took between 10 and 20 ms.  The second Add operation typically took almost twice as long as the first Add operation.  As you can see, that is a very minor performance issue, and it actually is only consistently noticeable once you're dealing with 100000 elements.  At 50000 elements, both operations typically take the same amount of time with only minor variations in performance on certain runs.

So yes, once you're dealing with a large enough set of elements, there is indeed a performance difference.  But it's extremely minor and the extra cost of the Add operation is most definitely <strong>the least of your concerns if you're retrieving that many entity instances through an ORM.</strong>  The extra amount of memory that needs to be used for those entities and the extra cost of pulling all of that data over the wire is what's really going to bite you, not the extra cost of the Add operation ;)