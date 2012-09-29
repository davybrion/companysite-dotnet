I was asked to show how you can easily do CRUD tests, so here’s a base class that makes it very easy

<script src="https://gist.github.com/3685693.js?file=s1.cs"></script>

Simply inherit from this class, implement the BuildEntity, ModifyEntity, AssertAreEqual and AssertValidId methods and that’s it. Those methods are usually pretty simple. In BuildEntity you just create an <em>unpersisted</em> entity and assign values to the properties, in ModifyEntity you modify the properties, and in AssertAreEqual you compare the properties of both instances. In AssertValidId, you make sure that the ID value is ok (depending on your identifier strategy).

This is good for regular CRUD operations, though we typically add extra tests when we want to test cascades or one-to-many associations mapped with inverse="true".