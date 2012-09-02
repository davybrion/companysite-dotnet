Welcome to episode 245 in my Love/Hate relationship with WCF.  Today i had to add some technical logging to some infrastructure code.  More specifically, we wanted to log the size of incoming and outgoing SOAP messages.  If you're using something like <a href="http://davybrion.com/blog/2008/07/the-request-response-service-layer/">the Request/Response service layer</a> you do want to keep an eye on the size of those SOAP messages to make sure nobody is going overboard with the WCF batching.

I obviously already had my service, so i just needed something that i could plug in at the appropriate moment to record the size of incoming and outgoing SOAP messages.  Turns out this was extremely easy to do.  First, you need to write an inspector for the messages (which has to implement WCF's IDispatchMessageInspector interface):

<code>
<div style="font-family: Consolas; font-size: 9pt; color: black; background: white;">
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; <span style="color: blue;">public</span> <span style="color: blue;">class</span> <span style="color: #2b91af;">MessageInspector</span> : <span style="color: #2b91af;">IDispatchMessageInspector</span></p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; {</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span style="color: blue;">private</span> <span style="color: blue;">readonly</span> <span style="color: #2b91af;">ILog</span> logger = <span style="color: #2b91af;">LogManager</span>.GetLogger(<span style="color: blue;">typeof</span>(<span style="color: #2b91af;">MessageInspector</span>));</p>
<p style="margin: 0px;">&nbsp;</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span style="color: blue;">public</span> <span style="color: blue;">object</span> AfterReceiveRequest(<span style="color: blue;">ref</span> <span style="color: #2b91af;">Message</span> request, <span style="color: #2b91af;">IClientChannel</span> channel, <span style="color: #2b91af;">InstanceContext</span> instanceContext)</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; {</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span style="color: blue;">if</span> (logger.IsInfoEnabled)</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; {</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; logger.Info(<span style="color: blue;">string</span>.Format(<span style="color: #a31515;">&quot;request message size: ~{0} KB&quot;</span>, GetMessageLengthInKB(request)));</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; }</p>
<p style="margin: 0px;">&nbsp;</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span style="color: blue;">return</span> <span style="color: blue;">null</span>; </p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; }</p>
<p style="margin: 0px;">&nbsp;</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span style="color: blue;">public</span> <span style="color: blue;">void</span> BeforeSendReply(<span style="color: blue;">ref</span> <span style="color: #2b91af;">Message</span> reply, <span style="color: blue;">object</span> correlationState)</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; {</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span style="color: blue;">if</span> (logger.IsInfoEnabled)</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; {</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; logger.Info(<span style="color: blue;">string</span>.Format(<span style="color: #a31515;">&quot;response message size: ~{0} KB&quot;</span>, GetMessageLengthInKB(reply)));</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; }</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; }</p>
<p style="margin: 0px;">&nbsp;</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span style="color: blue;">private</span> <span style="color: blue;">static</span> <span style="color: blue;">double</span> GetMessageLengthInKB(<span style="color: #2b91af;">Message</span> message)</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; {</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span style="color: blue;">return</span> <span style="color: #2b91af;">Math</span>.Round(<span style="color: #2b91af;">Encoding</span>.UTF8.GetBytes(message.ToString()).Length / 1024d, 2);</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; }</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; }</p>
</div>
</code>

After that, you need a way to inject the MessageInspector into the behavior of your service.  So you need to define your own behavior first:

<code>
<div style="font-family: Consolas; font-size: 9pt; color: black; background: white;">
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; <span style="color: blue;">public</span> <span style="color: blue;">class</span> <span style="color: #2b91af;">AddMessageInspectorBehaviorAttribute</span> : <span style="color: #2b91af;">Attribute</span>, <span style="color: #2b91af;">IServiceBehavior</span></p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; {</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span style="color: blue;">public</span> <span style="color: blue;">void</span> Validate(<span style="color: #2b91af;">ServiceDescription</span> serviceDescription, <span style="color: #2b91af;">ServiceHostBase</span> serviceHostBase) {}</p>
<p style="margin: 0px;">&nbsp;</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span style="color: blue;">public</span> <span style="color: blue;">void</span> AddBindingParameters(<span style="color: #2b91af;">ServiceDescription</span> serviceDescription, <span style="color: #2b91af;">ServiceHostBase</span> serviceHostBase, </p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span style="color: #2b91af;">Collection</span>&lt;<span style="color: #2b91af;">ServiceEndpoint</span>&gt; endpoints, <span style="color: #2b91af;">BindingParameterCollection</span> bindingParameters) {}</p>
<p style="margin: 0px;">&nbsp;</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span style="color: blue;">public</span> <span style="color: blue;">void</span> ApplyDispatchBehavior(<span style="color: #2b91af;">ServiceDescription</span> serviceDescription, <span style="color: #2b91af;">ServiceHostBase</span> serviceHostBase)</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; {</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span style="color: blue;">foreach</span> (<span style="color: #2b91af;">ChannelDispatcher</span> dispatcher <span style="color: blue;">in</span> serviceHostBase.ChannelDispatchers)</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; {</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span style="color: blue;">foreach</span> (<span style="color: blue;">var</span> endpoint <span style="color: blue;">in</span> dispatcher.Endpoints)</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; {</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; endpoint.DispatchRuntime.MessageInspectors.Add(<span style="color: blue;">new</span> <span style="color: #2b91af;">MessageInspector</span>());</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; }</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; }</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; }</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; }</p>
</div>
</code>

And then you apply that to your service:

<code>
<div style="font-family: Consolas; font-size: 9pt; color: black; background: white;">
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; [<span style="color: #2b91af;">ServiceBehavior</span>(InstanceContextMode = <span style="color: #2b91af;">InstanceContextMode</span>.PerCall)]</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; [<span style="color: #2b91af;">AddMessageInspectorBehavior</span>]</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; <span style="color: blue;">public</span> <span style="color: blue;">class</span> <span style="color: #2b91af;">WcfRequestProcessor</span> : <span style="color: #2b91af;">IRequestProcessor</span></p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; {</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span style="color: green;">// the service stuff...</span></p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; }</p>
</div>
</code>

And that was it! I was afraid i was going to have to figure out which one of WCF's 12.4 billion configuration options would make this possible but this actually didn't require any configuration at all.  Which is very nice, IMO.  

On a side note, if anyone knows of a better way to calculate the real size of a SOAP message, please let me know :)