I recently had to research which UI technology would be the best choice for the applications that my client is going to build in the next couple of years. This is a .NET shop, so there are 2 major directions you could move into: standards-based web development, or Silverlight. When you have to recommend one over the other, you ideally want to be able to back up your choice with more than just some opinions.  So we made a list of candidates and did a POC for each one. Then we came up with a list of criteria, grouped in a bunch of categories. The criteria were all assigned a weight, and we scored each of them for all candidates. 

In this post, I want to go over the categories of criteria, and discuss our findings. I'm also going to share the spreadsheet so you can go through the numbers yourself. Depending on your needs or your opinions, you can change the weights and the scores and see how that affects the outcome. I removed some of the criteria that were specific to my client, but it didn't have a significant impact on the outcome. For this post, I also limited the candidates to ASP.NET MVC 3 in combination with the jQuery family (jQuery Core, jQuery UI and jQuery Mobile) and Silverlight.

Here's a quick listing of the categories and some of their criteria (for the actual list, check the spreadsheet... the link is at the end of the post):

- User experience (compelling UI, accessibility, intuitive/ease-of-use, accessible from multiple devices, accessable from multiple platforms)
- Infrastructure (easy/flexible deployment, monitorability)
- Security (safe from XSS, CSRF)
- Performance (server footprint, client-side resource usage, asynchronicity, UI responsiveness, initial load times)
- Code/Architecture (maturity, reusability of validation logic, simplicity, maintainability, flexibility, power, testability, i18n, feedback cycle, learning curve, potential efficiency, rapid application prototyping, readable URLs, extensibility)
- People (limits the number of required skills, mindshare, documentation, community support, commercial support)
- Strategic (future-proof, standards-compliant, differentiator, backing, vision)
- License (do we have access to the code?)
- Cost
- Tools (IDE support, availability of extra tools, free 3rd party component availability, commercial 3rd party component availability

Depending on what you or your organization requires, some of these might not apply to you. Perhaps there are other criteria that you find important and that we missed. Nevertheless, I think this is a pretty comprehensive list which covers most of the factors that you need to think about when making this kind of decision. 

This graph visualizes how both technologies scored, grouped by category:

<a href="/postcontent/webdev_vs_silverlight.png"><img src="/postcontent/webdev_vs_silverlight.png" alt="" title="webdev_vs_silverlight" width="756" height="500" class="aligncenter size-full wp-image-3143" /></a>

I'm sure there are quite a few things about that image that surprise you. The first thing you might be thinking is "how can Silverlight score so badly when it comes to User Experience?". The answer to that is quite simple: if your users aren't using a desktop/laptop with Windows or OS X on it, there is no experience to be had at all. Users that require assistive technology are out of luck as well since accessibility support in Silverlight is still very poor. If you hold those factors into account, it really doesn't matter much that you can easily make Silverlight applications incredibly flashy (pardon the pun).  Besides, most people get bored and annoyed with excessive animations rather quickly, so you're often better off not to overdo it. With that in mind, jQuery UI and HTML5 will easily meet your needs for that kind of stuff.

Another area where Silverlight scores very poorly is the strategic department. The fact that it's not standards-compliant obviously hurts a lot here, but there's more to it than that. First of all, the mobile story (again) pretty much kills it. Android and iOS don't support it.  We already know it's never going to work on iOS and as long as it doesn't work on iOS, Android has no reason whatsoever to provide support because Silverlight simply isn't important in the grand scheme of things to any of the important players. Microsoft hasn't even announced a Silverlight browser plugin for WP7 yet and who knows if it will? That means that Silverlight web applications aren't usable on <em>any</em> mobile device right now, except for slates running a full Windows OS which looks like its only a tiny portion of the market.  Secondly, despite its original tagline of "Lighting up the web", it appears that Microsoft only has about 3 scenario's in mind where it still actively pushes Silverlight: <em>internal</em> business applications, video streaming and native WP7 development.  While internal business applications are certainly a large part of what we're going to do in the next couple of years, we're also going to build things that are available publicly and to a large variety of people. Going with Silverlight for the internal applications and HTML(5) for the public-facing applications wouldn't be very cost-efficient either since that means we have to train our developers for both cases. And it wouldn't make much sense anyway since HTML(5) is a great fit for internal business apps as well.

But, as you can see, there are areas where Silverlight scores better than ASP.NET MVC 3 with jQuery. For instance, when it comes to Tools, you can't deny the fact that Visual Studio and Blend cover a lot of ground when it comes to the whole Silverlight developer experience.  At the very least, you can mostly stick to your familiar integrated environment, whereas with standards-based web development, you're likely to spend some time in Firebug or Google Chrome's developer tools instead of sticking almost entirely with Visual Studio.  I personally don't mind (at all actually) to use other tools than Visual Studio, but there are quite a few .NET developers who do prefer to stick with Visual Studio.  Which brings me to the People category.  The biggest benefit that Silverlight has over standards-based web development is that you only need to know C# and XAML.  With standards-based web development, you have to know HTML, CSS, JavaScript <em>and</em> the language of your server-side technology, in this case also C#.  This might impact your ability to find new developers so Silverlight does have sort of an advantage there. Though I'd argue that you're better off in the long term with people who are willing to <a href="/blog/2010/09/you-need-to-step-out-of-your-comfort-zone/">step out of their comfort zone instead of clinging to what they know</a>. From a security point of view, Silverlight also scores better because you don't really have to worry about common issues such as XSS, CSRF and other vulnerabilities that are common in web-development. 

So we have 3 categories where Silverlight scores better than ASP.NET MVC3/jQuery but that's far from sufficient to close the gap. Based on the weights we assigned to the criteria, the maximum possible score is 732.  ASP.NET MVC3 with jQuery scored 568.  Silverlight scored 304. Obviously, the results will vary depending on what you find important.  Which is why we asked an analyst from one of those large IT research & advisory companies to give us some feedback on this. The analyst agreed entirely with our findings and our data, and  confirmed that his company is recommending moving towards HTML5 to all of their customers.  He even went as far as to say that Silverlight is hard to recommend, unless you're not targeting any mobile users <em>and</em> the applications are internal-only <em>and</em> you've already invested in the technology.  I can't provide a link for any of this yet, but a paper about this will be published soon so I'll either link to it when it's out (if it's publicly available) or at least reference it. 

I encourage anyone who is faced with the same decision to use the spreadsheet and modify it to your needs (adding more criteria, changing weights and/or scores, whatever) to see which one is the best fit for your situation. You can download the spreadsheet <a href="/postcontent/html_vs_silverlight.xlsx">here</a>.