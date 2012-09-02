There is a reported <a href="http://nhjira.koah.net/browse/NH-1079">performance issue</a> with NHibernate that i wanted to look into.  The reported issue was related to retrieving objects through a generically typed List or through an IList reference.  

The following code simulates the issue:

<code>
<style type="text/css">
.cf { font-family: Consolas; font-size: 9pt; color: black; background: white; }
.cl { margin: 0px; }
.cb1 { color: blue; }
.cb2 { color: #2b91af; }
.cb3 { color: #a31515; }
</style>
<div class="cf">
<p class="cl">&nbsp;&nbsp;&nbsp; <span class="cb1">class</span> <span class="cb2">MyClass</span> {}</p>
<p class="cl">&nbsp;</p>
<p class="cl">&nbsp;&nbsp;&nbsp; <span class="cb1">class</span> <span class="cb2">Program</span></p>
<p class="cl">&nbsp;&nbsp;&nbsp; {</p>
<p class="cl">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span class="cb1">static</span> <span class="cb1">void</span> Main(<span class="cb1">string</span>[] args)</p>
<p class="cl">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; {</p>
<p class="cl">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span class="cb2">List</span>&lt;<span class="cb2">MyClass</span>&gt; list = <span class="cb1">new</span> <span class="cb2">List</span>&lt;<span class="cb2">MyClass</span>&gt;();</p>
<p class="cl">&nbsp;</p>
<p class="cl">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span class="cb2">Stopwatch</span> stopwatch = <span class="cb2">Stopwatch</span>.StartNew();</p>
<p class="cl">&nbsp;</p>
<p class="cl">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span class="cb1">for</span> (<span class="cb1">int</span> i = 0; i &lt; 100000; i++)</p>
<p class="cl">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; {</p>
<p class="cl">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; list.Add(<span class="cb1">new</span> <span class="cb2">MyClass</span>());</p>
<p class="cl">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; }</p>
<p class="cl">&nbsp;</p>
<p class="cl">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; stopwatch.Stop();</p>
<p class="cl">&nbsp;</p>
<p class="cl">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span class="cb2">Console</span>.WriteLine(<span class="cb3">&quot;Elapsed ms: &quot;</span> + stopwatch.ElapsedMilliseconds);</p>
<p class="cl">&nbsp;</p>
<p class="cl">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span class="cb2">IList</span> iList = <span class="cb1">new</span> <span class="cb2">List</span>&lt;<span class="cb2">MyClass</span>&gt;();</p>
<p class="cl">&nbsp;</p>
<p class="cl">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; stopwatch = <span class="cb2">Stopwatch</span>.StartNew();</p>
<p class="cl">&nbsp;</p>
<p class="cl">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span class="cb1">for</span> (<span class="cb1">int</span> i = 0; i &lt; 100000; i++)</p>
<p class="cl">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; {</p>
<p class="cl">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; iList.Add(<span class="cb1">new</span> <span class="cb2">MyClass</span>());</p>
<p class="cl">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; }</p>
<p class="cl">&nbsp;</p>
<p class="cl">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; stopwatch.Stop();</p>
<p class="cl">&nbsp;</p>
<p class="cl">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span class="cb2">Console</span>.WriteLine(<span class="cb3">&quot;Elapsed ms: &quot;</span> + stopwatch.ElapsedMilliseconds);</p>
<p class="cl">&nbsp;</p>
<p class="cl">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span class="cb2">Console</span>.ReadLine();</p>
<p class="cl">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; }</p>
<p class="cl">&nbsp;&nbsp;&nbsp; }</p>
</div>
</code>

The only difference between both Add operations is that in the first case, the typed Add method of the generically typed List reference is called.  In the second case, the untyped Add method of the generically typed List is called through the IList reference.

On my slow Macbook, the first Add operation typically took between 10 and 20 ms.  The second Add operation typically took almost twice as long as the first Add operation.  As you can see, that is a very minor performance issue, and it actually is only consistently noticeable once you're dealing with 100000 elements.  At 50000 elements, both operations typically take the same amount of time with only minor variations in performance on certain runs.

So yes, once you're dealing with a large enough set of elements, there is indeed a performance difference.  But it's extremely minor and the extra cost of the Add operation is most definitely <strong>the least of your concerns if you're retrieving that many entity instances through an ORM.</strong>  The extra amount of memory that needs to be used for those entities and the extra cost of pulling all of that data over the wire is what's really going to bite you, not the extra cost of the Add operation ;)