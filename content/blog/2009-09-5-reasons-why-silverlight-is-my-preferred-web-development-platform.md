<strong>NOTE: if you landed on this post while doing research as to whether or not Silverlight is a technology worth learning or using, please read the following posts as well, which i wrote about a year after writing the one below: <a href="http://davybrion.com/blog/2010/09/keep-your-eyes-on-the-road/">Keep Your Eyes On The Road</a> and <a href="http://davybrion.com/blog/2011/01/learn-to-work-with-the-web-instead-of-against-it/">Learn To Work With The Web Instead Of Against It</a>. Simply put, Silverlight is no longer my preferred web development platform for a variety of reasons i mention in those two posts.</strong>

The company where i work has been using Silverlight ever since the first version came out, and by now we have quite a bit of experience and dare i say expertise in it.  We generally suggest using Silverlight for every new project that we do, and the only time we don't use it is when customers don't want us to use it.  This happens very rarely though.  

While i generally don't get involved in most of the UI stuff, i'm pretty happy with this because i seriously like Silverlight as a platform.  Below is my list of top 5 reasons why Silverlight is my preferred platform for Web Development nowadays:

<ol>
	<li><p><strong>The ability to create an excellent UIX which definitely matters to customers.</strong></p>

<p>
We have some projects at work where the Silverlight UI definitely receives a "wow" response from customers.  Not only because the UI is extremely responsive and snappy, but also because we can use new ways of visualizing data and enabling user actions in the UI in ways that are either completely new for web applications, or just new in general.  We can easily create much more intuitive user interfaces with a responsiveness that can easily compete with that of desktop applications.
</p>
</li>

	<li><p><strong>Low bandwidth overhead</strong></p>

<p>
Downloading the XAP file can sometimes be painfully slow, depending on your bandwidth obviously, but once it's downloaded it's also cached by the user's browser.  Everything that happens after that consumes very little bandwidth.  There is no CSS, no HTML markup, no javascript, ... the only thing that goes over the wire is the data that you actually need.  And with Silverlight's Binary XML feature you're also using less bandwidth than before.  This might not seem like a big deal to you, but bandwidth has a huge impact on the client-side responsiveness as well as your server-side ability to process requests.
</p>
</li>


	<li><p><strong>The client is stateful again</strong></p>

<p>
This one is huge to me.  We don't have to deal with things like ViewState or SessionState or anything like that.  If a client has retrieved a set of data, it can just keep it in memory if it makes sense to do so.  This is especially useful for things like user-profiles, or static data that never changes but it has an impact on pretty much everything you develop.  You can simply keep whatever data you need in memory, and it has no impact on the server at all.  Well, it actually reduces the amount of data that you need to send to the server (or receive), and the number of requests you need the server to handle to implement the functionality you need client-side.  This is pretty much a win-win for both the client and the server whereas with typical ASP.NET development, either the client or the server has to take some kind of a hit when it comes to maintaining state (even if it's only a little).  
</p>

</li>

	<li><p><strong>The ability to write the client almost entirely in C#</strong></p>

<p>
I don't know about you, but my HTML, CSS and Javascript skills are extremely limited.  This seriously reduces my effectiveness when i need to make changes in the presentation layer in typical ASP.NET applications.  And while my XAML skills aren't what they should be, my C# skills are sufficient for me to be effective in client-side changes.  I can easily look at a piece of client-side code and make whatever change i need to make without having to consult a reference guide.  That's not the biggest advantage to being able to write the client in C# though.  Existing C# skills are very easily transferable to the Silverlight side of things, which means that a larger pool of developers is available to work on your front-end.  True, there is a bit of a learning curve when it comes to Silverlight, but i'd argue that there is a much steeper learning curve to doing traditional client-side web development <strong>effectively</strong>.
</p>

</li>

	<li><p><strong>It's only going to get better</strong></p>

<p>
As much as i like Silverlight, it is by no means perfect and there is plenty of room for improvement.  That's normal though for such a young platform.  In the future we'll see plenty of new libraries and tools that will become available to Silverlight developers which should enable us to create even better web applications.  Microsoft is investing a lot of money and effort into it, and i'm pretty sure that others will follow.
</p>

</li>
</ol>

Now, you won't hear me say that Silverlight is the perfect solution for every web application.  SEO is still a problem, as well as acceptance of the Silverlight plugin in corporate environments (though this will only improve over time).  But, in the situations where you can use Silverlight, it certainly pays off to do so.

