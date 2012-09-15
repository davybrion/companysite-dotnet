I recently got <a href="/blog/2011/03/convention-based-localization-with-asp-net-mvc/">convention-based localization</a> of display labels working with ASP.NET MVC, and this week, i wanted to get something similar working for required field validation messages.  ASP.NET MVC3 shows a default validation message when a required field is not filled in, unless you specify a resource provider and the name of the resource key when you put the Required attribute on a property.  Just like with the display value of labels, i wanted a convention based approach for this.  I wanted ASP.NET MVC to look for a resource key with the NameOfModelClass_NameOfProperty_required convention.

After some googling and browsing the MVC3 source code, i couldn't really find the hook i needed to make this happen, so i let it rest for a few days and got back to it later on.  I had more luck the second time around and found the hook i needed.  ASP.NET MVC uses the RequiredAttributeAdapter class to retrieve a ModelClientValidationRequiredRule which by default is initialized with the default error message.  The trick was just to inherit from this class and return a ModelClientValidationRequiredRule with your own message, and then register that class with the DataAnnotationsModelValidatorProvider.  This is the new subclass of the RequiredAttributeAdapter class:

<script src="https://gist.github.com/3728776.js?file=s1.cs"></script>

And this is how you tell ASP.NET MVC to use it:

<script src="https://gist.github.com/3728776.js?file=s2.cs"></script>

And that's it... with this approach you have full control over how the message is formatted.