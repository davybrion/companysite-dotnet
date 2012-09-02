Suppose we have the following set of classes in an ASP.NET MVC project:

<div>
[csharp]
	public class NameModel
	{
		[Display(Name = &quot;First name&quot;)]
		[Required]
		public string FirstName { get; set; }

		[Display(Name = &quot;Last name&quot;)]
		[Required]
		public string LastName { get; set; }
	}

	public class AddressModel
	{
		[Required]
		public string Street { get; set; }

		[Required]
		public int Number { get; set; }

		[Display(Name = &quot;Zip code&quot;)]
		[Required]
		public int ZipCode { get; set; }

		[Required]
		public string City { get; set; }
	}

	public class PersonModel
	{
		public PersonModel()
		{
			Name = new NameModel();
			Address = new AddressModel();
		}

		public NameModel Name { get; private set; }

		public AddressModel Address { get; private set; }

		[Required]
		[MustBeValidEmailAddress(ErrorMessage = &quot;Not a valid email&quot;)]
		public string Email { get; set; }
	}
[/csharp]
</div>

An input form for an instance of PersonModel could look like this:

<div>
[html]
@using (Html.BeginForm(&quot;MyPostAction&quot;, &quot;MyController&quot;, FormMethod.Post))
{
    @Html.ValidationSummary(true)

    &lt;p&gt;
        @Html.LabelFor(model =&gt; model.Name.FirstName)&lt;br /&gt;
        @Html.TextBoxFor(model =&gt; model.Name.FirstName)
        @Html.ValidationMessageFor(model =&gt; model.Name.FirstName)
    &lt;/p&gt;

    &lt;p&gt;
        @Html.LabelFor(model =&gt; model.Name.LastName)&lt;br /&gt;
        @Html.TextBoxFor(model =&gt; model.Name.LastName)
        @Html.ValidationMessageFor(model =&gt; model.Name.LastName)
    &lt;/p&gt;
        
    &lt;p&gt;
        @Html.LabelFor(model =&gt; model.Address.Street)&lt;br /&gt;
        @Html.TextBoxFor(model =&gt; model.Address.Street)
        @Html.ValidationMessageFor(model =&gt; model.Address.Street)
    &lt;/p&gt;

    &lt;p&gt;
        @Html.LabelFor(model =&gt; model.Address.Number)&lt;br /&gt;
        @Html.TextBoxFor(model =&gt; model.Address.Number)
        @Html.ValidationMessageFor(model =&gt; model.Address.Number)
    &lt;/p&gt;
    
    &lt;p&gt;
        @Html.LabelFor(model =&gt; model.Address.City)&lt;br /&gt;
        @Html.TextBoxFor(model =&gt; model.Address.City)
        @Html.ValidationMessageFor(model =&gt; model.Address.City)
    &lt;/p&gt;

    &lt;p&gt;
        @Html.LabelFor(model =&gt; model.Address.ZipCode)&lt;br /&gt;
        @Html.TextBoxFor(model =&gt; model.Address.ZipCode)
        @Html.ValidationMessageFor(model =&gt; model.Address.ZipCode)
    &lt;/p&gt;
                           
    &lt;p&gt;
        @Html.LabelFor(model =&gt; model.Email)&lt;br /&gt;
        @Html.TextBoxFor(model =&gt; model.Email)
        @Html.ValidationMessageFor(model =&gt; model.Email)
    &lt;/p&gt;
        
    &lt;input type=&quot;submit&quot; value=&quot;Confirm&quot; /&gt;
}
[/html]
</div>

That would produce HTML like this:

<div>
[html]
&lt;label for=&quot;Name_FirstName&quot;&gt;First name&lt;/label&gt;&lt;br /&gt;
        &lt;input data-val=&quot;true&quot; data-val-required=&quot;The First name field is required.&quot; id=&quot;Name_FirstName&quot; name=&quot;Name.FirstName&quot; type=&quot;text&quot; value=&quot;&quot; /&gt;
        &lt;span class=&quot;field-validation-valid&quot; data-valmsg-for=&quot;Name.FirstName&quot; data-valmsg-replace=&quot;true&quot;&gt;&lt;/span&gt;
    &lt;/p&gt;
    &lt;p&gt;
        &lt;label for=&quot;Name_LastName&quot;&gt;Last name&lt;/label&gt;&lt;br /&gt;
        &lt;input data-val=&quot;true&quot; data-val-required=&quot;The Last name field is required.&quot; id=&quot;Name_LastName&quot; name=&quot;Name.LastName&quot; type=&quot;text&quot; value=&quot;&quot; /&gt;
        &lt;span class=&quot;field-validation-valid&quot; data-valmsg-for=&quot;Name.LastName&quot; data-valmsg-replace=&quot;true&quot;&gt;&lt;/span&gt;
    &lt;/p&gt;
    &lt;p&gt;
        &lt;label for=&quot;Address_Street&quot;&gt;Street&lt;/label&gt;&lt;br /&gt;
        &lt;input data-val=&quot;true&quot; data-val-required=&quot;The Street field is required.&quot; id=&quot;Address_Street&quot; name=&quot;Address.Street&quot; type=&quot;text&quot; value=&quot;&quot; /&gt;
        &lt;span class=&quot;field-validation-valid&quot; data-valmsg-for=&quot;Address.Street&quot; data-valmsg-replace=&quot;true&quot;&gt;&lt;/span&gt;
    &lt;/p&gt;
[/html]
</div>

Which is perfect, because this means we get client-side validation for free, based on the DataAnnotations that we used on our model classes.  Also, pay attention to the values of the name attributes for the input elements.  For the Name.FirstName property of an instance of PersonModel, the name attribute of the corresponding element is correctly set to "Name.FirstName".  This enables the ModelBinder to correctly bind all values of the submitted form to construct a valid instance of PersonModel.  So far, so good.

However, considering that we went through the trouble of creating separate NameModel and AddressModel classes, it would make sense that we use Partial Views to render editable fields for instances of NameModel and AddressModel.  Our Partial View for the NameModel could look like this:

<div>
[html]
@model CarShop.Web.Models.NameModel

    &lt;p&gt;
        @Html.LabelFor(model =&gt; model.FirstName)&lt;br /&gt;
        @Html.TextBoxFor(model =&gt; model.FirstName)
        @Html.ValidationMessageFor(model =&gt; model.FirstName)
    &lt;/p&gt;

    &lt;p&gt;
        @Html.LabelFor(model =&gt; model.LastName)&lt;br /&gt;
        @Html.TextBoxFor(model =&gt; model.LastName)
        @Html.ValidationMessageFor(model =&gt; model.LastName)
    &lt;/p&gt;
[/html]
</div>

While our Partial View for our AddressModel would look like this:

<div>
[html]
@model CarShop.Web.Models.AddressModel

    &lt;p&gt;
        @Html.LabelFor(model =&gt; model.Street)&lt;br /&gt;
        @Html.TextBoxFor(model =&gt; model.Street)
        @Html.ValidationMessageFor(model =&gt; model.Street)
    &lt;/p&gt;

    &lt;p&gt;
        @Html.LabelFor(model =&gt; model.Number)&lt;br /&gt;
        @Html.TextBoxFor(model =&gt; model.Number)
        @Html.ValidationMessageFor(model =&gt; model.Number)
    &lt;/p&gt;

    &lt;p&gt;
        @Html.LabelFor(model =&gt; model.ZipCode)&lt;br /&gt;
        @Html.TextBoxFor(model =&gt; model.ZipCode)
        @Html.ValidationMessageFor(model =&gt; model.ZipCode)
    &lt;/p&gt;
    
    &lt;p&gt;
        @Html.LabelFor(model =&gt; model.City)&lt;br /&gt;
        @Html.TextBoxFor(model =&gt; model.City)
        @Html.ValidationMessageFor(model =&gt; model.City)
    &lt;/p&gt;
[/html]
</div>

We could then modify our original View so it would look like this:

<div>
[html]
@using (Html.BeginForm(&quot;MyPostAction&quot;, &quot;MyController&quot;, FormMethod.Post))
{
    @Html.ValidationSummary(true)

    @Html.Partial(&quot;NamePartial&quot;, Model.Name)
    
    @Html.Partial(&quot;AddressPartial&quot;, Model.Address)    
                     
    &lt;p&gt;
        @Html.LabelFor(model =&gt; model.Email)&lt;br /&gt;
        @Html.TextBoxFor(model =&gt; model.Email)
        @Html.ValidationMessageFor(model =&gt; model.Email)
    &lt;/p&gt;
        
    &lt;input type=&quot;submit&quot; value=&quot;Confirm&quot; /&gt;
}
[/html]
</div>

Pretty clean huh? Unfortunately, there's a problem. In the generated HTML, the input fields for Name and Address are (obviously) no longer properly prefixed so the Model Binder would not be able to construct a proper PersonModel instance out of the posted values.  Basically, we'd have a PersonModel instance where the Name and Address properties would point to instances whose properties would be null, even when the user filled in the values.

So, how do we set the prefix correctly within the Partial Views? We obviously can't hardcode the prefix within the Partial Views, because that would limit their usability to usage within parent Views where the Model indeed has a property of the correct type and with the expected name.  We also really want to keep using the TextBoxFor methods in our Partial Views because that's what gives us the DataAnnotations-based client-side validation for free. Ideally, our Partial Views don't know anything about the prefix and we should be able to pass it in from the parent View.  And it would also be nice if we could still use the Partial Views as they are, even when no prefix is required.  After some searching, i found a pretty clean way to do this.

When we call the Html.Partial method in the parent View, we can pass in a ViewDataDictionary instance to the Partial View, which contains a TemplateInfo object, and that TemplateInfo object happens to have an HtmlFieldPrefix property.  It took me a while to find this, but i'm glad i did.  Now i can just change the calls to Html.Partial in the Parent View to this:

<div>
[html]
    @Html.Partial(&quot;NamePartial&quot;, Model.Name, new ViewDataDictionary 
    { 
        TemplateInfo = new System.Web.Mvc.TemplateInfo { HtmlFieldPrefix = &quot;Name&quot; } 
    })
    
    @Html.Partial(&quot;AddressPartial&quot;, Model.Address, new ViewDataDictionary 
    { 
        TemplateInfo = new System.Web.Mvc.TemplateInfo { HtmlFieldPrefix = &quot;Address&quot; } 
    })    
[/html]
</div>

And in the generated HTML, the input elements for the NameModel and AddressModel properties will be properly prefixed.  Now, we've already achieved our goal of keeping the Partial Views of having to know anything about a prefix that they may or may not need to include depending on which parent View is using them.  But, as i'm sure you can agree, passing in the ViewDataDictionary with an instance of TemplateInfo with the correct HtmlFieldPrefix is kinda cumbersome, not to mention repetitive and even slightly error-prone.  Ideally, we should be able to change our parent View to this:

<div>
[html]
    @Html.EditorForNameModel(model =&gt; model.Name);
                                                 
    @Html.EditorForAddressModel(model =&gt; model.Address);
[/html]
</div>

It's actually quite easy to do so. First, we'll need the following helper method:

<div>
[csharp]
		private static MvcHtmlString GetPartial&lt;TRootModel, TModelForPartial&gt;(
			HtmlHelper&lt;TRootModel&gt; helper, string partialName, Expression&lt;Func&lt;TRootModel, TModelForPartial&gt;&gt; getter)
		{
			var prefix = ExpressionHelper.GetExpressionText(getter);

			return helper.Partial(partialName, getter.Compile().Invoke(helper.ViewData.Model),
				new ViewDataDictionary { TemplateInfo = new TemplateInfo { HtmlFieldPrefix = prefix } });
		}
[/csharp]
</div>

I know, i know... that code is butt-ugly due to the generics usage but hey, that's C# for ya.  It does allow us to create the EditorForNameModel and EditorForAddressModel methods like this:

<div>
[csharp]
		public static MvcHtmlString EditorForNameModel&lt;TModel&gt;(
			this HtmlHelper&lt;TModel&gt; helper, Expression&lt;Func&lt;TModel, NameModel&gt;&gt; getter)
		{
			return GetPartial(helper, &quot;NamePartial&quot;, getter);
		}

		public static MvcHtmlString EditorForAddressModel&lt;TModel&gt;(
			this HtmlHelper&lt;TModel&gt; helper, Expression&lt;Func&lt;TModel, AddressModel&gt;&gt; getter)
		{
			return GetPartial(helper, &quot;AddressPartial&quot;, getter);
		}
[/csharp]
</div>

And there we go. Our Partial Views are clean and completely reusable. And using them in a parent View is nice and clean as well. 