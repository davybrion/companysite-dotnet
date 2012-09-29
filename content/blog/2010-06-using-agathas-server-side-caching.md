One of the most important new additions in the <a href="http://code.google.com/p/agatha-rrsl/" target="_blank">Agatha</a> 1.1 release is the ability to have the service layer cache responses for requests that are eligible for caching. Obviously, this doesn’t happen automatically and you need to configure this yourself. Unfortunately, I've never really written a post to describe how to do this and what you need to keep in mind. Hopefully, all of this will be clear after this post.

There are only two things you need to do to use Agatha’s server-side caching feature:

## Use the EnableServiceResponseCaching attribute

If you want certain Request-derived types to be eligible for caching, you need to put the EnableServiceResponseCaching attribute on top of them. This attribute enables you to set the logical region (more on that later) where the Response for this Request needs to stored in the cache, and it requires you to set an expiration. Here’s a simple example:

<script src="https://gist.github.com/3693436.js?file=s1.cs"></script>

For this particular request type, responses should be cached for a maximum duration of 10 minutes, and the cached responses will be stored in the Issues region. A region is pretty much just a section within the cache. If you don’t specify a region, each response will be placed in the default region. If you do specify one, each cached response for that region is placed within that section. This gives you the ability to clear an entire region (and thus, all the cached responses that are stored in that region) without impacting any of the other regions (including the default one).

The expiration can be configured by providing a number of hours, minutes or seconds (or a combination of those three) to the attribute.

Now obviously, Agatha needs a way to differentiate between multiple instances of the GetUnassignedIssuesForProjectRequest class. More specifically, Agatha needs to know when a request can be considered equal to a previous request for which a response has already been cached. So that brings us to the next thing you need to do:

## Override the Equals and GetHashCode methods

The response for a GetUnassignedIssuesForProjectRequest instance with ProjectId set to e35c60f7-c35e-43db-9988-0dab3f39c61b will obviously contain a different set of unassigned issues than one for a GetUnassignedIssuesForProjectRequest with ProjectId set to 5d47161a-f334-4cce-9cc4-9606a9d294a6. To make sure that Agatha knows which response can be returned for a given request, your request needs to override the Equals and GetHashCode methods so you can tell Agatha when an instance of a certain request can be considered equal to one for which a cached response already exists. In the case of our example, requests of type GetUnassignedIssuesForProjectRequest can be considered equal if they both return the same value through their ProjectId property. So in this case, our request class needs to look like this:

<script src="https://gist.github.com/3693436.js?file=s2.cs"></script>

Now that we have an Equals and a GetHashCode method which only looks at the value of the ProjectId property, Agatha can differentiate between different requests on a value basis instead of a reference basis. Note that this doesn’t mean that your request types need to be value objects in the truest sense of the term. They just need to be able to perform an equality check based on the values that make the difference between actually handling the request, or returning the response that has previously been cached for the <em>set of values</em> that you’re using to determine equality. Simply overriding the Equals method is not enough, since Agatha will use these instances of requests as keys in a dictionary so you need to provide a proper GetHashCode implementation as well (which is recommended anyway if you’re overriding the Equals method).

It’s very important to really consider which properties you want to include in the equality check and the hashcode calculation. If your request type inherits from some base request (typically one that contains user credentials and stuff like that), then you typically <em>don’t</em> want to include those inherited property values in your equality check, unless you really want to cache different responses based on one of those inherited properties. I'd recommend writing enough tests to verify that your equality checks and hashcode calculations indeed behave the way you <em>want</em> them to because if they don’t, you will either get suboptimal results from Agatha’s caching or even incorrect ones which would lead to bugs that will be very hard to debug. 

A question that came up recently in the <a href="http://groups.google.com/group/agatha-rrsl" target="_blank">Agatha discussion group</a> was how to implement the Equals and GetHashCode method for request types for which each instance should really be considered equal. For instance, a request type like this:

<script src="https://gist.github.com/3693436.js?file=s3.cs"></script>

In this case, the default Equals and GetHashCode implementations will be reference-based and not value based. But as you can see, there are no values to differentiate between requests. In this case, there is only one way to retrieve the known Countries in this system, so how do we implement the Equals and GetHashCode methods so this request type can be used correctly with Agatha’s caching layer? Well, the solution isn’t very nice but it is pretty simple. You can just introduce a dummy field with a fixed value:

<script src="https://gist.github.com/3693436.js?file=s4.cs"></script>

Now, every instance of the GetAllCountriesRequest will be considered equal to each other and they’ll all return the same hashcode as well. So every incoming instance of this request will return the same cached response (once it’s been cached that is).

That’s pretty much it. In itself, the caching layer of Agatha is very easy to use, but you definitely need to make sure that your Equals and GetHashCode implementations are correct. That’s pretty much the only tricky part (and downside) to how Agatha’s caching works, but I was unfortunately unable to come up with something that was easier to use. 

One final word on the usage of regions. If you’re caching responses, then you typically want a way to remove stale responses from the cache. If some of the data that you’re caching is changed before the cached responses expire, you can clear the region in which those cached responses are being stored. Just add an ICacheManager constructor parameter to your handler (or any other class for that matter) and call the Clear method which takes a region name as a parameter.

As always with caching: be careful in how you use it, and make sure you think it through ;)