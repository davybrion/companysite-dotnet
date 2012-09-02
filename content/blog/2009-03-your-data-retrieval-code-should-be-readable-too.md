Keeping our code readable and easy to understand is a goal that many of us try to reach as much as possible.  Ideally, any developer should be able to look at a piece of code and be able to grasp <strong>exactly</strong> what the code does in a <strong>short amount of time</strong>.   Data retrieval code is the kind of code that often fails to achieve that goal, at least IMO.  A lot of applications will have at least a couple or more non-trivial queries that need to be executed.  What do i mean by non-trivial queries? Queries with joins spanning several relationships, queries that need to take certain 'non-optimal' data structure specialties into account, pretty much any query that makes you read it twice (or more) before you understand what it does.

You can hide the complexity of those queries in views or stored procedures but by doing that, you're moving some very important details outside of your code.  You're introducing one more little hurdle the next developer will have to get over when trying to comprehend a certain piece of code which has to use that view or that stored procedure. 

Ideally, you want something in your code which clearly states the intention of a non-trivial query so that it's easy to understand immediately, or at least without having to read it multiple times.  Personally, i think that straight SQL is a poor choice to achieve this.  HQL (Hibernate Query Language) is a bit better, but even that isn't always as readable as it should be.  NHibernate's Criteria API can offer you pretty good readability however.  Of course, it does require some familiarity with the Criteria API before those queries are easy to comprehend, but i'd argue that SQL, HQL and LINQ suffer from the same problem.

I'm going to a use a query from a real application as an example.  This application unfortunately uses a legacy database which we can't really change.  Well we can change some parts of it, but not the whole thing.  Anyways, there are some parts of the database model which are somewhat complex from a table structure point of view, but from a domain point of view, it's not that hard at all if you map everything properly.   Do i want to be faced with the database complexity every time we need to use data from these areas of the data model in our code? Hell no. Do i want to know exactly what is going on from a functional point of view? Hell yes.

Alright, let's just get to the specific query:

<code>
<style type="text/css">
.cf { font-family: Consolas; font-size: 9pt; color: black; background: white; }
.cl { margin: 0px; }
.cb1 { color: blue; }
.cb2 { color: #2b91af; }
.cb3 { color: #a31515; }
</style>
<div class="cf">
<p class="cl">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span class="cb1">var</span> teamIdsForUser = <span class="cb2">DetachedCriteria</span>.For&lt;<span class="cb2">User</span>&gt;()</p>
<p class="cl">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; .Add(<span class="cb2">Restrictions</span>.Eq(<span class="cb3">&quot;Id&quot;</span>, userId))</p>
<p class="cl">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; .CreateCriteria(<span class="cb3">&quot;Teams&quot;</span>, <span class="cb3">&quot;team&quot;</span>)</p>
<p class="cl">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; .SetProjection(<span class="cb2">Projections</span>.Property(<span class="cb3">&quot;team.Id&quot;</span>));</p>
<p class="cl">&nbsp;</p>
<p class="cl">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span class="cb1">var</span> incidents = Session.CreateCriteria(<span class="cb1">typeof</span>(<span class="cb2">Incident</span>), <span class="cb3">&quot;incident&quot;</span>)</p>
<p class="cl">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; .CreateCriteria(<span class="cb3">&quot;Configurations&quot;</span>, <span class="cb3">&quot;configuration&quot;</span>, <span class="cb2">JoinType</span>.InnerJoin)</p>
<p class="cl">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; .CreateCriteria(<span class="cb3">&quot;configuration.Structure&quot;</span>, <span class="cb3">&quot;structure&quot;</span>, <span class="cb2">JoinType</span>.InnerJoin)</p>
<p class="cl">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; .Add(<span class="cb2">Subqueries</span>.PropertyIn(<span class="cb3">&quot;configuration.OrganisationalItem.Id&quot;</span>, teamIdsForUser))</p>
<p class="cl">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; .Add(<span class="cb2">Restrictions</span>.EqProperty(<span class="cb3">&quot;incident.ApprovedPhase&quot;</span>, <span class="cb3">&quot;structure.Phase&quot;</span>))</p>
<p class="cl">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; .Add(<span class="cb2">Restrictions</span>.IsNull(<span class="cb3">&quot;incident.EndDate&quot;</span>))</p>
<p class="cl">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; .SetResultTransformer(<span class="cb2">CriteriaSpecification</span>.DistinctRootEntity)</p>
<p class="cl">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; .List&lt;<span class="cb2">Incident</span>&gt;();</p>
</div>
</code>

If you have experience with NHibernate, this query will probably make sense to you rather quickly.  If you don't have NHibernate experience, this query will probably be as clear to you as a complex SQL query is clear to the average developer who claims to know SQL (not every developer who claims to know SQL actually knows more than the very basics of it).

I'm not going to explain the query, or what exactly makes it special, just yet.  Read it again, try to figure out what it does, and try to see if there's really anything special there.  I'll show the actual SQL below, but if you want to play along, try to figure it out first.



Ready?



Ok, here's the actual SQL that is generated (by NHibernate, so the readability of this query could definitely be improved):

<code>
<style type="text/css">
.cf { font-family: Consolas; font-size: 9pt; color: black; background: white; }
.cl { margin: 0px; }
.cb1 { color: blue; }
.cb2 { color: green; }
</style>
<div class="cf">
<p class="cl"><span class="cb1">SELECT </span>this_.ID&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp;&nbsp; <span class="cb1">as </span>ID21_2_,</p>
<p class="cl">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp; this_.StartedByUserID&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span class="cb1">as </span>StartedB2_21_2_,</p>
<p class="cl">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp; this_.EndedByUserID&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp; <span class="cb1">as </span>EndedByN3_21_2_,</p>
<p class="cl">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp; this_.Name&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp; <span class="cb1">as </span>Name21_2_,</p>
<p class="cl">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp; this_.Description&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span class="cb1">as </span>Descript5_21_2_,</p>
<p class="cl">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp; this_.StartDate&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp; <span class="cb1">as </span>StartDate21_2_,</p>
<p class="cl">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp; this_.EndDate&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span class="cb1">as </span>EndDate21_2_,</p>
<p class="cl">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp; this_.ScenarioID&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp;&nbsp; <span class="cb1">as </span>ScenarioID21_2_,</p>
<p class="cl">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp; this_.ApprovedPhaseID&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span class="cb1">as </span>Approved9_21_2_,</p>
<p class="cl">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp; this_.ApprovedPhaseDate&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp; <span class="cb1">as </span>Approve10_21_2_,</p>
<p class="cl">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp; this_.ApprovedPhaseUserID&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span class="cb1">as </span>Approve11_21_2_,</p>
<p class="cl">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp; this_.SuggestedPhaseID&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp; <span class="cb1">as </span>Suggest12_21_2_,</p>
<p class="cl">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp; this_.SuggestedPhaseDate&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp;&nbsp; <span class="cb1">as </span>Suggest13_21_2_,</p>
<p class="cl">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp; this_.SuggestedPhaseUserID&nbsp;&nbsp;&nbsp; &nbsp;&nbsp; <span class="cb1">as </span>Suggest14_21_2_,</p>
<p class="cl">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp; configurat1_.ID&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp; <span class="cb1">as </span>ID15_0_,</p>
<p class="cl">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp; configurat1_.OrgItemid&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp; <span class="cb1">as </span>OrgItemid15_0_,</p>
<p class="cl">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp; configurat1_.OrgChartID&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp; <span class="cb1">as </span>OrgChartID15_0_,</p>
<p class="cl">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp; configurat1_.IncidentID&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp; <span class="cb1">as </span>IncidentID15_0_,</p>
<p class="cl">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp; structure2_.orgchartid&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp; <span class="cb1">as </span>orgchartid33_1_,</p>
<p class="cl">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp; structure2_.isadviser&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span class="cb1">as </span>isadviser33_1_,</p>
<p class="cl">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp; structure2_.roleid&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp; <span class="cb1">as </span>roleid33_1_,</p>
<p class="cl">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp; structure2_.parentroleid&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp;&nbsp; <span class="cb1">as </span>parentro4_33_1_,</p>
<p class="cl">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp; structure2_.phaseid&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp; <span class="cb1">as </span>phaseid33_1_,</p>
<p class="cl">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp; structure2_.descriptionentryid&nbsp;&nbsp; <span class="cb1">as </span>descript6_33_1_</p>
<p class="cl"><span class="cb1">FROM </span>&nbsp; console_incident this_</p>
<p class="cl">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp; <span class="cb1">inner join </span>console_incident_organisation configurat1_</p>
<p class="cl">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp;&nbsp; <span class="cb1">on </span>this_.ID = configurat1_.IncidentID</p>
<p class="cl">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp; <span class="cb1">inner join </span>org_chart structure2_</p>
<p class="cl">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp;&nbsp; <span class="cb1">on </span>configurat1_.OrgChartID = structure2_.orgchartid</p>
<p class="cl"><span class="cb1">WHERE </span> configurat1_.OrgItemid <span class="cb1">in </span>(<span class="cb1">SELECT </span>team1_.orgitemid <span class="cb1">as </span>y0_</p>
<p class="cl">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp; <span class="cb1">FROM </span>&nbsp; application_user this_0_</p>
<p class="cl">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp;&nbsp; <span class="cb1">inner join </span>org_item this_0_1_</p>
<p class="cl">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp; <span class="cb1">on </span>this_0_.userid = this_0_1_.orgitemid</p>
<p class="cl">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp;&nbsp; <span class="cb1">inner join </span>org_item_to_org_item teams3_</p>
<p class="cl">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp; <span class="cb1">on </span>this_0_.userid = teams3_.childid</p>
<p class="cl">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp;&nbsp; <span class="cb1">inner join </span>team team1_</p>
<p class="cl">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp; <span class="cb1">on </span>teams3_.parentid = team1_.orgitemid</p>
<p class="cl">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp;&nbsp; <span class="cb1">left outer join </span>org_item team1_1_</p>
<p class="cl">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp; <span class="cb1">on </span>team1_.orgitemid = team1_1_.orgitemid</p>
<p class="cl">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp; <span class="cb1">WHERE </span> this_0_.userid = 55397 <span class="cb2">/* ?p0 */</span>)</p>
<p class="cl">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp; <span class="cb1">and </span>this_.EndDate <span class="cb1">is null</span></p>
<p class="cl">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp; <span class="cb1">and </span>this_.ApprovedPhaseID = structure2_.phaseid</p>
</div>
</code>

First of all, if anyone were to write this SQL query manually, you'd hope the author would use much clearer aliases.  That would obviously improve the readability of this specific SQL query, but there is another issue here.  Take a close look at the SQL statement in the subselect, and compare it to the DetachedCriteria which is assigned to the teamIdsForUser variable in the C# code listed earlier.

The DetachedCriteria looks really simple, doesn't it? So why is the actual SQL for that part more complex than expected?  Well, in this particular database there is an inheritance relationship which complicates things.  A 'user' inherits from an 'org_item', and a 'team' inherits from an 'org_item' as well.  This inheritance relationship was implemented using the Table Per Class strategy.   But wait, there's another twist.  There is a many-to-many relationship between records in the org_item table.  Each user can be linked to multiple teams, and each team can be linked to multiple users.  But since the many-to-many relationship uses the org_item table (the common base class really), many-to-many associations can be made with teams on both sides of the relationship, or users on both sides of the relationship.  Confused already? I can imagine.  Surely, this complexity is not something you want exposed in your actual code, right?  Keep in mind that this complexity would need to be dealt with practically every time you deal with users and teams in code.  Both concepts are pretty important in the domain of this application so you can imagine how often they are used :)

In our case, this complexity is handled <strong>once</strong> in our NHibernate mapping files.  We can keep it out of our queries while we let NHibernate handle the complexity for us.  At the same time, our code still clearly communicates the intent of each query.  This, IMO, is one of the huge benefits of using a powerful ORM.

Thoughts?