A feature of ASP.NET MVC that i really like is that when you use the LabelFor extension method in a strongly-typed view, the LabelFor implementation will try to retrieve and use metadata for the property you're creating a label for. For instance:

<div>
[csharp]
@Html.LabelFor(m =&gt; m.SomeProperty)
[/csharp]
</div>

This will generate an HTML label for the SomeProperty property of your model. If you need localized views, you can annotate the property in your model like this:

<div>
[csharp]
        [Display(ResourceType = typeof(Resources), Name = &quot;ResourceKeyForSomeProperty&quot;)]
        public string SomeProperty { get; set; }
[/csharp]
</div>

And the label will be generated with a localized value from your application's resources depending on the culture of the current user. Which is great, but putting those Display attributes on each property gets quite tedious.  It would be better if the localized value was automatically retrieved based on a convention. Something like NameOfModelClass_NameOfProperty.

It turns out that ASP.NET MVC uses a DataAnnotationsModelMetaDataProvider by default to retrieve this metadata, and that you can provide a different implementation to be used by the framework.  We still want to take advantage of those DataAnnotations, but we just want to add some convention-based default behavior to it as well.  So we inherited from the DataAnnotationsModelMetaDataProvider and came up with something pretty simple like this:

<div>
[csharp]
    public class AnnotationsAndConventionsBasedModelMetaDataProvider : DataAnnotationsModelMetadataProvider
    {
        protected override ModelMetadata CreateMetadata(IEnumerable&lt;Attribute&gt; attributes, Type containerType,
                                                        Func&lt;object&gt; modelAccessor, Type modelType, string propertyName)
        {
            var result = base.CreateMetadata(attributes, containerType, modelAccessor, modelType, propertyName);

            if (string.IsNullOrEmpty(result.DisplayName) &amp;&amp; containerType != null &amp;&amp; propertyName != null)
            {
                var keyForDisplayValue = string.Format(&quot;{0}_{1}&quot;, containerType.Name, propertyName);
                var translatedValue = Resources.ResourceManager.GetObject(keyForDisplayValue) as string;

                if (!string.IsNullOrEmpty(translatedValue))
                {
                    result.DisplayName = translatedValue;
                }
            }

            return result;
        }
    }
[/csharp]
</div>

We first call the base implementation which will get the values from annotations if they're present. If no DisplayName value was created based on annotations, we're going to check to see if the value is present in our resources based on the convention and if so, add it to the metadata before we return it.  Then we instruct ASP.NET MVC to use this provider instead of the default:

<div>
[csharp]
            ModelMetadataProviders.Current = new AnnotationsAndConventionsBasedModelMetaDataProvider();
[/csharp]
</div>

And now, every label will be localized automatically if a translation is present in the resources with the expected key.  Not sure if this is the best way to do this (better suggestions are welcome!), but it's certainly a big step up from having to annotate each property.