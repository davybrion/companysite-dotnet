I lost some time yesterday trying to get a Silverlight client to use Integrated Security with a WCF service so i figured i'd post the steps necessary to make it work here.

First of all, you need to make sure that your IIS installation has support for Windows Authentication.  Go to Add/Remove Programs (appwiz.cpl), click on Turn Windows Features on or off, select Internet Information Services - World Wide Web Services - Security and make sure that Windows Authentication is checked.

Next, you need to make sure that the virtual directory where you're hosting the WCF service has Windows Authentication enabled.  Open Internet Information Services Manager (inetmgr), select the virtual directory where the WCF service is hosted, click on the Authentication icon and enable Windows Authentication. 

After that, you need to add the following to the binding configuration of the service endpoint (in the host, obviously):

<div>
[xml]
          &lt;security mode=&quot;TransportCredentialOnly&quot;&gt;
            &lt;transport clientCredentialType=&quot;Windows&quot; /&gt;
          &lt;/security&gt;
[/xml]
</div>

I only got it working with basicHttpBinding, so unfortunately i can no longer use the customBinding to use binary XML...

In your Silverlight project, open the ServiceReferences.ClientConfig file and add the following to the binding configuration:

<div>
[xml]
          &lt;security mode=&quot;TransportCredentialOnly&quot; /&gt;
[/xml]
</div>

After that, you should be able to do this in your WCF service:

<div>
[csharp]
            WindowsIdentity myuser = ServiceSecurityContext.Current.WindowsIdentity;
[/csharp]
</div>

And that should return the Windows user of the user running the Silverlight client.

For the record: this is with Silverlight 3... i have no idea if it'll work with Silverlight 2