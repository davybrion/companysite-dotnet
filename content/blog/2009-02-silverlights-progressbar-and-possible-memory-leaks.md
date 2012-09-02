This has got to be the weirdest memory leak i've ever investigated.  We have this kick-ass Silverlight application, but unfortunately it suffered from very high memory usage that went up rather rapidly.  So i attached windbg to the browser's process, took a memory dump and checked out which objects were still available in the heap.  Much to my surprise, pretty much everything we instantiated was retained in memory and never got removed from the heap.

So i started looking into the usual things: making sure disposable instances where disposed of properly, that evenhandlers were unregistered properly, etc.  I went over the code and it seemed to be alright.  Stepping through the code with a debugger verified that disposables were indeed disposed of, and that all event handlers were unregistered.

So why was pretty much everything kept in memory?  Further research with windbg showed that every ProgressBar instance that was ever created (and we use a lot of them, basically every time we make a call to the application server) kept a reference to the UserControl it was placed on and thus, kept the UserControl and all the references it contained alive.  In our case, that includes our presentation models and obviously all of the contained child UserControls.

The ProgressBar is defined like this:

<div>
[xml]
&lt;ProgressBar Height=&quot;40&quot; Style=&quot;{StaticResource ourKickAssStyle}&quot; VerticalAlignment=&quot;Center&quot; Width=&quot;40&quot; IsIndeterminate=&quot;True&quot;/&gt;
[/xml]
</div>

The key here is the usage of the IsIndeterminate property... setting this to true causes the ProgressBar to move continuously without respecting any current Value property.  You know, basic stuff.  The thing is... if i change the definition of the ProgressBar to this:

<div>
[xml]
&lt;ProgressBar Height=&quot;40&quot; Style=&quot;{StaticResource ourKickAssStyle}&quot; VerticalAlignment=&quot;Center&quot; Width=&quot;40&quot; /&gt;
[/xml]
</div>

The memory leak suddenly went away :)

Obviously, this isn't a solution because the ProgressBar now doesn't really indicate any progress and we need our kick ass custom animation to retain the coolness of the application.

So for some reason, when you set the ProgressBar's IsIndeterminate property to true, it actually keeps all of its references alive even when the ProgressBar control is removed from its parent control.  Happy times.

We now have the following ugly method in one of our base UI classes:

<div>
[csharp]
        private static void StopProgressBars(DependencyObject dependencyObject)
        {
            var count = VisualTreeHelper.GetChildrenCount(dependencyObject);
 
            for (int i = 0; i &lt; count; i++)
            {
                var child = VisualTreeHelper.GetChild(dependencyObject, i);
                if (child != null)
                {
                    var progressBar = child as ProgressBar;
 
                    if (progressBar != null)
                    {
                        progressBar.IsIndeterminate = false;
                    }
 
                    StopProgressBars(child);
                }
            }
        }
[/csharp]
</div>
