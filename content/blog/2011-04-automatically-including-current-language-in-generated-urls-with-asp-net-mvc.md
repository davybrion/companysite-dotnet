When you're pushing out localized content to your users, you don't want to mess up any possible output caching you've got set up (or would want to set up later on).  One common approach to deal with this is to always include the relevant language code as a route parameter in your URLs.  It works great with output caching because each localized version of the content will be accessible through its own URL, and as an extra benefit, your content is indexable by search engines in every language you support as well.

The only downside to that approach is that you absolutely have to make sure that the correct language code is always included in each URL you put on your pages. That's tedious work at best, error-prone at worst. Ideally, each URL that you generate on your pages automatically has the current language code included in it.  And obviously, you want to be able to provide it explicitly as well (for language selection links for example).  It took me a while to figure out how this can be done with ASP.NET MVC but I did manage to find a pretty nice solution.  

I was browsing the MVC source code (see how useful this whole open source thing is?) to look for some kind of hook I could use to influence how URLs are generated when you use Url.Action or Html.ActionLink in your views.  And it turns out that there is one, though it's not really an obvious one.  Whenever you use Url.Action or Html.ActionLink, ASP.NET MVC calls the GetVirtualPath method for each defined Route object and the first returned VirtualPathData instance is the one that will provide the final URL that is rendered in your links.  So we first need to come up with our own custom Route class:

<script src="https://gist.github.com/3728767.js?file=s1.cs"></script>

Now we have to make sure that we define a route of this type <em>before</em> our normal routes are defined:

<script src="https://gist.github.com/3728767.js?file=s2.cs"></script>

This ensures that our AutoLocalizingRoute instance will get a chance to provide a VirtualPathData instance whenever an action-URL is needed, before the standard Route instance is called to create one.

Now, all we need is something that sets the current thread's Culture and UICulture property based on the language code in the URL of each request.  I did this with a HttpModule, of which only this part is relevant here:

<script src="https://gist.github.com/3728767.js?file=s3.cs"></script>

And that's it.  Every time we generate an URL to a Controller Action, the current language code will be included automagically, so there's no chance of us forgetting it somewhere.