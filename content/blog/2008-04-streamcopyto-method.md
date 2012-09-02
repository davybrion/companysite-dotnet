No, Stream does not have a useful CopyTo method :(

unless.... :

<div style="font-family:Courier New;font-size:10pt;color:black;background:white;">
<p style="margin:0;">&nbsp;&nbsp;&nbsp; <span style="color:blue;">public</span> <span style="color:blue;">static</span> <span style="color:blue;">class</span> <span style="color:#2b91af;">Extensions</span></p>
<p style="margin:0;">&nbsp;&nbsp;&nbsp; {</p>
<p style="margin:0;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span style="color:blue;">public</span> <span style="color:blue;">static</span> <span style="color:blue;">void</span> CopyTo(<span style="color:blue;">this</span> <span style="color:#2b91af;">Stream</span> source, <span style="color:#2b91af;">Stream</span> target)</p>
<p style="margin:0;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; {</p>
<p style="margin:0;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span style="color:blue;">if</span> (!source.CanRead) <span style="color:blue;">throw</span> <span style="color:blue;">new</span> <span style="color:#2b91af;">ArgumentException</span>(<span style="color:#a31515;">"source can not be read"</span>, <span style="color:#a31515;">"source"</span>);</p>
<p style="margin:0;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span style="color:blue;">if</span> (!target.CanWrite) <span style="color:blue;">throw</span> <span style="color:blue;">new</span> <span style="color:#2b91af;">ArgumentException</span>(<span style="color:#a31515;">"target can not be written to"</span>, <span style="color:#a31515;">"target"</span>);</p>
<p style="margin:0;">&nbsp;</p>
<p style="margin:0;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; source.Position = 0;</p>
<p style="margin:0;">&nbsp;</p>
<p style="margin:0;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span style="color:blue;">while</span> (source.Position &lt; source.Length)</p>
<p style="margin:0;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; {</p>
<p style="margin:0;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span style="color:blue;">var</span> buffer = <span style="color:blue;">new</span> <span style="color:blue;">byte</span>[4096];</p>
<p style="margin:0;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span style="color:blue;">int</span> bytesRead = source.Read(buffer, 0, buffer.Length);</p>
<p style="margin:0;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; target.Write(buffer, 0, bytesRead);</p>
<p style="margin:0;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; }</p>
<p style="margin:0;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; }</p>
<p style="margin:0;">&nbsp;&nbsp;&nbsp; }</p>
</div>

now you can do:

<div style="font-family:Courier New;font-size:10pt;color:black;background:white;">
<p style="margin:0;">stream.CopyTo(fileStream);</p>
</div>

If there is a way to copy the contents of one stream to another in the .NET framework, please let me know ;)