One of the downsides of working with a young platform like Silverlight is that the tool support isn't quite 'there' yet, particularly when it comes to profiling for performance and memory usage.  And as you may or may not know, Silverlight makes it very easy to introduce memory leaks into your code.  Since you can't just attach a memory profiler to your Silverlight application, optimizing memory usage or tracking down a leak can be a real pain in the ass.

In the past i have resorted to grabbing a memory dump of the browser and analyzing the content of the managed heap with windbg to track down which instances where being kept in memory.  While this works, it's pretty time consuming and can quite easily lead you down the wrong path.  One of the most important things that you want to know in this specific case is: which types are being kept in memory, and how many of them?

In this case, being able to query something during debugging regarding which instances are still kept alive in memory at certain times is sufficient for me.  So i came up with the following class:

<script src="https://gist.github.com/3685147.js?file=s1.cs"></script>

The idea is basically to just keep a list of WeakReferences of objects you want to track (only if a debugger is attached), and later on you can 'query' the live instances through the GetAllLiveTrackedObjects method.  This method obviously performs a full garbage collect to make sure only objects that are really still alive are returned.  Again, you should almost never do a GC.Collect() manually, but in this case the method will only be called while you're debugging.

Then, you strategically put the following line in the constructor of every kind of type you'd like to track:

<script src="https://gist.github.com/3685147.js?file=s2.cs"></script>

I have that line in the constructor of my base View class, my base Controller class and in my base Disposable class.  So now, when i want to check if there are any dangling references to instances of types that i really don't want hanging around, i can just do something like this:

<script src="https://gist.github.com/3685147.js?file=s3.cs"></script>

And then manually inspect the content of the liveObjects collection.  It's far from perfect, but at least it's an easy way to at least know which instances (that i care about) are being kept in memory.  It'll do until we get better tool support :p