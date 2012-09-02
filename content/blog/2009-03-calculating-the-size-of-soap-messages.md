I <a href="http://davybrion.com/blog/2009/03/i-love-easy-extensibility/">recently</a> needed something to log the size of incoming and outgoing SOAP messages, and my first implementation of calculating the size looked like this:

<div>
[csharp]
        private static double GetMessageLengthInKB(Message message)
        {
            return Math.Round(Encoding.UTF8.GetBytes(message.ToString()).Length / 1024d, 2);
        }
[/csharp]
</div>

There is a big problem with this.  Well, at least one that i know of, possibly more.  The ToString() method on the Message class returns the nicely formatted content of the message.  Including all whitespace.  This obviously increases the reported size of the SOAP message by a significant number, even though it's not sent over the wire with all that whitespace. A better way to calculate the size is like this:

<div>
[csharp]
        private static double GetMessageLengthInKB(Message message)
        {
            var writerSettings = new XmlWriterSettings { Encoding = Encoding.UTF8, Indent = false };
 
            using (var memoryStream = new MemoryStream())
            using (var writer = XmlDictionaryWriter.Create(memoryStream, writerSettings))
            {
                message.WriteMessage(writer);
                writer.Flush();
                return Math.Round(memoryStream.Position / 1024d, 2);
            }
        }
[/csharp]
</div>

One thing to keep in mind is that you need to be careful with writing the contents of a SOAP message.  If you write the content of a SOAP message without copying it first, you'll run into other problems further along the WCF pipeline.  So the MessageInspector now looks like this:

<div>
[csharp]
    public class MessageInspector : IDispatchMessageInspector
    {
        private readonly ILog logger = LogManager.GetLogger(typeof(MessageInspector));
 
        public object AfterReceiveRequest(ref Message request, IClientChannel channel, InstanceContext instanceContext)
        {
            if (logger.IsInfoEnabled)
            {
                var bufferedCopy = request.CreateBufferedCopy(int.MaxValue);
 
                var sizeLog = string.Format(&quot;request message size: ~{0} KB&quot;, GetMessageLengthInKB(bufferedCopy.CreateMessage()));
                logger.Info(sizeLog);
 
                request = bufferedCopy.CreateMessage();
            }
 
            return null;
        }
 
        public void BeforeSendReply(ref Message reply, object correlationState)
        {
            if (logger.IsInfoEnabled)
            {
                var bufferedCopy = reply.CreateBufferedCopy(int.MaxValue);
 
                var sizeLog = string.Format(&quot;response message size: ~{0} KB&quot;, GetMessageLengthInKB(bufferedCopy.CreateMessage()));
                logger.Info(sizeLog);
 
                reply = bufferedCopy.CreateMessage();
            }
        }
 
        private static double GetMessageLengthInKB(Message message)
        {
            var writerSettings = new XmlWriterSettings { Encoding = Encoding.UTF8, Indent = false };
 
            using (var memoryStream = new MemoryStream())
            using (var writer = XmlDictionaryWriter.Create(memoryStream, writerSettings))
            {
                message.WriteMessage(writer);
                writer.Flush();
                return Math.Round(memoryStream.Position / 1024d, 2);
            }
        }
    }
[/csharp]
</div>

As you can see, you're better off creating a buffered copy of a message, and then creating a new message out of that copy before doing anything with it.