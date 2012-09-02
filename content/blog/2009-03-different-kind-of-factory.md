I was just refactoring some code and there were a few places in a class where we needed to instantiate a new instance of something, based on a key... could've used a factory class there, right? Instead i did this:

<code>
<div style="font-family: Consolas; font-size: 9pt; color: black; background: white;">
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span style="color: blue;">private</span> <span style="color: blue;">readonly</span> <span style="color: #2b91af;">Dictionary</span>&lt;<span style="color: blue;">string</span>, <span style="color: #2b91af;">Func</span>&lt;<span style="color: #2b91af;">IPanel</span>&gt;&gt; panelFactory = <span style="color: blue;">new</span> <span style="color: #2b91af;">Dictionary</span>&lt;<span style="color: blue;">string</span>, <span style="color: #2b91af;">Func</span>&lt;<span style="color: #2b91af;">IPanel</span>&gt;&gt;</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; {</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; { typeAndSizeKey, () =&gt; <span style="color: blue;">new</span> <span style="color: #2b91af;">TypeAndSizePanel</span>() },</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; { phaseKey, () =&gt; <span style="color: blue;">new</span> <span style="color: #2b91af;">PhasePanel</span>() },</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; { organisationStructureKey, () =&gt; <span style="color: blue;">new</span> <span style="color: #2b91af;">OrganisationStructurePanel</span>() },</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; { incidentDataKey, () =&gt; <span style="color: blue;">new</span> <span style="color: #2b91af;">IncidentDataPanel</span>() },</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; { processesKey, () =&gt; <span style="color: blue;">new</span> <span style="color: #2b91af;">ProcessesPanel</span>() },</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; { generalInfoKey, () =&gt; <span style="color: blue;">new</span> <span style="color: #2b91af;">GeneralInfoPanel</span>() },</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; { linksKey, () =&gt; <span style="color: blue;">new</span> <span style="color: #2b91af;">LinksPanel</span>() }</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; };</p>
</div>
</code>

and now, when i need to create a new instance i just do this:

<code>
<div style="font-family: Consolas; font-size: 9pt; color: black; background: white;">
<p style="margin: 0px;">panelFactory[myKey]()</p>
</div>
</code>

i kinda like the simplicity