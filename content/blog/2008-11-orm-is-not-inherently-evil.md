There were some comments on [a previous post](/blog/2008/11/why-on-earth-would-a-developer-do-this/) about how the problem described in that post is somehow typical for applications that use ORMs.  I really could not disagree more.

It seems that even today, in 2008 mind you, we still have a lot of people who are convinced that ORM usage can never be as efficient as the more classic data access approaches.  Now, i don't even want to get into the whole debate about where business logic belongs (but if you think it belongs in the database you no doubt have better things to do than reading this blog), but one thing that does bother me tremendously is that a lot of people discard ORMs because they simply **don't know how to use it properly**.

An ORM is a tool.  Nothing more, nothing less.  Well, it is a pretty powerful tool and, as with any other powerful tool, improper usage of said tool can really cause a lot of problems.  Should we discard the tool because a lot of people never took the time to figure out how to use it properly?  That would be kinda stupid, no? 

It seems to me that a lot of people seem to have this misconception that using an ORM essentially leads to data-fetching in tremendously inefficient manners.  They see the tutorials where only the 'get-by-id' functionality and the lazy loading features are shown and they somehow think that's all there is to it.  They hear all the horror stories about projects that performed terribly because the developers used an ORM and they blame the tool, not the developers.  Nevermind the fact that there are **plenty** of projects that use more classic data access approaches who perform like shit as well.

So let's try to get a few of these misconceptions out of the way, shall we?

- You <strong>can</strong> create highly efficient queries with an ORM tool, and you can actually do so in a manner which enables <strong>high developer productivity</strong>
- ORM's are not slow by definition.  Using them wrong (just like with any other data access technology) can be tremendously slow however.  Who's at fault?
- ORM's do not use a shitload of memory.  Improper usage of them however can lead to excessive memory usage.  Blame the developer, not the tool.
- ORM's do not lead to lazy developers, who are doing lazy coding by relying on lazy loading.  <strong>Bad developers</strong> lead to lazy coding by relying on lazy loading.

So, for those who think that ORM's can never work 'right', i have only one question: are you absolutely sure you know what you're talking about?