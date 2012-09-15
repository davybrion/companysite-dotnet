I'm currently doing a proof of concept with <a href="http://www.telerik.com/products/aspnet-mvc.aspx">Telerik's ASP.NET MVC Extensions</a> and ran into something that i find rather troubling. I was trying to get the grid to work with custom binding, so i can take care of paging/sorting serverside for AJAX requests.  It was pretty easy to set up, but for some reason my controller method which is invoked for the AJAX requests wasn't being passed the relevant sorting information.

The method definition looks like this:

<script src="https://gist.github.com/3728615.js?file=s1.cs"></script>

When i go to a different page in the grid, the gridCommand instance contains the relevant paging information (pagesize, current page).  The GridCommand type also exposes a SortDescriptors collection which contains all the information you'd need to correctly sort the data you're supposed to return.  Except that in my case, the SortDescriptors collection was always empty. I went through the documentation and looked over everything to make sure that i wasn't doing anything stupid, which is always my first assumption.  Unfortunately, everything looked alright. I googled and didn't really find anything, until i found a thread on the Telerik support forum where someone else mentioned the exact same problem.  Unfortunately, the thread didn't get an answer so that didn't offer any help. 

Then i turned to Reflector to check out the code of Telerik's GridAction attribute. I noticed the following code in the constructor of the argument:

<script src="https://gist.github.com/3728615.js?file=s2.cs"></script>

Cue the "you've gotta be shitting me" response.  I went back to my controller method and changed it to this:

<script src="https://gist.github.com/3728615.js?file=s3.cs"></script>

And suddenly, it worked. 

I can't believe i ran into something like this, <em>in the year 2011</em>.  This isn't exactly a good first impression that Telerik is leaving on me, and i can only hope that this proof of concept is not going to turn into a Daily WTF discovery.