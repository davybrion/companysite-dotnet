As of Agatha 1.0 beta 2, you can now use whatever logging library you prefer. Agatha now uses the <a href="http://netcommon.sourceforge.net/" target="_blank">Common.Logging</a> project instead of using a logging library directly. This means you just need to add a bit of configuration to your service host to enable logging and get that logging information in whatever format or manner you want. Enabling this is pretty easy, since you just need to follow the standard approaches for <a href="http://netcommon.sourceforge.net/docs/2.0.0/reference/html/ch01.html#logging-config" target="_blank">configuring Common.Logging</a>, and obviously also for the logging library that you use. I’m just going to show a short example of using XML in your web.config (or app.config depending on your service host) to get this working.

First, you need to add the configuration section for Common.Logging to the &lt;configSections&gt; element:  

<script src="https://gist.github.com/3685638.js?file=s1.xml"></script>

As you can see, i also included the definition of my preferred logging library (log4net) in this as well. In this particular case, your service host needs to reference the following assemblies: Common.Logging.dll, Common.Logging.Log4Net.dll and log4net.dll.

The configuration of the Common.Logging library looks like this:

<script src="https://gist.github.com/3685638.js?file=s2.xml"></script>

You need to specify which factoryAdapter that Common.Logging will use, in our case the one for log4net. If you then configure the INLINE value for the configType key, Common.Logging will just initialize log4net in a way that it will simply use the XML configuration that is also present in your config file. Again, i’ll point you to the <a href="http://netcommon.sourceforge.net/docs/2.0.0/reference/html/ch01.html#logging-config" target="_blank">Common.Logging documentation</a> for configuring it for your specific library if you’re using something else. 

In my case, my log4net configuration looks like this:

<script src="https://gist.github.com/3685638.js?file=s3.xml"></script> 

The ‘Agatha’ logger simply logs everything coming from types whose namespace starts with Agatha and that logs at the error level (basically just exceptions caught by the Request Processor) and sends that the LogFileAppender (which simply logs in a log.txt) file. The ‘AgathaPerformance’ logger specifically logs performance warnings coming from Agatha’s PerformanceLoggingRequestProcessor and also sends it to the LogFileAppender.

And that’s all there is to it.