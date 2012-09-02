Note: This post is part of a series. You can find the introduction and overview of the series <a href="http://davybrion.com/blog/2010/08/mvp-in-silverlightwpf-series">here</a>.

Coming up with a good sample project for this series wasn't easy.  It had to be small enough to be easy to comprehend and look into, yet it also has to make it clear why i prefer the MVP approach over the MVVM approach, which isn't easy to do when you have a very simple sample.  There has to be business logic, and it has to be encapsulated by a Service Layer.  But i obviously wanted to avoid having to use a database and go through everything to get all of that working while still being easy to download and play around with it.  The Service Layer has been implemented very quickly, and is not representative of a real Service Layer.  It doesn't use a database, it holds its data statically in memory (and doesn't even care about thread-safety of this data either) and i didn't even write tests for any of it.  It's just a simple Service Layer, implemented with a <a href="http://davybrion.com/blog/2009/11/requestresponse-service-layer-series/">Request/Response Service Layer</a>.  It accepts Requests and returns Responses with DTO's (not entities <a href="http://davybrion.com/blog/2010/05/why-you-shouldnt-expose-your-entities-through-your-services/">obviously</a>) to the client.  That's it. 

The client code has been written entirely using Test Driven Development.  Apart from the Views, everything is tested and the tests are obviously also included in the downloadable Visual Studio solution. Some tests were written after a piece of code was written, but most of the tests were written before the actual code was written. I hope you go through the tests to see just how much UI logic you can actually cover quite easily.  I also hope you'll notice that the large majority of tests is very short and focused, which would be harder to achieve when using MVVM.  If you have questions regarding the implementation of the User Controls or their tests, it might be better to hold off on asking them until i've published the posts that cover writing the implementations and the actual tests. You can always ask questions if you want of course, but odds are high that i'm gonna cover the answer to your question in one of the future posts anyway.

One more important thing: the client in this sample is Silverlight, not WPF. You can apply all of these ideas to WPF programming as well obviously.

Now, what exactly is the sample project about? Here's a screenshot:

<a href="http://davybrion.com/blog/wp-content/uploads/2010/08/sample_both_controls.png"><img src="http://davybrion.com/blog/wp-content/uploads/2010/08/sample_both_controls.png" alt="" title="sample_both_controls" width="510" height="437" class="aligncenter size-full wp-image-2438" /></a>

You're probably laughing pretty hard at my lousy UI skills (and rightfully so), but i'm sure you'll agree that the crappy looking UI is not relevant to the topic we're covering in this series ;)

That screenshot shows 2 UserControls.  The implementation of both UserControls will be covered in-depth in the next 2 posts in this series, but for now, i'm just going to tell you what they're supposed to do.  

The first UserControl looks like this:

<a href="http://davybrion.com/blog/wp-content/uploads/2010/08/sample_overview.png"><img src="http://davybrion.com/blog/wp-content/uploads/2010/08/sample_overview.png" alt="" title="sample_overview" width="500" height="344" class="aligncenter size-full wp-image-2440" /></a>

It has a TreeView which shows a UserGroup hierarchy.  When you select a UserGroup, its details must be displayed in the second UserControl where they can be edited.  There's also a button to create a new UserGroup, whose details must also be provided in the second UserControl.  Finally, when a UserGroup has been modified (or deleted) in the second UserControl, the contents of the TreeView must be updated correctly <em>without</em> simply fetching the entire hierarchy again.

The second UserControl looks like this:

<a href="http://davybrion.com/blog/wp-content/uploads/2010/08/sample_details.png"><img src="http://davybrion.com/blog/wp-content/uploads/2010/08/sample_details.png" alt="" title="sample_details" width="503" height="92" class="aligncenter size-full wp-image-2439" /></a>

In this UserControl, you can modify the name of the UserGroup, change its Parent, or just delete the UserGroup.  If you've made changes, but haven't pushed the Save button yet, you can press the Cancel button and the values will be reverted to their original values.  If you make a change here, the TreeView in the first UserControl needs to be updated to reflect that change (either an updated name, or a different parent). 

In both UserControls, some of the actions that the user can perform are either enabled or disabled based on the permissions of the user.  Right now, the permissions can only be changed in the code, but feel free to do so to see how that works. 

Initially, i wanted to add a third UserControl where you can add/remove Users to the selected UserGroup.  While you'll find some traces of this in the Service Layer code, there is nothing in the client to do this.  Feel free to try to implement such a UserControl to see whether or not you like this whole approach with some hands-on coding.  

I hope you'll find the sample to be small enough, but still have enough 'complexity' to show the benefits of using the MVP pattern over MVVM.  Also, keep in mind that this sample is not perfect and that there are still some bugs in it.  If you want to criticize, please focus on problems that are inherent to the usage of MVP instead of MVVM since that's what this is all about.  

You can download the sample <a href="http://davybrion.com/files/SilverlightMVP.zip">here</a>.

In the next 2 posts, we'll cover the implementation of both UserControls in their entirety, and after that we'll focus on automated tests.