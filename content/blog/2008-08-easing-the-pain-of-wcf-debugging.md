WCF is pretty cool, i guess. It's quite powerful, and it's so configurable it even has options to control the speed of the CPU fans of the users of your service.  Ok, maybe you can't really configure that, but with approximately 12.4 billion WCF configuration settings available to you, who knows? But the biggest problem i have with WCF is the painful debugging experience when something goes wrong.

Ever got a client-side exception that looked like this?

<script src="https://gist.github.com/3676416.js?file=s1.txt"></script>

Rather messy, no? Does it give you any clue as to what could possibly be wrong? Nope. This particular client-side exception occurs when something goes wrong server-side, at some point after you've already returned your return value in your service implementation.  Of course, you don't actually see it happening server-side. So i tried setting IncludeExceptionDetailInFaults to true on the service implementation... didn't make a difference.  I set IncludeExceptionDetailInFaults to true on the service host but that didn't work either. Sigh.

After some googling i discovered that you can enable WCF tracing.  Bingo! Why didn't Juval Lowy's book mention this? It's supposed to be the WCF Bible.... Oh well, thanks to Google we now know how to enable WCF's tracing:

<script src="https://gist.github.com/3676416.js?file=s2.xml"></script>

You can also use the Service Configuration Editor tool which is available in the Windows SDK, but spare yourself the pain of that tool and just copy/paste this xml in your config file.

Now run the service again, and do whatever it was that triggered the weird client-side exception.  After the exception occurred, open the WcfTrace.svclog file with either an editor (it's not very readable though) or with the Microsoft Service Trace Viewer tool (which is not too bad actually).

When i opened my trace output, i immediately saw a red item in the Activity list... so i clicked on it, and i finally saw the problem:

<script src="https://gist.github.com/3676416.js?file=s3.txt"></script>

There we go... the client-side exception was terribly useless, but this is the best kind of exception: not only is it clear about what's wrong, it even gives you the solution to the problem.  Btw, if you get that client-side exception, it doesn't mean that your problem will be the same as the one listed here.  It could literally be anything if it doesn't happen inside of your service implementation.  I've seen the same client-side exception when one type somewhere in the object graph didn't have a DataContract attribute, for instance. 

So anyways, do yourself a favor and enable WCF tracing while you're still in development... it could save you a lot of time.

Oh and bonus points go to whoever points out which of WCF's many configuration settings actually make the real exception appear in the client-side stacktrace :)