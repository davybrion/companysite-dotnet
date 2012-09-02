I've been playing around with <a href="http://www.jetbrains.com/teamcity/">TeamCity</a> a lot lately. It's really amazing how easy to use and powerful it is. Definitely my favorite CI server for the time being... i'm even running it at home now for my personal projects.

There was one issue at work that was somewhat hard to fix though. TeamCity has fantastic integrated support for NUnit, but unfortunately it doesn't write NUnit's output to XML files like nunit-console.exe does.  After a bit of browsing, i found a <a href="http://intellij.net/forums/message.jspa?messageID=5218450#5218450">post</a> on the TeamCity forums that discussed a workaround on how to get the output. Unfortunately the workaround is a bit cumbersome as it requires you to you create an XML file which contains the arguments that TeamCity would pass to its NUnitLauncher task. 

I believe in 'script reuse' as much as i believe in 'code reuse', so every project's build script merely imports a generic build script and then it just overwrites some variables that the generic script uses and then it kicks off the usual build process.  Since all of our projects will now be built by TeamCity Build Agents, and we definitely need to have NUnit's results xml file i wanted to automate this whole thing as much as possible.

So instead of having to create (and thus, maintain) a cumbersome xml file for each project, i wrote an MSBuild task that would generate the xml file containing the arguments on the fly during the build, and would then pass those xml arguments to TeamCity's NUnitLauncher.  This way we get to keep all of TeamCity's NUnit integration goodness, while still keeping the NUnit result files around as well.

So now in my MSBuild file, i can just do this:

<code>
<div style="font-family: Consolas; font-size: 10pt; color: black; background: white;">
<p style="margin: 0px;"><span style="color: blue;">&nbsp; &nbsp; &lt;</span><span style="color: #a31515;">CreateItem</span><span style="color: blue;"> </span><span style="color: red;">Include</span><span style="color: blue;">=</span>"<span style="color: blue;">**\Bin\Debug\*Tests*.dll</span>"<span style="color: blue;"> &gt;</span></p>
<p style="margin: 0px;"><span style="color: blue;">&nbsp; &nbsp; &nbsp; &lt;</span><span style="color: #a31515;">Output</span><span style="color: blue;"> </span><span style="color: red;">TaskParameter</span><span style="color: blue;">=</span>"<span style="color: blue;">Include</span>"<span style="color: blue;"> </span><span style="color: red;">ItemName</span><span style="color: blue;">=</span>"<span style="color: blue;">TestAssemblies</span>"<span style="color: blue;"> /&gt;</span></p>
<p style="margin: 0px;"><span style="color: blue;">&nbsp; &nbsp; &lt;/</span><span style="color: #a31515;">CreateItem</span><span style="color: blue;">&gt;</span></p>
<p style="margin: 0px;">&nbsp;</p>
<p style="margin: 0px;"><span style="color: blue;">&nbsp; &nbsp; &lt;</span><span style="color: #a31515;">BuildTeamCityNUnitArguments</span><span style="color: blue;"> </span><span style="color: red;">HaltOnError</span><span style="color: blue;">=</span>"<span style="color: blue;">true</span>"<span style="color: blue;"> </span><span style="color: red;">HaltOnFirstTestFailure</span><span style="color: blue;">=</span>"<span style="color: blue;">true</span>"</p>
<p style="margin: 0px;"><span style="color: blue;">&nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp;  </span><span style="color: red;">HaltOnFailureAtEnd</span><span style="color: blue;">=</span>"<span style="color: blue;">true</span>"<span style="color: blue;"> </span><span style="color: red;">TestAssemblies</span><span style="color: blue;">=</span>"<span style="color: blue;">@(TestAssemblies)</span>"</p>
<p style="margin: 0px;"><span style="color: blue;">&nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp;  </span><span style="color: red;">NUnitResultsOutputFolder</span><span style="color: blue;">=</span>"<span style="color: blue;">TestResults</span>"<span style="color: blue;"> </span></p>
<p style="margin: 0px;"><span style="color: blue;">&nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp;  </span><span style="color: red;">PathOfNUnitArgumentsXmlFile</span><span style="color: blue;">=</span>"<span style="color: blue;">nunitarguments.xml</span>"<span style="color: blue;"> /&gt;</span></p>
<p style="margin: 0px;">&nbsp;</p>
<p style="margin: 0px;">&nbsp;</p>
<p style="margin: 0px;"><span style="color: blue;">&nbsp; &nbsp; &lt;</span><span style="color: #a31515;">Exec</span><span style="color: blue;"> </span><span style="color: red;">Command</span><span style="color: blue;">=</span>"<span style="color: blue;">$(teamcity_dotnet_nunitlauncher) @@ nunitarguments.xml</span>"<span style="color: blue;"> /&gt;</span></p>
</div>
</code>

As you can see, there is nothing project-specific in there so this just integrates nicely into each build that we need.  The only limitation to this approach is that TeamCity's NUnitLauncher will write one NUnit result file for each assembly containing tests. Apparently there is no way in the current version of TeamCity to get it to combine those results.  We use an extra tool to analyze the NUnit output after the tests have run, and unfortunately it requires all of the results to be in one file.  I looked around for a way to merge the output of several NUnit result files, but i didn't find something that was already available.  So i wrote another MSBuild task that would merge the output into one xml file:

<code>
<div style="font-family: Consolas; font-size: 10pt; color: black; background: white;">
<p style="margin: 0px;"><span style="color: blue;">&nbsp; &nbsp; &lt;</span><span style="color: #a31515;">CreateItem</span><span style="color: blue;"> </span><span style="color: red;">Include</span><span style="color: blue;">=</span>"<span style="color: blue;">TestResults\*.xml</span>"<span style="color: blue;"> &gt;</span></p>
<p style="margin: 0px;"><span style="color: blue;">&nbsp; &nbsp; &nbsp; &lt;</span><span style="color: #a31515;">Output</span><span style="color: blue;"> </span><span style="color: red;">TaskParameter</span><span style="color: blue;">=</span>"<span style="color: blue;">Include</span>"<span style="color: blue;"> </span><span style="color: red;">ItemName</span><span style="color: blue;">=</span>"<span style="color: blue;">NUnitOutputXmlFiles</span>"<span style="color: blue;">/&gt;</span></p>
<p style="margin: 0px;"><span style="color: blue;">&nbsp; &nbsp; &lt;/</span><span style="color: #a31515;">CreateItem</span><span style="color: blue;">&gt;</span></p>
<p style="margin: 0px;">&nbsp;</p>
<p style="margin: 0px;"><span style="color: blue;">&nbsp; &nbsp; &lt;</span><span style="color: #a31515;">NUnitMergeOutput</span><span style="color: blue;"> </span><span style="color: red;">NUnitOutputXmlFiles</span><span style="color: blue;">=</span>"<span style="color: blue;">@(NUnitOutputXmlFiles)</span>"<span style="color: blue;"> </span></p>
<p style="margin: 0px;"><span style="color: blue;">&nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; </span><span style="color: red;">PathOfMergedXmlFile</span><span style="color: blue;">=</span>"<span style="color: blue;">TestResults.xml</span>"<span style="color: blue;"> /&gt;</span></p>
</div>
</code>

Now we finally have all of TeamCity's goodness while we still get to run our post-test analysis on the NUnit result file that contains the testresults of all the test assemblies :)

You can find the code of the MSBuild tasks <a href="http://davybrion.com/blog/stuff/">here</a>.  They are BSD-licensed so feel free to use them if you need them.