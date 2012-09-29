Suppose we have the following set of classes in an ASP.NET MVC project:

<script src="https://gist.github.com/3728590.js?file=s1.cs"></script>

An input form for an instance of PersonModel could look like this:

<script src="https://gist.github.com/3728590.js?file=s2.html"></script>

That would produce HTML like this:

<script src="https://gist.github.com/3728590.js?file=s3.html"></script>

Which is perfect, because this means we get client-side validation for free, based on the DataAnnotations that we used on our model classes.  Also, pay attention to the values of the name attributes for the input elements.  For the Name.FirstName property of an instance of PersonModel, the name attribute of the corresponding element is correctly set to "Name.FirstName".  This enables the ModelBinder to correctly bind all values of the submitted form to construct a valid instance of PersonModel.  So far, so good.

However, considering that we went through the trouble of creating separate NameModel and AddressModel classes, it would make sense that we use Partial Views to render editable fields for instances of NameModel and AddressModel.  Our Partial View for the NameModel could look like this:

<script src="https://gist.github.com/3728590.js?file=s4.html"></script>

While our Partial View for our AddressModel would look like this:

<script src="https://gist.github.com/3728590.js?file=s5.html"></script>

We could then modify our original View so it would look like this:

<script src="https://gist.github.com/3728590.js?file=s6.html"></script>

Pretty clean huh? Unfortunately, there's a problem. In the generated HTML, the input fields for Name and Address are (obviously) no longer properly prefixed so the Model Binder would not be able to construct a proper PersonModel instance out of the posted values.  Basically, we'd have a PersonModel instance where the Name and Address properties would point to instances whose properties would be null, even when the user filled in the values.

So, how do we set the prefix correctly within the Partial Views? We obviously can't hardcode the prefix within the Partial Views, because that would limit their usability to usage within parent Views where the Model indeed has a property of the correct type and with the expected name.  We also really want to keep using the TextBoxFor methods in our Partial Views because that's what gives us the DataAnnotations-based client-side validation for free. Ideally, our Partial Views don't know anything about the prefix and we should be able to pass it in from the parent View.  And it would also be nice if we could still use the Partial Views as they are, even when no prefix is required.  After some searching, I found a pretty clean way to do this.

When we call the Html.Partial method in the parent View, we can pass in a ViewDataDictionary instance to the Partial View, which contains a TemplateInfo object, and that TemplateInfo object happens to have an HtmlFieldPrefix property.  It took me a while to find this, but I'm glad I did.  Now I can just change the calls to Html.Partial in the Parent View to this:

<script src="https://gist.github.com/3728590.js?file=s7.html"></script>

And in the generated HTML, the input elements for the NameModel and AddressModel properties will be properly prefixed.  Now, we've already achieved our goal of keeping the Partial Views of having to know anything about a prefix that they may or may not need to include depending on which parent View is using them.  But, as I'm sure you can agree, passing in the ViewDataDictionary with an instance of TemplateInfo with the correct HtmlFieldPrefix is kinda cumbersome, not to mention repetitive and even slightly error-prone.  Ideally, we should be able to change our parent View to this:

<script src="https://gist.github.com/3728590.js?file=s8.html"></script>

It's actually quite easy to do so. First, we'll need the following helper method:

<script src="https://gist.github.com/3728590.js?file=s9.cs"></script>

I know, I know... that code is butt-ugly due to the generics usage but hey, that's C# for ya.  It does allow us to create the EditorForNameModel and EditorForAddressModel methods like this:

<script src="https://gist.github.com/3728590.js?file=s10.cs"></script>

And there we go. Our Partial Views are clean and completely reusable. And using them in a parent View is nice and clean as well. 