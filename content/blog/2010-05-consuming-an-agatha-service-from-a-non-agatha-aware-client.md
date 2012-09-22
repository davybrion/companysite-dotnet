A question that also comes up occasionally is how you can use an Agatha service from a client which isn’t aware of Agatha? Or more specifically: can an Agatha service be used from a client which has generated a proxy based on the WSDL of the Agatha service? The answer is yes!

First of all, make sure your service exposes its metadata. You do this in the usual WCF fashion:

<script src="https://gist.github.com/3693369.js?file=s1.xml"></script>

The serviceMetadata element with the httpGetEnabled=”true” attribute is the important one in the snippet above.</p>  <p>After that, you can simply generate a service proxy through visual studio or svcutil or whatever:

<a href="http://davybrion.com/pictures/ConsumingAnAgathaServiceFromANonAgathaAw_DF1A/add_service_reference.png"><img style="border-right-width: 0px; display: inline; border-top-width: 0px; border-bottom-width: 0px; border-left-width: 0px" title="add_service_reference" border="0" alt="add_service_reference" src="http://davybrion.com/pictures/ConsumingAnAgathaServiceFromANonAgathaAw_DF1A/add_service_reference_thumb.png" width="635" height="510" /></a> </p>  

Now you can write the following code to communicate with your Agatha Service Layer:

<script src="https://gist.github.com/3693369.js?file=s2.cs"></script>

Notice that there are no Agatha-related using statements, nor is there any reference to Agatha or the assembly which contains the Request/Response types. All of the required data can be found within the WSDL and you can generate proxies for it just as you could with any other WCF service. The client-side usage model is of course as bad as it always is with standard WCF (for more information, be sure to read: <a href="/blog/2009/07/why-i-dislike-classic-or-typical-wcf-usage/" target="_blank">Why I Dislike Classic Or Typical WCF Usage</a>) but if you’re willing to put up with it, then at least you can ;)

This also means that you should be able to generate service proxies for other platforms as well, as long as they support SOAP services.