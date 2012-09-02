I recently used the excellent <a href="http://code.google.com/p/moq/">Moq</a> mocking library for the first time, and i noticed a difference between Moq and Rhino Mocks (what i usually use) that i found interesting.  

Consider the following useless and contrived example code:

<div>
[csharp]
    public interface ISomeComponent
    {
        void DoSomething();
    }
 
    public class SomeClass
    {
        private ISomeComponent someComponent;
 
        public SomeClass(ISomeComponent someComponent)
        {
            this.someComponent = someComponent;
        }
 
        public void DoSomethingReallyImportant()
        {
            someComponent.DoSomething();
        }
    } 
[/csharp]
</div>

Now suppose that we want to verify in a test that the DoSomethingReallyImportant method of SomeClass actually calls the DoSomething method of its ISomeComponent dependency.

With Moq, we could do that like this:

<div>
[csharp]
    [TestFixture]
    public class TestWithMoq
    {

        [Test]
        public void CallsDoSomethingOnSomeComponent()
        {
            var mock = new Mock&lt;ISomeComponent&gt;();
            var someObject = new SomeClass(mock.Object);
 
            someObject.DoSomethingReallyImportant();
 
            mock.Verify(m =&gt; m.DoSomething());
        }
    }
[/csharp]
</div>

And with Rhino Mocks, it would look like this:

<div>
[csharp]
    [TestFixture]
    public class TestWithMoq
    {
        [Test]
        public void CallsDoSomethingOnSomeComponent()
        {
            var mock = new Mock&lt;ISomeComponent&gt;();
            var someObject = new SomeClass(mock.Object);
 
            someObject.DoSomethingReallyImportant();
 
            mock.Verify(m =&gt; m.DoSomething());
        }
    }
[/csharp]
</div>

Not much of a difference, right? Except that Rhino Mocks provides you with a proxy that implements the ISomeComponent interface and Moq provides you with a generic Mock object, which contains a proxy that implements the ISomeComponent interface and is exposed through the Object property.  Other than that, the tests are very similar.

The key difference is what you experience when you write the tests, as the 2 pictures below will illustrate:

<p>
<img src="http://davybrion.com/blog/wp-content/uploads/2010/08/with_moq.png" alt="" title="with_moq" width="471" height="210" class="aligncenter size-full wp-image-2511" />

<img src="http://davybrion.com/blog/wp-content/uploads/2010/08/with_rhino.png" alt="" title="with_rhino" width="757" height="303" class="aligncenter size-full wp-image-2512" />
</p>

<p>
Since Moq's API is not fully based on Extension Methods, you get a normal and clean IntelliSense experience.  Rhino Mocks on the other hand provides its API (at least the non-legacy stuff) solely through extension methods, which leads to all of them being included in your IntelliSense, even when they don't make any sense at all.

It's obviously not a major issue, but i was suprised with how much i liked <em>not</em> seeing all of the extension methods all the time while writing tests.
</p>