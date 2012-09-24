I recently showed how you can <a href="/blog/2010/09/using-nhibernate-to-persist-and-query-ruby-objects/">use NHibernate to persist and query Ruby objects</a> through IronRuby. We've continued the experiment (though we've already done some big optimizations in the code based on the first results of these tests) and we recently had to decide whether or not the performance difference between using NHibernate with regular static C# code and using it with dynamic Ruby objects was acceptable.  So we ran a set of tests, and compared all of the numbers.  Note that we don't claim that these benchmarks are scientifically correct in any way, but we do think they give us a good idea on what we can reasonably expect.  I want to share the results with you, and would appreciate any feedback you guys have on this... particularly on whether or not we missed something obvious in our tests or whether or not we should trust these numbers. After all, we're not professional benchmarkers so our approach might very well just suck :)

We have a scenario which consists of 15 'actions'.  For these actions, we use some tables from the Chinook database, basically just Artist/Album/Track/Genre/MediaType.  The actions are the following:

- Retrieve single track without joins, and access each non-reference property
- Retrieve single track with joins, and access all properties, including references
- Retrieve single track without joins, and access all properties, including references (triggers lazy-loading)
- Create and persist object graph: one artist with two albums with 13 tracks each
- Retrieve created artist from nr 4, add a new album with another 13 tracks, change the title of the first album from nr 4, and remove the second album from nr 4 including its tracks
- Retrieve created artist from nr 4 and delete its entire graph
- Create a single track
- Retrieve single track from step 7 and update it
- Retrieve single track from step 7 and update the name of one of its referenced properties
- Retrieve single track from step 7 and change one of the reference properties so it references a different instance
- Delete the track from step 7
- Retrieve 100 tracks and access each non-reference property
- Retrieve 200 tracks and access each non-reference property
- Retrieve 100 tracks without joins and access all properties, including references (triggers lazy-loading)
- Retrieve 100 tracks with joins and access all properties, including references

Note: when I say we access reference properties to trigger lazy loading, I mean that we access a non-id property of the referenced property to make sure it indeed hits the database.

The scenario is ran 500 times with regular C# objects, and 500 times with Ruby objects.  We keep track of the average time of each action in the scenario, as well as the total duration of the scenario.  Also, keep in mind that we ran these tests on a local database.

The following graph shows the average duration of each action in milliseconds on the Y axis, and the number of the action on the X axis:

<a href="/postcontent/average_duration_of_each_action_in_millis.png"><img src="/postcontent/average_duration_of_each_action_in_millis.png" alt="" title="average_duration_of_each_action_in_millis" width="600" height="406" class="aligncenter size-medium wp-image-2818" /></a>

(you can click on the graph to watch it in its full size)

Before I'll discuss these results, I'd also like to show the following graph which shows the average difference in milliseconds between the static and the dynamic execution of each action:

<a href="/postcontent/average_difference_in_millis_for_each_action.png"><img src="/postcontent/average_difference_in_millis_for_each_action.png" alt="" title="average_difference_in_millis_for_each_action" width="600" height="418" class="aligncenter size-medium wp-image-2821" /></a>

Two actions immediately stand out: the last two which both deal with fetching a set of items and accessing all of their properties.  They're both about 6ms slower than their static counterparts, which is a performance penalty of 71% for action 14, and 87% for action 15.  That deals with a part of code that we can't really optimize any more.  Well, it probably is possible but we've already done a lot of work on that, and this is the best we can come up with so far.

Now, those 2 actions are things we avoid as much as possible in real code anyway, so maybe they aren't that big of an issue.  The other 2 actions where there is a noticable difference (though it actually means an increase in average execution time of 1.1ms using a local database) is the creation and persistance of an object graph (step 4), and the retrieval/modification/persistence of that same graph (step 5).  Most other actions don't have a noticeable difference, and in some cases the dynamic version is actually faster than the static one, no doubt because NHibernate has in some cases less work to do when using the Map EntityMode (which we rely on for the dynamic stuff) compared to the Poco EntityMode.

We also wanted to see whether the performance difference would get worse when spreading the workload evenly over a set of threads, or even a 'pool' of IronRuby engines.  I was pretty happy to see that it didn't really lead to a noticeable difference.

The following graph shows the average duration of the entire scenario in a couple of different situations:

<a href="/postcontent/average_scenario_duration.png"><img src="/postcontent/average_scenario_duration.png" alt="" title="average_scenario_duration" width="462" height="363" class="aligncenter size-full wp-image-2822" /></a>

I do have to mention that the numbers shown in this graph aren't averages, but the result from running the scenario once in each situation.  We did however ran the scenarios in each situation more than once, and while we didn't list the averages, the numbers are representative of each testrun... we didn't see any really noticeable differences over multiple runs.  The percentage difference for each situation is shown in this graph:

<a href="/postcontent/average_scenario_duration_difference.png"><img src="/postcontent/average_scenario_duration_difference.png" alt="" title="average_scenario_duration_difference" width="395" height="365" class="aligncenter size-full wp-image-2825" /></a>

As you can see, the performance penalty of the entire scenario in each situation varies between 15% and 26%.

Now, considering the fact that we prefer to avoid loading 'large' sets of data through NHibernate into entities (we prefer to use projections instead for that) we wanted to see what the difference would be for the entire duration of the scenario in each situation, without the final 4 actions.  Basically, just the typical CRUD scenarios:

<a href="/postcontent/average_scenario_duration_without_last_4_actions.png"><img src="/postcontent/average_scenario_duration_without_last_4_actions.png" alt="" title="average_scenario_duration_without_last_4_actions" width="468" height="365" class="aligncenter size-full wp-image-2826" /></a>

<a href="/postcontent/average_scenario_duration_difference_without_last_4_actions.png"><img src="/postcontent/average_scenario_duration_difference_without_last_4_actions.png" alt="" title="average_scenario_duration_difference_without_last_4_actions" width="405" height="368" class="aligncenter size-full wp-image-2827" /></a>

Now the difference varies between 6% and 15%.

Now, suppose that we have a compelling reason to actually go ahead with using this approach (we do actually, but I'm not gonna get into that here), do you think we can trust these numbers? Is there anything else we're missing? Are we complete idiots for testing the performance difference like this?  Do you have any feedback whatsoever? Then please leave a comment :)