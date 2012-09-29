I wrote a post last week about a memory leak I had introduced in my code due to <a href="/blog/2008/12/the-importance-of-releasing-your-components-through-windsor/">not properly releasing resolved components through the Windsor IoC container</a>.  I wanted to try to make sure that I'd never make that mistake again and this is the approach I came up with.

If you're using an IOC container it's important to not use it all over the place.  You basically use it in as few places as possible to resolve a component and you let the container sort out all of the dependencies.  So in the few places where you use the container directly, you need to resolve the component, and in case of transient components you also need to release them through the container.  Releasing it is very easy to forget, so I wanted something that would guarantee that the component would be properly released.  Enter the Resolvable class:

<script src="https://gist.github.com/3684176.js?file=s1.cs"></script>

The Resolvable class inherits from my <a href="/blog/2008/06/disposing-of-the-idisposable-implementation/">Disposable class</a>, so the Disposable pattern is correctly implemented.

From now on, instead of calling the container directly, I just instantiate a new Resolvable in a using block.  Let's try it out.

I'm reusing my test component with a dependency from one of the previous posts:

<script src="https://gist.github.com/3684176.js?file=s2.cs"></script>

Now, instead of resolving an IController directly through the container and having to dispose of it, I just do this:

<script src="https://gist.github.com/3684176.js?file=s3.cs"></script>

The container doesn't hold the reference to the instance, and both the instance and its dependency is properly disposed. 