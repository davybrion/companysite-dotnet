WCF is pretty cool, i guess. It's quite powerful, and it's so configurable it even has options to control the speed of the CPU fans of the users of your service.  Ok, maybe you can't really configure that, but with approximately 12.4 billion WCF configuration settings available to you, who knows? But the biggest problem i have with WCF is the painful debugging experience when something goes wrong.

Ever got a client-side exception that looked like this?

<div>
[code]
[SocketException (0x2746): An existing connection was forcibly closed by the remote host]
   System.Net.Sockets.Socket.Receive(Byte[] buffer, Int32 offset, Int32 size, SocketFlags socketFlags) +73
   System.ServiceModel.Channels.SocketConnection.ReadCore(Byte[] buffer, Int32 offset, Int32 size, TimeSpan timeout, Boolean closing) +110

[CommunicationException: The socket connection was aborted. This could be caused by an error processing your message or a receive timeout being exceeded by the remote host, or an underlying network resource issue. Local socket timeout was '00:29:59.8590000'.]
   System.ServiceModel.Channels.SocketConnection.ReadCore(Byte[] buffer, Int32 offset, Int32 size, TimeSpan timeout, Boolean closing) +183
   System.ServiceModel.Channels.SocketConnection.Read(Byte[] buffer, Int32 offset, Int32 size, TimeSpan timeout) +54
   System.ServiceModel.Channels.DelegatingConnection.Read(Byte[] buffer, Int32 offset, Int32 size, TimeSpan timeout) +32
   System.ServiceModel.Channels.ConnectionStream.Read(Byte[] buffer, Int32 offset, Int32 count, TimeSpan timeout) +32
   System.ServiceModel.Channels.ConnectionStream.Read(Byte[] buffer, Int32 offset, Int32 count) +53
   System.Net.FixedSizeReader.ReadPacket(Byte[] buffer, Int32 offset, Int32 count) +37
   System.Net.Security.NegotiateStream.StartFrameHeader(Byte[] buffer, Int32 offset, Int32 count, AsyncProtocolRequest asyncRequest) +131
   System.Net.Security.NegotiateStream.StartReading(Byte[] buffer, Int32 offset, Int32 count, AsyncProtocolRequest asyncRequest) +28
   System.Net.Security.NegotiateStream.ProcessRead(Byte[] buffer, Int32 offset, Int32 count, AsyncProtocolRequest asyncRequest) +223

[IOException: The read operation failed, see inner exception.]
   System.Net.Security.NegotiateStream.ProcessRead(Byte[] buffer, Int32 offset, Int32 count, AsyncProtocolRequest asyncRequest) +333
   System.Net.Security.NegotiateStream.Read(Byte[] buffer, Int32 offset, Int32 count) +79
   System.ServiceModel.Channels.StreamConnection.Read(Byte[] buffer, Int32 offset, Int32 size, TimeSpan timeout) +72

[CommunicationException: The socket connection was aborted. This could be caused by an error processing your message or a receive timeout being exceeded by the remote host, or an underlying network resource issue. Local socket timeout was '00:29:59.8590000'.]
   System.Runtime.Remoting.Proxies.RealProxy.HandleReturnMessage(IMessage reqMsg, IMessage retMsg) +7594687
   System.Runtime.Remoting.Proxies.RealProxy.PrivateInvoke(MessageData&amp; msgData, Int32 type) +275
[/code]
</div>

Rather messy, no? Does it give you any clue as to what could possibly be wrong? Nope. This particular client-side exception occurs when something goes wrong server-side, at some point after you've already returned your return value in your service implementation.  Of course, you don't actually see it happening server-side. So i tried setting IncludeExceptionDetailInFaults to true on the service implementation... didn't make a difference.  I set IncludeExceptionDetailInFaults to true on the service host but that didn't work either. Sigh.

After some Live Searching (i was actually googling, but let's make Microsoft think at least someone outside of Redmond uses Live Search) i discovered that you can enable WCF tracing.  Bingo! Why didn't Juval Lowy's book mention this? It's supposed to be the WCF Bible.... Oh well, thanks to Google, i mean Live Search, we now know how to enable WCF's tracing:

<div>
[xml]
  &lt;system.diagnostics&gt;
    &lt;trace autoflush=&quot;true&quot; /&gt;
    &lt;sources&gt;
      &lt;source name=&quot;System.ServiceModel&quot;
              switchValue=&quot;Information, ActivityTracing&quot;
              propagateActivity=&quot;true&quot;&gt;
        &lt;listeners&gt;
          &lt;add name=&quot;wcfTraceListener&quot; type=&quot;System.Diagnostics.XmlWriterTraceListener&quot; initializeData=&quot;WcfTrace.svclog&quot; /&gt;
        &lt;/listeners&gt;
      &lt;/source&gt;
    &lt;/sources&gt;
  &lt;/system.diagnostics&gt;
[/xml]
</div>

You can also use the Service Configuration Editor tool which is available in the Windows SDK, but spare yourself the pain of that tool and just copy/paste this xml in your config file.

Now run the service again, and do whatever it was that triggered the weird client-side exception.  After the exception occurred, open the WcfTrace.svclog file with either an editor (it's not very readable though) or with the Microsoft Service Trace Viewer tool (which is not too bad actually).

When i opened my trace output, i immediately saw a red item in the Activity list... so i clicked on it, and i finally saw the problem:

There was an error while trying to serialize parameter http://tempuri.org/:ProcessReadOnlyRequestsResult. The InnerException message was 'Maximum number of items that can be serialized or deserialized in an object graph is '65536'. Change the object graph or increase the MaxItemsInObjectGraph quota. '.  Please see InnerException for more details.

There we go... the client-side exception was terribly useless, but this is the best kind of exception: not only is it clear about what's wrong, it even gives you the solution to the problem.  Btw, if you get that client-side exception, it doesn't mean that your problem will be the same as the one listed here.  It could literally be anything if it doesn't happen inside of your service implementation.  I've seen the same client-side exception when one type somewhere in the object graph didn't have a DataContract attribute, for instance. 

So anyways, do yourself a favor and enable WCF tracing while you're still in development... it could save you a lot of time.

Oh and bonus points go to whoever points out which of WCF's many configuration settings actually make the real exception appear in the client-side stacktrace :)