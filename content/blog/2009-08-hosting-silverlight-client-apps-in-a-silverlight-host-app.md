For one of our customers, we need to be able to host multiple Silverlight applications within one dedicated Silverlight 'host' application.  <a href="http://compositewpf.codeplex.com/">Prism</a> offers this ability, but we already have our own implementations for pretty much all of the other stuff that Prism offers (and that we need), so we figured we might as well implement this hosting feature ourselves.  It turns out it's pretty easy to do this.

The general idea is to be able to implement each client application in its own Silverlight Application Project, and to have the ability to download each client aplication's XAP file into the Host application and display it within the designated visual area for the currently active client application.

First, we need an interface that each client application needs to contain an implementation of:

<code>
<style type="text/css">
.cf { font-family: Consolas; font-size: 9pt; color: black; background: white; }
.cl { margin: 0px; }
.cb1 { color: blue; }
.cb2 { color: #2b91af; }
</style>
<div class="cf">
<p class="cl">&nbsp;&nbsp;&nbsp; <span class="cb1">public</span> <span class="cb1">interface</span> <span class="cb2">IClientApplication</span></p>
<p class="cl">&nbsp;&nbsp;&nbsp; {</p>
<p class="cl">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span class="cb1">void</span> Initialize();</p>
<p class="cl">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span class="cb1">void</span> Cleanup();</p>
<p class="cl">&nbsp;</p>
<p class="cl">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span class="cb2">UIElement</span> VisualContainer { <span class="cb1">get</span>; }</p>
<p class="cl">&nbsp;&nbsp;&nbsp; }</p>
</div>
</code>

The VisualContainer property needs to return a UIElement which is basically the main visual container element of the client application.  This could be its main page or just a grid or just about any graphical component.

Then we need a class to download XAP files based on their URI. Since you have to do all of these things asynchronously in Silverlight, and the generally used approach seems to be the event-based async pattern, i use the following base eventargs class for all of my asynchronous operations:

<code>
<style type="text/css">
.cf { font-family: Consolas; font-size: 9pt; color: black; background: white; }
.cl { margin: 0px; }
.cb1 { color: blue; }
.cb2 { color: #2b91af; }
.cb3 { color: #a31515; }
</style>
<div class="cf">
<p class="cl">&nbsp;&nbsp;&nbsp; <span class="cb1">public</span> <span class="cb1">abstract</span> <span class="cb1">class</span> <span class="cb2">AsyncOperationCompletedEventArgs</span>&lt;T&gt; : <span class="cb2">EventArgs</span></p>
<p class="cl">&nbsp;&nbsp;&nbsp; {</p>
<p class="cl">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span class="cb1">protected</span> AsyncOperationCompletedEventArgs(T result, <span class="cb2">Exception</span> error)</p>
<p class="cl">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; {</p>
<p class="cl">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span class="cb1">this</span>.result = result;</p>
<p class="cl">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; Error = error;</p>
<p class="cl">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; }</p>
<p class="cl">&nbsp;</p>
<p class="cl">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span class="cb1">public</span> <span class="cb2">Exception</span> Error { <span class="cb1">get</span>; <span class="cb1">set</span>; }</p>
<p class="cl">&nbsp;</p>
<p class="cl">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span class="cb1">private</span> <span class="cb1">readonly</span> T result;</p>
<p class="cl">&nbsp;</p>
<p class="cl">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span class="cb1">public</span> T Result</p>
<p class="cl">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; {</p>
<p class="cl">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span class="cb1">get</span></p>
<p class="cl">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; {</p>
<p class="cl">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span class="cb1">if</span> (Error != <span class="cb1">null</span>)</p>
<p class="cl">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; {</p>
<p class="cl">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span class="cb1">throw</span> <span class="cb1">new</span> <span class="cb2">InvalidOperationException</span>(<span class="cb3">&quot;The operation did not complete succesfully, please retrieve the Error property&quot;</span>);</p>
<p class="cl">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; }</p>
<p class="cl">&nbsp;</p>
<p class="cl">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span class="cb1">return</span> result;</p>
<p class="cl">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; }</p>
<p class="cl">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; }</p>
<p class="cl">&nbsp;&nbsp;&nbsp; }</p>
</div>
</code>

And then we have the DownloadCompletedEventArgs class:

<code>
<style type="text/css">
.cf { font-family: Consolas; font-size: 9pt; color: black; background: white; }
.cl { margin: 0px; }
.cb1 { color: blue; }
.cb2 { color: #2b91af; }
</style>
<div class="cf">
<p class="cl">&nbsp;&nbsp;&nbsp; <span class="cb1">public</span> <span class="cb1">class</span> <span class="cb2">DownloadCompletedEventArgs</span> : <span class="cb2">AsyncOperationCompletedEventArgs</span>&lt;<span class="cb2">Stream</span>&gt;</p>
<p class="cl">&nbsp;&nbsp;&nbsp; {</p>
<p class="cl">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span class="cb1">public</span> DownloadCompletedEventArgs(<span class="cb2">Stream</span> result, <span class="cb2">Exception</span> e) : <span class="cb1">base</span>(result, e) {}</p>
<p class="cl">&nbsp;&nbsp;&nbsp; }</p>
</div>
</code>

Now we can write our simple FileDownloader class:

<code>
<style type="text/css">
.cf { font-family: Consolas; font-size: 9pt; color: black; background: white; }
.cl { margin: 0px; }
.cb1 { color: blue; }
.cb2 { color: #2b91af; }
</style>
<div class="cf">
<p class="cl">&nbsp;&nbsp;&nbsp; <span class="cb1">public</span> <span class="cb1">interface</span> <span class="cb2">IFileDownloader</span> </p>
<p class="cl">&nbsp;&nbsp;&nbsp; {</p>
<p class="cl">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span class="cb1">event</span> <span class="cb2">EventHandler</span>&lt;<span class="cb2">ProgressChangedEventArgs</span>&gt; ProgressChanged;</p>
<p class="cl">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span class="cb1">event</span> <span class="cb2">EventHandler</span>&lt;<span class="cb2">DownloadCompletedEventArgs</span>&gt; DownloadCompleted;</p>
<p class="cl">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span class="cb1">void</span> StartDownloading(<span class="cb2">Uri</span> locationOfResource);</p>
<p class="cl">&nbsp;&nbsp;&nbsp; }</p>
<p class="cl">&nbsp;</p>
<p class="cl">&nbsp;&nbsp;&nbsp; <span class="cb1">public</span> <span class="cb1">class</span> <span class="cb2">FileDownloader</span> : <span class="cb2">IFileDownloader</span></p>
<p class="cl">&nbsp;&nbsp;&nbsp; {</p>
<p class="cl">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span class="cb1">public</span> <span class="cb1">event</span> <span class="cb2">EventHandler</span>&lt;<span class="cb2">ProgressChangedEventArgs</span>&gt; ProgressChanged = <span class="cb1">delegate</span> { };</p>
<p class="cl">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span class="cb1">public</span> <span class="cb1">event</span> <span class="cb2">EventHandler</span>&lt;<span class="cb2">DownloadCompletedEventArgs</span>&gt; DownloadCompleted = <span class="cb1">delegate</span> { };</p>
<p class="cl">&nbsp;</p>
<p class="cl">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span class="cb1">public</span> <span class="cb1">void</span> StartDownloading(<span class="cb2">Uri</span> locationOfResource)</p>
<p class="cl">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; {</p>
<p class="cl">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span class="cb1">var</span> webClient = <span class="cb1">new</span> <span class="cb2">WebClient</span>();</p>
<p class="cl">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; webClient.DownloadProgressChanged += webClient_DownloadProgressChanged;</p>
<p class="cl">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; webClient.OpenReadCompleted += webClient_OpenReadCompleted;</p>
<p class="cl">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; webClient.OpenReadAsync(locationOfResource);</p>
<p class="cl">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; }</p>
<p class="cl">&nbsp;</p>
<p class="cl">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span class="cb1">private</span> <span class="cb1">void</span> webClient_OpenReadCompleted(<span class="cb1">object</span> sender, <span class="cb2">OpenReadCompletedEventArgs</span> e)</p>
<p class="cl">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; {</p>
<p class="cl">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span class="cb2">DownloadCompletedEventArgs</span> eventArgs;</p>
<p class="cl">&nbsp;</p>
<p class="cl">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span class="cb1">if</span> (!e.Cancelled &amp;&amp; e.Error == <span class="cb1">null</span>)</p>
<p class="cl">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; {</p>
<p class="cl">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; eventArgs = <span class="cb1">new</span> <span class="cb2">DownloadCompletedEventArgs</span>(e.Result, <span class="cb1">null</span>);</p>
<p class="cl">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; }</p>
<p class="cl">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span class="cb1">else</span></p>
<p class="cl">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; {</p>
<p class="cl">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; eventArgs = <span class="cb1">new</span> <span class="cb2">DownloadCompletedEventArgs</span>(<span class="cb1">null</span>, e.Error);</p>
<p class="cl">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; }</p>
<p class="cl">&nbsp;</p>
<p class="cl">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; DownloadCompleted(<span class="cb1">this</span>, eventArgs);</p>
<p class="cl">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; }</p>
<p class="cl">&nbsp;</p>
<p class="cl">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span class="cb1">private</span> <span class="cb1">void</span> webClient_DownloadProgressChanged(<span class="cb1">object</span> sender, <span class="cb2">DownloadProgressChangedEventArgs</span> e)</p>
<p class="cl">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; {</p>
<p class="cl">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; ProgressChanged(<span class="cb1">this</span>, <span class="cb1">new</span> <span class="cb2">ProgressChangedEventArgs</span>(e.ProgressPercentage, <span class="cb1">null</span>));</p>
<p class="cl">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; }</p>
<p class="cl">&nbsp;&nbsp;&nbsp; }</p>
</div>
</code>

We also need something to extract the assemblies from a downloaded XAP file:

<code>
<style type="text/css">
.cf { font-family: Consolas; font-size: 9pt; color: black; background: white; }
.cl { margin: 0px; }
.cb1 { color: blue; }
.cb2 { color: #2b91af; }
.cb3 { color: #a31515; }
</style>
<div class="cf">
<p class="cl">&nbsp;&nbsp;&nbsp; <span class="cb1">public</span> <span class="cb1">interface</span> <span class="cb2">IXapReader</span> </p>
<p class="cl">&nbsp;&nbsp;&nbsp; {</p>
<p class="cl">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span class="cb2">IEnumerable</span>&lt;<span class="cb2">Assembly</span>&gt; GetAssemblies(<span class="cb2">Stream</span> stream);</p>
<p class="cl">&nbsp;&nbsp;&nbsp; }</p>
<p class="cl">&nbsp;</p>
<p class="cl">&nbsp;&nbsp;&nbsp; <span class="cb1">public</span> <span class="cb1">class</span> <span class="cb2">XapReader</span> : <span class="cb2">IXapReader</span></p>
<p class="cl">&nbsp;&nbsp;&nbsp; {</p>
<p class="cl">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span class="cb1">public</span> <span class="cb2">IEnumerable</span>&lt;<span class="cb2">Assembly</span>&gt; GetAssemblies(<span class="cb2">Stream</span> stream)</p>
<p class="cl">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; {</p>
<p class="cl">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span class="cb1">var</span> assemblies = <span class="cb1">new</span> <span class="cb2">List</span>&lt;<span class="cb2">Assembly</span>&gt;();</p>
<p class="cl">&nbsp;</p>
<p class="cl">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span class="cb1">foreach</span> (<span class="cb1">var</span> deploymentPart <span class="cb1">in</span> GetDeploymentParts(stream))</p>
<p class="cl">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; {</p>
<p class="cl">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; assemblies.Add(LoadAssembly(deploymentPart, stream));</p>
<p class="cl">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; }</p>
<p class="cl">&nbsp;</p>
<p class="cl">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span class="cb1">return</span> assemblies;</p>
<p class="cl">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; }</p>
<p class="cl">&nbsp;</p>
<p class="cl">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span class="cb1">private</span> <span class="cb1">static</span> <span class="cb2">Assembly</span> LoadAssembly(<span class="cb2">XElement</span> deploymentPart, <span class="cb2">Stream</span> stream)</p>
<p class="cl">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; {</p>
<p class="cl">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span class="cb1">var</span> assemblyPart = <span class="cb1">new</span> <span class="cb2">AssemblyPart</span>();</p>
<p class="cl">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span class="cb1">var</span> source = deploymentPart.Attribute(<span class="cb3">&quot;Source&quot;</span>).Value;</p>
<p class="cl">&nbsp;</p>
<p class="cl">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span class="cb1">var</span> streamResourceInfo = <span class="cb2">Application</span>.GetResourceStream(</p>
<p class="cl">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span class="cb1">new</span> <span class="cb2">StreamResourceInfo</span>(stream, <span class="cb3">&quot;application/binary&quot;</span>),</p>
<p class="cl">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span class="cb1">new</span> <span class="cb2">Uri</span>(source, <span class="cb2">UriKind</span>.Relative));</p>
<p class="cl">&nbsp;</p>
<p class="cl">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span class="cb1">return</span> assemblyPart.Load(streamResourceInfo.Stream);</p>
<p class="cl">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; }</p>
<p class="cl">&nbsp;</p>
<p class="cl">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span class="cb1">private</span> <span class="cb1">static</span> <span class="cb2">IEnumerable</span>&lt;<span class="cb2">XElement</span>&gt; GetDeploymentParts(<span class="cb2">Stream</span> stream)</p>
<p class="cl">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; {</p>
<p class="cl">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span class="cb1">var</span> appManifest = GetApplicationManifest(stream);</p>
<p class="cl">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span class="cb1">var</span> deploymentRoot = <span class="cb2">XDocument</span>.Parse(appManifest).Root;</p>
<p class="cl">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span class="cb1">return</span> deploymentRoot.Elements().First().Elements();</p>
<p class="cl">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; }</p>
<p class="cl">&nbsp;</p>
<p class="cl">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span class="cb1">private</span> <span class="cb1">static</span> <span class="cb1">string</span> GetApplicationManifest(<span class="cb2">Stream</span> stream)</p>
<p class="cl">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; {</p>
<p class="cl">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span class="cb1">return</span> <span class="cb1">new</span> <span class="cb2">StreamReader</span>(<span class="cb2">Application</span>.GetResourceStream(</p>
<p class="cl">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span class="cb1">new</span> <span class="cb2">StreamResourceInfo</span>(stream, <span class="cb1">null</span>),</p>
<p class="cl">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span class="cb1">new</span> <span class="cb2">Uri</span>(<span class="cb3">&quot;AppManifest.xaml&quot;</span>, <span class="cb2">UriKind</span>.Relative)).Stream)</p>
<p class="cl">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; .ReadToEnd();</p>
<p class="cl">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; }</p>
<p class="cl">&nbsp;&nbsp;&nbsp; }</p>
</div>
</code>

And then we can write a class which can retrieve the type which implements the IClientApplication interface from a downloaded XAP file:

<code>
<style type="text/css">
.cf { font-family: Consolas; font-size: 9pt; color: black; background: white; }
.cl { margin: 0px; }
.cb1 { color: blue; }
.cb2 { color: #2b91af; }
</style>
<div class="cf">
<p class="cl">&nbsp;&nbsp;&nbsp; <span class="cb1">public</span> <span class="cb1">interface</span> <span class="cb2">IClientApplicationTypeLoader</span> </p>
<p class="cl">&nbsp;&nbsp;&nbsp; {</p>
<p class="cl">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span class="cb1">event</span> <span class="cb2">EventHandler</span>&lt;<span class="cb2">ClientApplicationLoadedEventArgs</span>&gt; LoadCompleted;</p>
<p class="cl">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span class="cb1">event</span> <span class="cb2">EventHandler</span>&lt;<span class="cb2">ProgressChangedEventArgs</span>&gt; ProgressChanged;</p>
<p class="cl">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span class="cb1">void</span> Load(<span class="cb2">Uri</span> xapLocation);</p>
<p class="cl">&nbsp;&nbsp;&nbsp; }</p>
<p class="cl">&nbsp;</p>
<p class="cl">&nbsp;&nbsp;&nbsp; <span class="cb1">public</span> <span class="cb1">class</span> <span class="cb2">ClientApplicationTypeLoader</span> : <span class="cb2">IClientApplicationTypeLoader</span></p>
<p class="cl">&nbsp;&nbsp;&nbsp; {</p>
<p class="cl">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span class="cb1">public</span> <span class="cb1">event</span> <span class="cb2">EventHandler</span>&lt;<span class="cb2">ClientApplicationLoadedEventArgs</span>&gt; LoadCompleted = <span class="cb1">delegate</span> { };</p>
<p class="cl">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span class="cb1">public</span> <span class="cb1">event</span> <span class="cb2">EventHandler</span>&lt;<span class="cb2">ProgressChangedEventArgs</span>&gt; ProgressChanged = <span class="cb1">delegate</span> { };</p>
<p class="cl">&nbsp;</p>
<p class="cl">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span class="cb1">private</span> <span class="cb2">IFileDownloader</span> fileDownloader;</p>
<p class="cl">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span class="cb1">private</span> <span class="cb2">IXapReader</span> xapReader;</p>
<p class="cl">&nbsp;</p>
<p class="cl">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span class="cb1">public</span> ClientApplicationTypeLoader(<span class="cb2">IFileDownloader</span> fileDownloader, <span class="cb2">IXapReader</span> xapReader)</p>
<p class="cl">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; {</p>
<p class="cl">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span class="cb1">this</span>.fileDownloader = fileDownloader;</p>
<p class="cl">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span class="cb1">this</span>.xapReader = xapReader;</p>
<p class="cl">&nbsp;</p>
<p class="cl">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; fileDownloader.ProgressChanged += downloader_ProgressChanged;</p>
<p class="cl">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; fileDownloader.DownloadCompleted += downloader_DownloadCompleted;</p>
<p class="cl">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; }</p>
<p class="cl">&nbsp;</p>
<p class="cl">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span class="cb1">public</span> <span class="cb1">void</span> Load(<span class="cb2">Uri</span> xapLocation)</p>
<p class="cl">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; {</p>
<p class="cl">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; fileDownloader.StartDownloading(xapLocation);</p>
<p class="cl">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; }</p>
<p class="cl">&nbsp;</p>
<p class="cl">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span class="cb1">private</span> <span class="cb1">void</span> downloader_ProgressChanged(<span class="cb1">object</span> sender, <span class="cb2">ProgressChangedEventArgs</span> e)</p>
<p class="cl">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; {</p>
<p class="cl">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; ProgressChanged(<span class="cb1">this</span>, e);</p>
<p class="cl">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; }</p>
<p class="cl">&nbsp;</p>
<p class="cl">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span class="cb1">private</span> <span class="cb1">void</span> downloader_DownloadCompleted(<span class="cb1">object</span> sender, <span class="cb2">DownloadCompletedEventArgs</span> e)</p>
<p class="cl">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; {</p>
<p class="cl">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span class="cb1">if</span> (e.Error != <span class="cb1">null</span>)</p>
<p class="cl">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; {</p>
<p class="cl">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; LoadCompleted(<span class="cb1">this</span>, <span class="cb1">new</span> <span class="cb2">ClientApplicationLoadedEventArgs</span>(<span class="cb1">null</span>, e.Error));</p>
<p class="cl">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span class="cb1">return</span>;</p>
<p class="cl">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; }</p>
<p class="cl">&nbsp;</p>
<p class="cl">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span class="cb1">var</span> assemblies = xapReader.GetAssemblies(e.Result);</p>
<p class="cl">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span class="cb1">var</span> clientApplicationType = FindClientApplicationType(assemblies);</p>
<p class="cl">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; LoadCompleted(<span class="cb1">this</span>, <span class="cb1">new</span> <span class="cb2">ClientApplicationLoadedEventArgs</span>(clientApplicationType, <span class="cb1">null</span>));</p>
<p class="cl">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; }</p>
<p class="cl">&nbsp;</p>
<p class="cl">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span class="cb1">private</span> <span class="cb2">Type</span> FindClientApplicationType(<span class="cb2">IEnumerable</span>&lt;<span class="cb2">Assembly</span>&gt; assemblies)</p>
<p class="cl">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; {</p>
<p class="cl">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span class="cb1">foreach</span> (<span class="cb1">var</span> assembly <span class="cb1">in</span> assemblies)</p>
<p class="cl">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; {</p>
<p class="cl">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span class="cb1">foreach</span> (<span class="cb1">var</span> type <span class="cb1">in</span> assembly.GetTypes())</p>
<p class="cl">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; {</p>
<p class="cl">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span class="cb1">if</span> (type.GetInterfaces().Contains(<span class="cb1">typeof</span>(<span class="cb2">IClientApplication</span>)))</p>
<p class="cl">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; {</p>
<p class="cl">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span class="cb1">return</span> type;</p>
<p class="cl">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; }</p>
<p class="cl">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; }</p>
<p class="cl">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; }</p>
<p class="cl">&nbsp;</p>
<p class="cl">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span class="cb1">return</span> <span class="cb1">null</span>;</p>
<p class="cl">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; }</p>
<p class="cl">&nbsp;&nbsp;&nbsp; }</p>
</div>
</code>

Obviously, this code doesn't deal with the possibility of multiple or no IClientApplication implementations within the same XAP file.  Not really relevant for this post though ;)

And now, we can simply do something like this within our host application:

<code>
<style type="text/css">
.cf { font-family: Consolas; font-size: 9pt; color: black; background: white; }
.cl { margin: 0px; }
</style>
<div class="cf">
<p class="cl">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; clientApplicationTypeLoader.LoadCompleted += loader_LoadCompleted;</p>
<p class="cl">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; clientApplicationTypeLoader.ProgressChanged += loader_ProgressChanged;</p>
<p class="cl">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; clientApplicationTypeLoader.Load(xapUri);</p>
</div>
</code>

and the LoadCompleted event handler could look like this:

<code>
<style type="text/css">
.cf { font-family: Consolas; font-size: 9pt; color: black; background: white; }
.cl { margin: 0px; }
.cb1 { color: blue; }
.cb2 { color: #2b91af; }
.cb3 { color: #a31515; }
</style>
<div class="cf">
<p class="cl">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span class="cb1">void</span> loader_LoadCompleted(<span class="cb1">object</span> sender, <span class="cb2">ClientApplicationLoadedEventArgs</span> e)</p>
<p class="cl">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; {</p>
<p class="cl">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span class="cb1">if</span> (e.Error != <span class="cb1">null</span>)</p>
<p class="cl">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; {</p>
<p class="cl">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span class="cb1">throw</span> <span class="cb1">new</span> <span class="cb2">Exception</span>(<span class="cb3">&quot;error while retrieving client application&quot;</span>, e.Error);</p>
<p class="cl">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; }</p>
<p class="cl">&nbsp;</p>
<p class="cl">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; CleanupCurrentClientApp();</p>
<p class="cl">&nbsp;</p>
<p class="cl">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span class="cb1">var</span> clientApp = (<span class="cb2">IClientApplication</span>)<span class="cb2">Activator</span>.CreateInstance(e.Result);</p>
<p class="cl">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; clientApp.Initialize();</p>
<p class="cl">&nbsp;</p>
<p class="cl">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; AppContainer.Children.Add(clientApp.VisualContainer);</p>
<p class="cl">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; }</p>
<p class="cl">&nbsp;</p>
<p class="cl">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span class="cb1">private</span> <span class="cb1">void</span> CleanupCurrentClientApp()</p>
<p class="cl">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; {</p>
<p class="cl">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span class="cb1">if</span> (AppContainer.Children.Count == 0)</p>
<p class="cl">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; {</p>
<p class="cl">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span class="cb1">return</span>;</p>
<p class="cl">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; }</p>
<p class="cl">&nbsp;</p>
<p class="cl">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span class="cb1">var</span> currentClientApp = (<span class="cb2">IClientApplication</span>)AppContainer.Children[0];</p>
<p class="cl">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; currentClientApp.Cleanup();</p>
<p class="cl">&nbsp;</p>
<p class="cl">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; AppContainer.Children.Clear();</p>
<p class="cl">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; }</p>
</div>
</code>

And that's all there is to it.

Update: download a sample of this <a href="http://davybrion.com/blog/wp-content/uploads/2009/08/SilverlightHost.zip">here</a>.