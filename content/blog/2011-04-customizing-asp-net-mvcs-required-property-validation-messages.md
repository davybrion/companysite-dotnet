I recently got <a href="http://davybrion.com/blog/2011/03/convention-based-localization-with-asp-net-mvc/">convention-based localization</a> of display labels working with ASP.NET MVC, and this week, i wanted to get something similar working for required field validation messages.  ASP.NET MVC3 shows a default validation message when a required field is not filled in, unless you specify a resource provider and the name of the resource key when you put the Required attribute on a property.  Just like with the display value of labels, i wanted a convention based approach for this.  I wanted ASP.NET MVC to look for a resource key with the NameOfModelClass_NameOfProperty_required convention.

After some googling and browsing the MVC3 source code, i couldn't really find the hook i needed to make this happen, so i let it rest for a few days and got back to it later on.  I had more luck the second time around and found the hook i needed.  ASP.NET MVC uses the RequiredAttributeAdapter class to retrieve a ModelClientValidationRequiredRule which by default is initialized with the default error message.  The trick was just to inherit from this class and return a ModelClientValidationRequiredRule with your own message, and then register that class with the DataAnnotationsModelValidatorProvider.  This is the new subclass of the RequiredAttributeAdapter class:

<div>
[csharp]
	public class ConventionsBasedRequiredAttributeAdapter : RequiredAttributeAdapter
	{
		public ConventionsBasedRequiredAttributeAdapter(ModelMetadata metadata, ControllerContext context, RequiredAttribute attribute) 
			: base(metadata, context, attribute) {}

		public override IEnumerable&lt;ModelClientValidationRule&gt; GetClientValidationRules()
		{
			string errorMessage;

			var className = Metadata.ContainerType.Name;
			var propertyName = Metadata.PropertyName;

			var specificKey = string.Format(&quot;{0}_{1}_required&quot;, className, propertyName);
			// TODO: make the ResourceManager configurable
			errorMessage = Resources.ResourceManager.GetObject(specificKey) as string;

			if (string.IsNullOrEmpty(errorMessage))
			{
				var genericMessageWithPlaceHolder = (string)Resources.ResourceManager.GetObject(&quot;Generic_required_field_message&quot;);

				if (!string.IsNullOrEmpty(genericMessageWithPlaceHolder))
				{
					errorMessage = string.Format(genericMessageWithPlaceHolder, Metadata.DisplayName);
				}
			}

			if (string.IsNullOrEmpty(errorMessage))
			{
				errorMessage = ErrorMessage; // fallback to what ASP.NET MVC would normally display
			}

			return new[] { new ModelClientValidationRequiredRule(errorMessage) };
		}
	}
[/csharp]
</div>
And this is how you tell ASP.NET MVC to use it:
<div>
[csharp]
			DataAnnotationsModelValidatorProvider.RegisterAdapterFactory(typeof(RequiredAttribute), 
				(metadata, controllerContext, attribute) =&gt; new ConventionsBasedRequiredAttributeAdapter(metadata, 
					controllerContext, (RequiredAttribute)attribute));
[/csharp]
</div>
And that's it... with this approach you have full control over how the message is formatted.