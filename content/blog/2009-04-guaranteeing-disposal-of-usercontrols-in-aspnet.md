As i mentioned <a href="http://davybrion.com/blog/2009/04/the-joys-of-debugging-aspnet-memory-leaks/">recently</a>, one of our applications suffered from a memory leak because one of ASP.NET's UserControls (in this case, the Repeater) created instances of one of our own UserControl type without disposing them afterward.  In most cases, this isn't really a big issue, but if your UserControl really requires explicit Disposal this can obviously be a pretty big problem.

In order to prevent this situation from ever happening again, i came up with an approach which guarantees that all instances of UserControls that require explicit disposal are indeed properly disposed at the end of the request in which they were created.  I don't really like this approach as i consider it a hack.  But then again, when ASP.NET controls fail to dispose the controls they create in some occasions, all bets are off.

And so the Disposer class was born:

<div>
[csharp]
    public static class Disposer
    {
        private const string DisposalEnabledKey = &quot;_disposeTrackedObjects&quot;;
        private const string DisposableObjectsKey = &quot;_disposableObjects&quot;;
 
        public static void EnableDisposalOfTrackedObjectsForCurrentRequest()
        {
            HttpContext.Current.Items[DisposalEnabledKey] = true;
            HttpContext.Current.Items[DisposableObjectsKey] = new List&lt;WeakReference&gt;();
        }
 
        public static void RegisterForGuaranteedDisposal(IDisposable disposable)
        {
            if (GuaranteedDisposalIsEnabled())
            {
                var disposables = GetTrackedDisposables();
                disposables.Add(new WeakReference(disposable));
            }
        }
 
        public static void DisposeTrackedReferences()
        {
            var disposables = GetTrackedDisposables();
 
            foreach (var reference in disposables)
            {
                if (reference.IsAlive)
                {
                    var disposable = reference.Target as IDisposable;
                    if (disposable != null) disposable.Dispose();
                }
            }
        }
 
        private static bool GuaranteedDisposalIsEnabled()
        {
            var value = HttpContext.Current.Items[DisposalEnabledKey];
 
            if (value == null)
            {
                return false;
            }
 
            return (bool)value;
        }
 
        private static List&lt;WeakReference&gt; GetTrackedDisposables()
        {
            return HttpContext.Current.Items[DisposableObjectsKey] as List&lt;WeakReference&gt; ?? new List&lt;WeakReference&gt;();
        }
    }
[/csharp]
</div>

Ugly stuff, right? It gets worse.

In the constructor of the UserControl(s) that really need(s) to be disposed, add the following line:

<div>
[csharp]
            Disposer.RegisterForGuaranteedDisposal(this);
[/csharp]
</div>

Then, we have our own custom HttpModule to complete this little hack-fest:

<div>
[csharp]
    public class OurKickAssHttpModule : IHttpModule
    {
        public void Init(HttpApplication context)
        {
            context.BeginRequest += context_BeginRequest;
            context.EndRequest += context_EndRequest;
        }
 
        private void context_BeginRequest(object sender, EventArgs e)
        {
            Disposer.EnableDisposalOfTrackedObjectsForCurrentRequest();
        }
 
        private void context_EndRequest(object sender, EventArgs e)
        {
            Disposer.DisposeTrackedReferences();
        }
 
        // not really needed here but it's required by IHttpModule
        public void Dispose() {}
    }
[/csharp]
</div>

All in all, pretty horrible stuff if you ask me.  But at least we're sure now that all instances of the UserControl are always properly disposed.  