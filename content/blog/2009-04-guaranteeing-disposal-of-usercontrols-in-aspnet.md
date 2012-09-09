One of our applications suffered from a memory leak because one of ASP.NET's UserControls (in this case, the Repeater) created instances of one of our own UserControl type without disposing them afterward.  In most cases, this isn't really a big issue, but if your UserControl really requires explicit Disposal this can obviously be a pretty big problem.

In order to prevent this situation from ever happening again, i came up with an approach which guarantees that all instances of UserControls that require explicit disposal are indeed properly disposed at the end of the request in which they were created.  I don't really like this approach as i consider it a hack.  But then again, when ASP.NET controls fail to dispose the controls they create in some occasions, all bets are off.

And so the Disposer class was born:

<script src="https://gist.github.com/3684467.js?file=s1.cs"></script>

Ugly stuff, right? It gets worse.

In the constructor of the UserControl(s) that really need(s) to be disposed, add the following line:

<script src="https://gist.github.com/3684467.js?file=s2.cs"></script>

Then, we have our own custom HttpModule to complete this little hack-fest:

<script src="https://gist.github.com/3684467.js?file=s3.cs"></script>

All in all, pretty horrible stuff if you ask me.  But at least we're sure now that all instances of the UserControl are always properly disposed.  