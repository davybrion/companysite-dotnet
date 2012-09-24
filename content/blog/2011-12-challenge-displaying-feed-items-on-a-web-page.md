I'm finally getting around to implementing the website for my company, and there's one small part of it that's quite interesting from an implementation point of view. The website will have a footer on each page which displays links to my 5 most recent blog posts:

<a href="/postcontent/feed_items.png"><img src="/postcontent/feed_items.png" alt="" title="feed_items" width="683" height="402" class="aligncenter size-full wp-image-3823" /></a>

Of course, I don't want to update those links manually whenever I publish a new post, so they need to be retrieved from my blog's RSS feed, which is published by Feedburner. I was hoping to be able to retrieve only the metadata from the posts (date, title and URL is all I need) because my feed always contains the last 20 posts and its total size is usually above 100KB. I haven't found a way to do that, so getting the information I need has to be retrieved through the full feed. Sure, 100KB isn't much but keep in mind that you need to retrieve it and parse it and that I absolutely want to minimize the time each request takes and that I'd rather not see any visual delays on the page either.

I'm interested in hearing how you would implement this. You have total freedom to pick the technologies you'd like to use and no limits on how you'd use them. My only requirements are these:

- It needs to be **fast**
- The fewer requests that are impacted by retrieving the feed data, the better
- If I publish a post, the links on the company website should contain the new link within 30 minutes
- The simpler the solution, the better

My solution can be found <a href="/blog/2011/12/displaying-feed-items-on-a-web-page-my-solution/">here</a>.