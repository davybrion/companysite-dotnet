A couple of days ago I <a href="http://davybrion.com/blog/2011/12/challenge-displaying-feed-items-on-a-web-page/">asked you</a> how you'd implement showing links from an RSS feed on a web page (in this case: my new company web site). These are my requirements for this:

<ul>
	<li>It needs to be <strong>fast</strong></li>
	<li>The fewer requests that are impacted by retrieving the feed data, the better</li>
	<li>If I publish a post, the links on the company website should contain the new link within 30 minutes</li>
	<li>The simpler the solution, the better</li>
</ul>

I came up with a very simple solution, which satisfies these requirements better than any other solution I could think of, or heard of from other people. It is extremely fast, doesn't delay any requests, and doesn't require me to deploy anything but the company website. I'm building the site with <a href="http://expressjs.com/">Express</a> on <a href="http://nodejs.org/">Node.js</a>, which means I can take full advantage of the asynchronous nature of Node.js to implement this.

Let's go over the code... in the script that starts the express server, I have the following code:

<script src="https://gist.github.com/3728853.js?file=s1.js"></script>

I'll discuss the code in just a moment, but first I want to show the view code that renders the links:

<script src="https://gist.github.com/3728853.js?file=s2.html"></script>

And that's all. This is the solution in its entirety!

If you're new to Node, this code probably requires some explanation. Let's start with this part:

<script src="https://gist.github.com/3728853.js?file=s3.js"></script>

Here I'm adding a dynamic helper to the Express application. It basically means that my views have access to the getRecentFeedItems function, which returns the value of the recentFeedItems variable. It's important to know that the getRecentFeedItems function creates a closure on the recentFeedItems variable created above it. That means that if the value of the recentFeedItems variable changes at any point in time, the getRecentFeedItems function will return that new value.

<script src="https://gist.github.com/3728853.js?file=s4.js"></script>

This just creates a function that we can use later on. It retrieves the feed asynchronously, and when the result is retrieved, we parse the feed using the NodePie library and we get the 5 most recent items which we store in the recentFeedItems variable. Again, this creates a closure on the recentFeedItems variable which means that every time we assign a value to this variable, any subsequent call to the getRecentFeedItems function will return the value we just assigned to it because both functions point to the same memory thanks to the magic of closures. Finally, if a callback is provided as a parameter, the callback will be invoked.

<script src="https://gist.github.com/3728853.js?file=s5.js"></script>

The call to setInterval makes sure that the processFeed function is called every 30 minutes. After that, we call the processFeed function manually, and we pass in a callback where we start the Express server. This guarantees that the feed items will be in memory before the server starts processing requests.

What makes this solution so great is that we take full advantage of some of Node's benefits. Whenever we retrieve the RSS feed, Node.JS will retrieve that data asynchronously. As soon as it has fired the request to get the RSS feed, it just goes to the next event in its eventloop so no request is kept waiting while we wait for the data to be downloaded. Until the data from the RSS feed is returned, each request will just use the items that are stored in the recentFeedItems variable. Once the data has been returned, our callback is executed which overwrites the value of the recentFeedItems variable. We don't need to do any locking here because the Node.JS eventloop is single-threaded: while our callback is running, no other code that has access to the recentFeedItems variable can be executed anyway. And the actual parsing of the RSS feed is done by NodePie, which uses <a href="http://expat.sourceforge.net/">expat</a> behind the scenes, which is supposedly the fastest C XML parser available.

Looking back on my initial requirements, I think this solution matches very well.