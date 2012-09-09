I <a href="http://davybrion.com/blog/2009/03/i-love-easy-extensibility/">recently</a> needed something to log the size of incoming and outgoing SOAP messages, and my first implementation of calculating the size looked like this:

<script src="https://gist.github.com/3684381.js?file=s1.cs"></script>

There is a big problem with this.  Well, at least one that i know of, possibly more.  The ToString() method on the Message class returns the nicely formatted content of the message.  Including all whitespace.  This obviously increases the reported size of the SOAP message by a significant number, even though it's not sent over the wire with all that whitespace. A better way to calculate the size is like this:

<script src="https://gist.github.com/3684381.js?file=s2.cs"></script>

One thing to keep in mind is that you need to be careful with writing the contents of a SOAP message.  If you write the content of a SOAP message without copying it first, you'll run into other problems further along the WCF pipeline.  So the MessageInspector now looks like this:

<script src="https://gist.github.com/3684381.js?file=s3.cs"></script>

As you can see, you're better off creating a buffered copy of a message, and then creating a new message out of that copy before doing anything with it.