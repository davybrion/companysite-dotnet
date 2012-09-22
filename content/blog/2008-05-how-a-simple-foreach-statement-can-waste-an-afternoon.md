We all like the foreach statement, right? It's easy to use. It looks good. It does a good job of what it's supposed to do. What's not to like? Well... today I learned it can actually be a great hiding place for performance issues.

I wrote the following code a while ago:

<script src="https://gist.github.com/3612472.js?file=s1.cs"></script>

it's actually a simplified version of the code I wrote, but you get the idea.  It doesn't look so bad, right? It fetches a bunch of groups from an Active Directory store, then it processes the groups and the members of those groups.  It turns out there are actually a few problems with this code.  First of all, when you retrieve a GroupPrincipal, there's no way to make it fetch its Members collection in the same roundtrip (if I'm mistaken, please do correct me). So the Members property of the GroupPrincipal is a lazy-loaded collection. When you access it, it goes back to the Active Directory to fetch all the member Principals.  There's not really anything I can do about that, due to the limitations in how you can retrieve GroupPrincipals (again, unless I'm mistaken). 

So basically, we fetch a bunch of data (the groups) and then when we loop through the retrieved data we fetch more data (the members) for each item in the loop.  So we are making a hell of a lot of roundtrips if we have a lot of groups.  I despise situations like that. And I never do this unless I can't avoid it.  As unfortunate as that is, it's not the real problem that lurks in this code.

If you don't have a lot of groups, then this code works perfectly and the data is processed quickly and the memory is cleaned up pretty soon after we leave ProcessGroupsAndTheirMembers method. Unless you suddenly have to loop through 6000 groups. And almost all of them have at least a few Members, some even have a lot of them. Keep in mind that for each group, we go back to the Active Directory store to retrieve the members.  So that is at least one big query (to retrieve all of the groups) and another 6000 to retrieve all the members.  As if that's not bad enough, the Active Directory store turns out to be pretty slow.  All of a sudden, the code that used to run in a matter of seconds takes 9 minutes.

So you fire up your tools to help you diagnose the problem... the profiler quickly shows that the code spends most of its time in the ProcessGroupsAndTheirMembers method. Process Explorer shows stable cpu usage (low at 25%, but stable... no peaks) and ever-increasing memory usage (all the way up to 400mb).  This is the time where you get that warm and fuzzy "oh fuck..."-feeling.  So you start experimenting with changes, and you test it... each time you test it you basically have to wait 9 minutes if the change didn't have any effect. Joy...

It's actually really simple once you figure it out... each GroupPrincipal object takes up some memory space. If its Members collection is filled up, the GroupPrincipal will hold references to each member Principal in the collection.  The object graph that you are holding in memory basically increases each time you pass through the loop because each GroupPrincipal will hold all of its Members after we've processed it.

But hey, we have garbage collection! It'll clean up the used memory! Yea it does... eventually. Do you know how many garbage collections could occur in a period of 9 minutes? A lot of them actually. Especially if your code is aggressively requesting more and more memory space. 

The problem, of course, is with the foreach statement (duh, I already gave it away in the title). As you can see, we don't really do anything with the GroupPrincipal once we've processed it. Yet it's still kept in the groupPrincipals list, for the duration of the entire loop.  And we can't remove it from the list while we're in the foreach because then the underlying iterator will throw exceptions once we move to the next item. The trick was simply to replace the foreach with a do-while-loop (how old-school!) and to get rid of the GroupPrincipal once it was processed:

<script src="https://gist.github.com/3612472.js?file=s2.cs"></script>

When I ran this code, memory usage remained stable and cpu usage actually went down to 5%.  The time needed to process the groups went from 9 minutes to 5.  Still a lot, but as evidenced by the very low cpu usage, the code is constantly waiting for the data from Active Directory to cross the wire and then it quickly processes it, and then it waits for the next bunch of data.

So, as this story clearly demonstrates, the foreach statement can be quite the evil bitch even though it's usually the nice girl-next-door kinda statement.  It's too bad I wasted a few hours on this... well... honestly, a part of me loves situations like these in a weird, sick and twisted kinda way.  You always learn something very interesting from it :)

Hope you enjoyed this episode of How The Code Turns.
