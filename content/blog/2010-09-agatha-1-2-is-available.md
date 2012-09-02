Agatha 1.2 is now available... keep in mind that this version targets .NET 4 and Silverlight 4 only.  The most important change in this version is by far the <a href="http://davybrion.com/blog/2010/08/using-agathas-client-side-caching/">client-side caching support</a>, but there are some other improvements as well.  These are the changes that were made between 1.1 and 1.2:

<ul>
	<li>Client-side caching (BREAKING CHANGE: EnableResponseCachingAttribute no longer exists, use the EnableServiceResponseCachingAttribute instead) -> this is not supported in the in-process model</li>
	<li>RequestProcessor now has an AfterHandle(request, response) virtual method which is called after the request has been handled by the handler</li>
	<li>ReceivedResponses now has a Responses property which returns all of the retrieved responses</li>
	<li>ExceptionInfo now has a FaultCode property (string) which will be automatically filled in as long as your BusinessException type contains a FaultCode property (thanks to a patch from Huseyin Tufekcilerli)</li>
	<li>Agatha.Spring has been included (thanks to a patch from Jernej Logar)</li>
	<li>Agatha.StructureMap.Container has been fixed so that it instructs StructureMap to use the default constructors of RequestProcessorProxy and AsyncRequestProcessorProxy (thanks to a patch by Bart Deleye)</li>
	<li>Added BeforeResolvingRequestHandler virtual method to the RequestProcessor which gets called right before a RequestHandler is resolved through the container</li>
	<li>Fixed logging of WCF messages where some requests were logged as "... stream ..." (thanks to patch by Bart Deleye)</li>
	<li>Added Agatha.Ninject.Silveright (thanks to patch by Bart Deleye)</li>
	<li>Updated Agatha.Unity and Agatha.Unity.Silverlight to use the 2.0 version of Unity</li>
	<li>Applied patch from Andrew Rea to improve REST support (xml and json)</li>
</ul>

Thanks to everyone who contributed to this release. It's very nice to see the list of contributors growing with each release :)

You can download the source code or the binaries <a href="http://code.google.com/p/agatha-rrsl/downloads/list">here</a>.