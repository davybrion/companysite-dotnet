Welcome to episode 245 in my Love/Hate relationship with WCF.  Today i had to add some technical logging to some infrastructure code.  More specifically, we wanted to log the size of incoming and outgoing SOAP messages.  If you're using something like <a href="http://davybrion.com/blog/2008/07/the-request-response-service-layer/">the Request/Response service layer</a> you do want to keep an eye on the size of those SOAP messages to make sure nobody is going overboard with the WCF batching.

I obviously already had my service, so i just needed something that i could plug in at the appropriate moment to record the size of incoming and outgoing SOAP messages.  Turns out this was extremely easy to do.  First, you need to write an inspector for the messages (which has to implement WCF's IDispatchMessageInspector interface):

<script src="https://gist.github.com/3684415.js?file=s1.cs"></script>

After that, you need a way to inject the MessageInspector into the behavior of your service.  So you need to define your own behavior first:

<script src="https://gist.github.com/3684415.js?file=s2.cs"></script>

And then you apply that to your service:

<script src="https://gist.github.com/3684415.js?file=s3.cs"></script>

And that was it! I was afraid i was going to have to figure out which one of WCF's 12.4 billion configuration options would make this possible but this actually didn't require any configuration at all.  Which is very nice, IMO.  

On a side note, if anyone knows of a better way to calculate the real size of a SOAP message, please let me know :)