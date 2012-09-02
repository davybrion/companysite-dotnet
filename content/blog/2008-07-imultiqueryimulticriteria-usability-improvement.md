A while ago i posted my <a href="http://davybrion.com/blog/2008/06/the-query-batcher/">query batcher</a> which allowed you to use IMultiCriteria/IMultiQuery with key values instead of relying on the index position of the results.

I was asked to add those capabilities to NHibernate, so i submitted <a href="http://jira.nhibernate.org/browse/NH-1354">a patch</a> for IMultiCriteria first, which was applied <a href="http://nhibernate.svn.sourceforge.net/viewvc/nhibernate?view=rev&revision=3620">today</a>.  So i also submitted the patch for <a href="http://jira.nhibernate.org/browse/NH-1381">IMultiQuery</a>.

So now you can basically do this:

<code>
<div style="font-family: Consolas; font-size: 10pt; color: black; background: white;">
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span style="color: blue;">using</span> (<span style="color: #2b91af;">ISession</span> session = OpenSession())</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; {</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span style="color: #2b91af;">IMultiCriteria</span> multiCriteria = session.CreateMultiCriteria();</p>
<p style="margin: 0px;">&nbsp;</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span style="color: #2b91af;">DetachedCriteria</span> firstCriteria = <span style="color: #2b91af;">DetachedCriteria</span>.For(<span style="color: blue;">typeof</span>(<span style="color: #2b91af;">Item</span>))</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; .Add(<span style="color: #2b91af;">Expression</span>.Lt(<span style="color: #a31515;">"id"</span>, 50));</p>
<p style="margin: 0px;">&nbsp;</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span style="color: #2b91af;">DetachedCriteria</span> secondCriteria = <span style="color: #2b91af;">DetachedCriteria</span>.For(<span style="color: blue;">typeof</span>(<span style="color: #2b91af;">Item</span>));</p>
<p style="margin: 0px;">&nbsp;</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; multiCriteria.Add(<span style="color: #a31515;">"firstCriteria"</span>, firstCriteria);</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; multiCriteria.Add(<span style="color: #a31515;">"secondCriteria"</span>, secondCriteria);</p>
<p style="margin: 0px;">&nbsp;</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span style="color: #2b91af;">IList</span> secondResult = (<span style="color: #2b91af;">IList</span>)multiCriteria.GetResult(<span style="color: #a31515;">"secondCriteria"</span>);</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span style="color: #2b91af;">IList</span> firstResult = (<span style="color: #2b91af;">IList</span>)multiCriteria.GetResult(<span style="color: #a31515;">"firstCriteria"</span>);</p>
<p style="margin: 0px;">&nbsp;</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span style="color: #2b91af;">Assert</span>.Greater(secondResult.Count, firstResult.Count);</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; }</p>
</div>
</code>

Same thing for IMultiQuery, but with hql queries instead of criteria obviously.

This has been added to the trunk, so i'm not sure if this will be ported to the NH2.0 branch, but at least it should be available for NH2.1 :)

update: the second patch has also been <a href="http://nhibernate.svn.sourceforge.net/viewvc/nhibernate?view=rev&revision=3621">applied</a>