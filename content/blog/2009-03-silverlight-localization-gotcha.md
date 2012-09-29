Wasted a bit of time figuring this out, so I'm posting it here for future reference.  We have this multi-language silverlight app where the UI needs to be shown in either English or Dutch.  So you know, I copied my TextResources.resx file (which contains the translated strings in English) and created a TextResources.nl.resx file where I replaced the English strings with the Dutch strings.

In the output folder of the Silverlight project, you can clearly see that the Our.KickAss.Silverlight.Project.resources.dll file is located in the 'nl' folder.  The XAP file that is being copied to the actual web project however, doesn't contain the dutch resources.

Apparantly, the key to 'fixing' this is opening the .csproj file of your Silverlight project, and then you need to modify the SupportedCultures element so it contains the cultures you're supporting:

<code>
<div style="font-family: Consolas; font-size: 9pt; color: black;">
<p style="margin: 0px;"><span style="color: blue;">&lt;</span><span style="color: #a31515;">SupportedCultures</span><span style="color: blue;">&gt;</span>en;nl<span style="color: blue;">&lt;/</span><span style="color: #a31515;">SupportedCultures</span><span style="color: blue;">&gt;</span></p>
</div>
</code>

There might be a way to do this from the UI within Visual Studio, but I sure didn't find it anywhere. 

And now you can simply switch between the translations setting the generated resource class's static Culture property to the culture you want, for instance:

<code>
<div style="font-family: Consolas; font-size: 9pt; color: black;">
<p style="margin: 0px;"><span style="color: #2b91af;">TextResources</span>.Culture = <span style="color: blue;">new</span> <span style="color: #2b91af;">CultureInfo</span>(<span style="color: #a31515;">&quot;nl-BE&quot;</span>);</p>
</div>
</code>