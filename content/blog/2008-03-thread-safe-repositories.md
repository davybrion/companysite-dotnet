First of all, this is not about true DDD repositories... i just needed an in-memory data container with basic storing/retrieving functionality so i figured i'd call it a Repository.

These repositories will be accessed by multiple concurrent threads so i needed to make sure that all access to the underlying dictionary is properly synchronized. But, i also want them to be highly usable and i especially wanted a thread-safe way of dynamically executing queries on them.

Here's what i came up with:

<div style="font-family:Courier New;font-size:10pt;color:black;background:white;">
<p style="margin:0;">&nbsp;&nbsp;&nbsp; <span style="color:blue;">public</span> <span style="color:blue;">class</span> <span style="color:#2b91af;">Repository</span>&lt;T&gt; <span style="color:blue;">where</span> T : <span style="color:#2b91af;">Member</span></p>
<p style="margin:0;">&nbsp;&nbsp;&nbsp; {</p>
<p style="margin:0;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span style="color:blue;">private</span> <span style="color:blue;">readonly</span> <span style="color:blue;">object</span> _monitor = <span style="color:blue;">new</span> <span style="color:blue;">object</span>();</p>
<p style="margin:0;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span style="color:blue;">private</span> <span style="color:blue;">readonly</span> <span style="color:#2b91af;">Dictionary</span>&lt;<span style="color:#2b91af;">Id</span>, T&gt; _members = <span style="color:blue;">new</span> <span style="color:#2b91af;">Dictionary</span>&lt;<span style="color:#2b91af;">Id</span>,T&gt;();</p>
<p style="margin:0;">&nbsp;</p>
<p style="margin:0;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span style="color:blue;">protected</span> <span style="color:blue;">object</span> Monitor</p>
<p style="margin:0;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; {</p>
<p style="margin:0;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span style="color:blue;">get</span> { <span style="color:blue;">return</span> _monitor; }</p>
<p style="margin:0;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; }</p>
<p style="margin:0;">&nbsp;</p>
<p style="margin:0;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span style="color:gray;">///</span><span style="color:green;"> </span><span style="color:gray;">&lt;summary&gt;</span></p>
<p style="margin:0;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span style="color:gray;">///</span><span style="color:green;"> any derived classes that use this property are responsible for their own locking!</span></p>
<p style="margin:0;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span style="color:gray;">///</span><span style="color:green;"> (use the Monitor property for that)</span></p>
<p style="margin:0;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span style="color:gray;">///</span><span style="color:green;"> </span><span style="color:gray;">&lt;/summary&gt;</span></p>
<p style="margin:0;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span style="color:blue;">protected</span> <span style="color:#2b91af;">Dictionary</span>&lt;<span style="color:#2b91af;">Id</span>, T&gt; Members</p>
<p style="margin:0;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; {</p>
<p style="margin:0;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span style="color:blue;">get</span> { <span style="color:blue;">return</span> _members; }</p>
<p style="margin:0;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; }</p>
<p style="margin:0;">&nbsp;</p>
<p style="margin:0;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span style="color:blue;">public</span> <span style="color:blue;">virtual</span> <span style="color:blue;">void</span> Store(T member)</p>
<p style="margin:0;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; {</p>
<p style="margin:0;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span style="color:blue;">lock</span> (Monitor)</p>
<p style="margin:0;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; {</p>
<p style="margin:0;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; Put(member);</p>
<p style="margin:0;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; }</p>
<p style="margin:0;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; }</p>
<p style="margin:0;">&nbsp;</p>
<p style="margin:0;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span style="color:blue;">public</span> <span style="color:blue;">virtual</span> <span style="color:blue;">void</span> StoreRange(<span style="color:#2b91af;">IEnumerable</span>&lt;T&gt; members)</p>
<p style="margin:0;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; {</p>
<p style="margin:0;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span style="color:blue;">lock</span> (Monitor)</p>
<p style="margin:0;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; {</p>
<p style="margin:0;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span style="color:blue;">foreach</span> (T member <span style="color:blue;">in</span> members)</p>
<p style="margin:0;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; {</p>
<p style="margin:0;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; Put(member);</p>
<p style="margin:0;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; }</p>
<p style="margin:0;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; }</p>
<p style="margin:0;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; }</p>
<p style="margin:0;">&nbsp;</p>
<p style="margin:0;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span style="color:blue;">public</span> <span style="color:blue;">virtual</span> <span style="color:blue;">void</span> Remove(T member)</p>
<p style="margin:0;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; {</p>
<p style="margin:0;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span style="color:blue;">lock</span> (Monitor)</p>
<p style="margin:0;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; {</p>
<p style="margin:0;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span style="color:blue;">if</span> (Members.ContainsValue(member))</p>
<p style="margin:0;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; {</p>
<p style="margin:0;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; Members.Remove(member.Id);</p>
<p style="margin:0;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; }</p>
<p style="margin:0;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; }</p>
<p style="margin:0;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; }</p>
<p style="margin:0;">&nbsp;</p>
<p style="margin:0;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span style="color:blue;">public</span> <span style="color:blue;">virtual</span> T Get(<span style="color:#2b91af;">Id</span> id)</p>
<p style="margin:0;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; {</p>
<p style="margin:0;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span style="color:blue;">lock</span> (Monitor)</p>
<p style="margin:0;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; {</p>
<p style="margin:0;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span style="color:blue;">if</span> (!Members.ContainsKey(id))</p>
<p style="margin:0;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; {</p>
<p style="margin:0;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span style="color:blue;">return</span> <span style="color:blue;">null</span>;</p>
<p style="margin:0;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; }</p>
<p style="margin:0;">&nbsp;</p>
<p style="margin:0;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span style="color:blue;">return</span> Members[id];</p>
<p style="margin:0;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; }</p>
<p style="margin:0;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; }</p>
<p style="margin:0;">&nbsp;</p>
<p style="margin:0;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span style="color:blue;">public</span> <span style="color:blue;">virtual</span> T FindFirst(<span style="color:#2b91af;">Func</span>&lt;T, <span style="color:blue;">bool</span>&gt; expression)</p>
<p style="margin:0;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; {</p>
<p style="margin:0;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span style="color:blue;">lock</span> (Monitor)</p>
<p style="margin:0;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; {</p>
<p style="margin:0;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span style="color:blue;">return</span> Members.Values.FirstOrDefault(expression);</p>
<p style="margin:0;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; }</p>
<p style="margin:0;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; }</p>
<p style="margin:0;">&nbsp;</p>
<p style="margin:0;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span style="color:blue;">public</span> <span style="color:blue;">virtual</span> <span style="color:#2b91af;">IEnumerable</span>&lt;T&gt; FindAll(<span style="color:#2b91af;">Func</span>&lt;T, <span style="color:blue;">bool</span>&gt; expression)</p>
<p style="margin:0;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; {</p>
<p style="margin:0;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span style="color:blue;">lock</span> (Monitor)</p>
<p style="margin:0;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; {</p>
<p style="margin:0;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span style="color:blue;">return</span> Members.Values.Where(expression).ExecuteImmediately();</p>
<p style="margin:0;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; }</p>
<p style="margin:0;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; }</p>
<p style="margin:0;">&nbsp;</p>
<p style="margin:0;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span style="color:gray;">///</span><span style="color:green;"> </span><span style="color:gray;">&lt;summary&gt;</span></p>
<p style="margin:0;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span style="color:gray;">///</span><span style="color:green;"> this method should ONLY be called when a lock on Monitor has been acquired!</span></p>
<p style="margin:0;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span style="color:gray;">///</span><span style="color:green;"> </span><span style="color:gray;">&lt;/summary&gt;</span></p>
<p style="margin:0;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span style="color:gray;">///</span><span style="color:green;"> </span><span style="color:gray;">&lt;param name="member"&gt;&lt;/param&gt;</span></p>
<p style="margin:0;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span style="color:blue;">private</span> <span style="color:blue;">void</span> Put(T member)</p>
<p style="margin:0;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; {</p>
<p style="margin:0;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span style="color:green;">// this overwrites existing entries with the same ID... it's not a mistake ;)</span></p>
<p style="margin:0;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; Members[member.Id] = member;</p>
<p style="margin:0;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; }</p>
<p style="margin:0;">&nbsp;&nbsp;&nbsp; }</p>
</div>

each access to the dictionary is properly synchronized (at least, i think so... if anyone knows of a better way to do the locking, please leave a comment) and i can still execute dynamic queries on them by passing lambda expressions to the FindFirst and FindAll methods, like this:

<div style="font-family:Courier New;font-size:10pt;color:black;background:white;">
<p style="margin:0;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span style="color:#2b91af;">User</span> me = userReposority.FindFirst(u =&gt; u.FirstName == <span style="color:#a31515;">"Davy"</span> &amp;&amp; u.LastName == <span style="color:#a31515;">"Brion"</span>);</p>
<p style="margin:0;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span style="color:#2b91af;">IEnumerable</span>&lt;<span style="color:#2b91af;">User</span>&gt; usersWithNoLastName = </p>
<p style="margin:0;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; userReposority.FindAll(u =&gt; <span style="color:blue;">string</span>.IsNullOrEmpty(u.LastName));</p>
</div>

<br />
