As i mentioned in a <a href="/blog/2009/08/tracking-dangling-object-references-in-silverlight/">previous post</a>, you can attach WinDbg to a browser to find memory leaks in your Silverlight applications.  I figured it would be a good idea to write down how this process works, since i always end up having to look it up again whenever i need to do this.

I wrote a very simple Silverlight application which has a rather typical memory leak. Here's the actual code:

<script src="https://gist.github.com/3685131.js?file=s1.cs"></script>

For some of you, the memory leak is already very clear.  Like i said, it's a very simple example ;)

Let's go through the process of finding and fixing the memory leak using WinDbg. First of all, download Debugging Tools For Windows (which contains the WinDbg executable) <a href="http://www.microsoft.com/whdc/devtools/debugging/installx86.mspx">here</a> and install it.

Then we start our application in Internet Explorer (for some reason i can't use WinDbg to inspect the managed memory heap with Firefox, so i just use Internet Explorer for this stuff) and use it.  In the case of my example, that means clicking the button which creates a new view a couple of times.

Open WinDbg.exe and select the 'Attach to a Process' menu item in the 'File' menu and select the iexplore.exe process.

Then you need to load the correct version of sos.dll:

<p>
<img src="/blog/wp-content/uploads/2009/08/step1.png" alt="step1" title="step1" width="606" height="15" class="size-full wp-image-1498" />
</p>

After that we can see which types of our MySilverlightApplication namespace are present in the managed heap, including how many instances of them:

<p>
<img src="/blog/wp-content/uploads/2009/08/step2.png" alt="step2" title="step2" width="952" height="122" class="size-full wp-image-1499" />
</p>

As you can see, there are 13 instances of our MyView type present in the heap.  Using the value in the MT column, we can drill down further:

<p>
<img src="/blog/wp-content/uploads/2009/08/step3.png" alt="step3" title="step3" width="512" height="266" class="size-full wp-image-1500" />
</p>

This shows the memory address of each instance of the MyView type in the heap.  Now we can see if there are any live references to these instances:

<p>
<img src="/blog/wp-content/uploads/2009/08/step4.png" alt="step4" title="step4" width="831" height="192" class="size-full wp-image-1501" />
</p>

This is actually for the first address that was listed.  As you can see, it is still a reachable reference, which means it will not be collected by the garbage collector. The chain of references clearly indicates that the instance is still referenced from our event in the MainPage instance.  All of the previously listed instances show the same reference chain, so this is clearly a memory leak.  Even though we should only have one active reference of MyView at any point in time of this application, the MyEvent event on MainPage clearly keeps each instance of MyView alive. 

The correct way to fix this is to make sure that whenever we remove an instance of MyView, we need to unsubscribe it from the MainPage's MyEvent handler.  Always remember this rule when it comes to dealing with events: if the publisher of the event has a longer lifetime than the subscriber of the event, then you absolutely have to unsubscribe each subscriber from the event or the publisher will keep references to each subscriber (preventing them from being garbage collected) for as long as the publisher is alive.

Here's the modified version of the above code which avoids the memory leak:

<script src="https://gist.github.com/3685131.js?file=s2.cs"></script>

Let's see if this really fixed the memory leak.  If we fire up the application and press the button a couple of times, the application should normally only have one live reference of MyView in memory.

<p>
<img src="/blog/wp-content/uploads/2009/08/step5.png" alt="step5" title="step5" width="952" height="125" class="size-full wp-image-1502" />
</p>

I clicked the button 5 times, and the above output shows that there are 5 instances of MyView on the heap.  So did we fix the leak or not?  Check the output below:

<p>
<img src="/blog/wp-content/uploads/2009/08/step6.png" alt="step6" title="step6" width="622" height="604" class="size-full wp-image-1497" />
</p>

As you can see, only the last instance of MyView is actively referenced somewhere.  That means that the first 4 instances are ready to be collected during the next garbage collection.  

One thing i don't understand though, is that the reference chain of the last instance doesn't mention MainPage or the event handler anymore.  But when i attached Visual Studio's debugger to the browser instance i could clearly see that the MyEvent of MainPage indeed contained an event handler that pointed to this MyView instance.  I'm far from a WinDbg and SOS expert so i have no idea why the reference chain doesn't reflect this.  Perhaps someone with more WinDbg and SOS knowledge can shed some light on this?

Either way, this approach is a pretty good way of finding memory leaks in your Silverlight code.  In a real application it's obviously a bit more complicated to find the exact cause of a leak compared to this simple example, but it's still pretty doable.  Just execute the !dumpheap -stat -type YourRootNameSpaceHere and look for unusually high numbers of instances of your types.  Then you can start looking at each instance to figure out what's going on.  And for a nice list of commands that you can execute in WinDbg with SOS, be sure to check <a href="http://msdn.microsoft.com/en-us/library/bb190764.aspx">this</a> out.

Also, keep in mind that you can do this for every .NET process, and not just Silverlight.  Though you would need to load the sos.dll file of your particular .NET version.