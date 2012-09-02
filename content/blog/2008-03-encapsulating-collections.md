When i have classes that need to expose collections, i always try to make sure that consumers only get a read-only version of that collection.  Consumers should be able to easily use those collections, but if something would need to be added or removed from a collection, it should happen with a specific call to the owner of that collection instead of manipulating the collection directly.  In .NET 2.0 i often used the ReadOnlyCollection class for this, but now with .NET 3.5 there is a much easier way.  By simply exposing the collections as IEnumerable you prevent consumers from directly adding or removing items from your collection, but with the LINQ extension methods, they are still very usable to anyone that need to use them.

Small example:

<div style="font-family:Courier New;font-size:10pt;color:black;background:white;">
<p style="margin:0;">&nbsp;&nbsp;&nbsp; <span style="color:blue;">public</span> <span style="color:blue;">class</span> <span style="color:#2b91af;">Member</span></p>
<p style="margin:0;">&nbsp;&nbsp;&nbsp; {</p>
<p style="margin:0;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span style="color:blue;">private</span> <span style="color:blue;">readonly</span> <span style="color:#2b91af;">List</span>&lt;<span style="color:#2b91af;">Member</span>&gt; _parents;</p>
<p style="margin:0;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span style="color:blue;">private</span> <span style="color:blue;">readonly</span> <span style="color:#2b91af;">List</span>&lt;<span style="color:#2b91af;">Member</span>&gt; _children;</p>
<p style="margin:0;">&nbsp;</p>
<p style="margin:0;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span style="color:blue;">public</span> Member()</p>
<p style="margin:0;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; {</p>
<p style="margin:0;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; _parents = <span style="color:blue;">new</span> <span style="color:#2b91af;">List</span>&lt;<span style="color:#2b91af;">Member</span>&gt;();</p>
<p style="margin:0;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; _children = <span style="color:blue;">new</span> <span style="color:#2b91af;">List</span>&lt;<span style="color:#2b91af;">Member</span>&gt;();</p>
<p style="margin:0;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; }</p>
<p style="margin:0;">&nbsp;</p>
<p style="margin:0;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span style="color:blue;">public</span> <span style="color:#2b91af;">IEnumerable</span>&lt;<span style="color:#2b91af;">Member</span>&gt; Children</p>
<p style="margin:0;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; {</p>
<p style="margin:0;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span style="color:blue;">get</span> { <span style="color:blue;">return</span> _children; }</p>
<p style="margin:0;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; }</p>
<p style="margin:0;">&nbsp;</p>
<p style="margin:0;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span style="color:blue;">public</span> <span style="color:#2b91af;">IEnumerable</span>&lt;<span style="color:#2b91af;">Member</span>&gt; Parents</p>
<p style="margin:0;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; {</p>
<p style="margin:0;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span style="color:blue;">get</span> { <span style="color:blue;">return</span> _parents; }</p>
<p style="margin:0;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; }</p>
<p style="margin:0;">&nbsp;</p>
<p style="margin:0;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span style="color:blue;">public</span> <span style="color:blue;">void</span> AddChild(<span style="color:#2b91af;">Member</span> child)</p>
<p style="margin:0;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; {</p>
<p style="margin:0;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; _children.Add(child);</p>
<p style="margin:0;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; }</p>
<p style="margin:0;">&nbsp;</p>
<p style="margin:0;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span style="color:blue;">public</span> <span style="color:blue;">void</span> RemoveChild(<span style="color:#2b91af;">Member</span> child)</p>
<p style="margin:0;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; {</p>
<p style="margin:0;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; _children.Remove(child);</p>
<p style="margin:0;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; }</p>
<p style="margin:0;">&nbsp;</p>
<p style="margin:0;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span style="color:blue;">public</span> <span style="color:blue;">void</span> AddParent(<span style="color:#2b91af;">Member</span> parent)</p>
<p style="margin:0;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; {</p>
<p style="margin:0;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; _parents.Add(parent);</p>
<p style="margin:0;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; }</p>
<p style="margin:0;">&nbsp;</p>
<p style="margin:0;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span style="color:blue;">public</span> <span style="color:blue;">void</span> RemoveParent(<span style="color:#2b91af;">Member</span> parent)</p>
<p style="margin:0;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; {</p>
<p style="margin:0;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; _parents.Remove(parent);</p>
<p style="margin:0;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; }</p>
<p style="margin:0;">&nbsp;&nbsp;&nbsp; }</p>
</div>

the Parents and Children collections are completely encapsulated, yet still highly usable to consumers:

<a href='http://davybrion.com/blog/wp-content/uploads/2008/03/usableencapsulatedcollection.png' title='usableencapsulatedcollection.png'><img src='http://davybrion.com/blog/wp-content/uploads/2008/03/usableencapsulatedcollection.png' alt='usableencapsulatedcollection.png' /></a>
