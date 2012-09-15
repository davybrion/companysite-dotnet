It's always interesting to know how well your applications perform in production.  To get a better view on this, i recently added a bit of performance related logging to the RequestProcessor class of my <a href="/blog/2008/07/the-request-response-service-layer/">Request/Response service layer</a>.  I have the following 2 loggers set up:

<script src="https://gist.github.com/3685184.js?file=s1.cs"></script>

And here's a simplified version of the Process method:

<script src="https://gist.github.com/3685184.js?file=s2.cs"></script>

The part that is simplified is simply the part that deals with getting the actual responses for each request, including how to deal with failed requests and how to handle subsequent requests in the batch.  It's not relevant to this post and it only clutters the code more so i left that out.  But anyways, the interesting part here is the performance logging.  Ok, the code itself isn't interesting but what you get out of it is pretty nice.  Each single request that takes more than 100 milliseconds is logged.  Also, each batch of requests that takes more than 200 milliseconds is logged.  Both of these 'events' are logged to the performance logger which, in our case, is set up to use a different logfile than the typical error log. 

Which gives us some interesting looking data, like this:

WARN 2009-09-05 08:58:45 - Performance warning: 122ms for GetOpportunityCardHandler <br/>
WARN 2009-09-05 09:00:45 - Performance warning: 159ms for GetCompanyCardHandler <br/>
WARN 2009-09-05 09:01:23 - Performance warning: 187ms for GetOpportunityCardHandler <br/>
WARN 2009-09-05 09:01:32 - Performance warning: 155ms for GetCompanyCardHandler <br/>
WARN 2009-09-05 09:01:41 - Performance warning: 189ms for GetOpportunityCardHandler <br/>
WARN 2009-09-05 09:01:41 - Performance warning: 336ms for the following batch: GetSalesTaskCardRequest, GetContactCardRequest, GetOpportunityCardRequest, GetCompanyCardRequest <br/>

336ms for a single batch of requests is rather slow, and if we see this output regularly in the logfile, we definitely know that these specific request handlers really need to be optimized because we can be pretty sure that is too slow in the real production environment.

It's also useful to identify single request handlers who could use some optimization:

WARN 2009-09-04 02:55:40 - Performance warning: 108ms for PersistIssueHandler <br/>
WARN 2009-09-04 05:54:50 - Performance warning: 148ms for PersistTimesheetEntryHandler <br/>

We've already deployed this for an internal version of one of our applications, which helped us identify some parts that we really needed to optimize before our next external deployment (for our customers).  Even better, when we deploy for our customers, each log entry will also contain the name of the tenant (customer) for which the performance warning was generated.  Which will only make it easier for us to identify real hotspots, based on real usage from our customers, and try to optimize them as soon as possible, preferably before customers start complaining about specific parts.

I'm sure some of you are already doing something similar, but i'm also pretty sure that most of you aren't doing this yet.  I'd definitely recommend doing something like this, because it definitely makes it easier to fix _real_ performance issues instead of the theoretical ones ;)
