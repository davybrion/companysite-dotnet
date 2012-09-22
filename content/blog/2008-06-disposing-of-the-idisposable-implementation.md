Everytime I need to implement the IDisposable interface I have to lookup the recommended way of doing so. That in itself is a bad sign, so I figured I might as well get rid of this by putting the implementation in a reusable base class, based on <a href="http://msdn.microsoft.com/en-us/library/fs2xkftw.aspx">the officially recommended way</a>:

<script src="https://gist.github.com/3656247.js?file=s1.cs"></script>

So now I can simply inherit from Disposable, and I just need to implement the two abstract methods.  Here's a made up example to illustrate this:

<script src="https://gist.github.com/3656247.js?file=s2.cs"></script>

Obviously, you can't use the Disposable base class if you're already inheriting from another base class so in that case you'd still have to implement the IDisposable interface.

<strong>Update</strong>: <a href="http://blog.quantumbitdesigns.com/2008/07/22/a-thread-safe-idisposable-base-class/">Here</a>'s a thread-safe version of this idea.
