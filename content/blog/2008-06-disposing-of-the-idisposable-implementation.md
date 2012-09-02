Everytime i need to implement the IDisposable interface i have to lookup the recommended way of doing so. That in itself is a bad sign, so i figured i might as well get rid of this by putting the implementation in a reusable base class, based on <a href="http://msdn.microsoft.com/en-us/library/fs2xkftw.aspx">the officially recommended way</a>:

<div>
[csharp]
    public abstract class Disposable : IDisposable
    {
        private bool disposed;
 
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
 
        protected void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    DisposeManagedResources();
                }
 
                DisposeUnmanagedResources();
                disposed = true;
            }
        }
 
        protected void ThrowExceptionIfDisposed()
        {
            if (disposed)
            {
                throw new ObjectDisposedException(GetType().FullName);
            }
        }
 
        protected abstract void DisposeManagedResources();
        protected virtual void DisposeUnmanagedResources() {}
    }
[/csharp]
</div>

So now i can simply inherit from Disposable, and i just need to implement the two abstract methods.  Here's a made up example to illustrate this:

<div>
[csharp]
    public class MyExpensiveResource : Disposable
    {
        private FileStream fileStream;
        private MemoryStream memoryStream;
 
        public MyExpensiveResource(string path)
        {
            fileStream = new FileStream(path, FileMode.Open);
            memoryStream = new MemoryStream();
        }
 
        public void DoSomething()
        {
            ThrowExceptionIfDisposed();
 
            // ... something
        }
 
        protected override void DisposeManagedResources()
        {
            if (fileStream != null) fileStream.Dispose();
            if (memoryStream != null) memoryStream.Dispose();
        }
    }
[/csharp]
</div>

Obviously, you can't use the Disposable base class if you're already inheriting from another base class so in that case you'd still have to implement the IDisposable interface.

<strong>Update</strong>: <a href="http://blog.quantumbitdesigns.com/2008/07/22/a-thread-safe-idisposable-base-class/">Here</a>'s a thread-safe version of this idea.
