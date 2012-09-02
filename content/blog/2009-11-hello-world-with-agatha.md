I wrote a small <a href="http://code.google.com/p/agatha-rrsl/source/browse/#svn/trunk/examples">Hello World sample</a> with Agatha to demonstrate how easy it is to use in a new project.  Well, it's not really a small example since it's probably the most over-engineered Hello World app <em>ever</em>.  Doesn't matter though, the objective is to demonstrate how you can get started with Agatha and it shows that pretty well.  Let's go over the steps, shall we?

First of all, you'll need an assembly that contains the types you want to share between your service layer and your clients.  More specifically, this assembly needs to contain the Request/Response types and any other types that are contained in your Requests and Responses.  This assembly needs to reference Agatha.Common.  In my example, this example only contains one file:

<div>
[csharp]
using Agatha.Common;
 
namespace Sample.Common.RequestsAndResponses
{
    public class HelloWorldRequest : Request {}
 
    public class HelloWorldResponse : Response
    {
        public string Message { get; set; }
    }
}
[/csharp]
</div>

Once we have that, we need to implement our service layer.  You basically need an assembly that references both Agatha.Common and Agatha.ServiceLayer.  This assembly also needs to reference your shared assembly.  Then you can start implementing your request handlers.  Since my service layer only has one 'operation', i only have the following Request Handler:

<div>
[csharp]
using Agatha.Common;
using Agatha.ServiceLayer;
using Sample.Common.RequestsAndResponses;
 
namespace Sample.ServiceLayer.Handlers
{
    public class HelloWorldHandler : RequestHandler&lt;HelloWorldRequest, HelloWorldResponse&gt;
    {
        public override Response Handle(HelloWorldRequest request)
        {
            var response = CreateTypedResponse();
            response.Message = &quot;Hello World!&quot;;
            return response;
        }
    }
}
[/csharp]
</div>

I also always include a ComponentRegistration class with a static method to perform all of the initialization.  The only thing that needs to be done to initialize Agatha in your service layer is this:

<div>
[csharp]
using System.Reflection;
using Agatha.ServiceLayer;
using Sample.Common.RequestsAndResponses;
 
namespace Sample.ServiceLayer
{
    public static class ComponentRegistration
    {
        public static void Register()
        {
            new ServiceLayerConfiguration(Assembly.GetExecutingAssembly(), typeof(HelloWorldRequest).Assembly,
                typeof(Agatha.Castle.Container)).Initialize();
        }
    }
}
[/csharp]
</div>

The ServiceLayerConfiguration's constructor requires 3 parameters.  The first is the assembly that contains the Request Handlers, the second the one that contains your Request and Response types.  The third parameter is either a reference to a Type which implement's Agatha's IContainer interface, or an instance of IContainer if you want Agatha to reuse an existing IOC container instance (like, the one the rest of your application is using).  If you pass in a reference to a type which implements IContainer instead of an actual instance, Agatha will create a new container and use that instead.  All of the Request Handlers and Request and Response types found in the passed in assemblies will all be registered within the container automatically so you don't have to worry about that at all.  I'm going to write another detailed post soon to show how you can integrate your favorite IOC container with Agatha, so i'm not going to get into the specifics of this right now.  The only thing you need to remember from this part is how little work it takes to initialize everything.  Oh, you'll also need to reference the assembly which contains the relevant IOC container wrapper for what you want.  In my case, that would be Agatha.Castle but i also have an Agatha.Unity assembly for those who want to use Unity.  Agatha.StructureMap will be added soon.

Alright, you still need to host this service layer somewhere.  In my example, i chose to host it in an ASP.NET Web Application through IIS.  My Web Application project obviously has a reference to my Sample.ServiceLayer assembly.  In the Global.asax.cs file i have the following code:

<div>
[csharp]
using System;
 
namespace Sample.ServiceLayer.Host
{
    public class Global : System.Web.HttpApplication
    {
        protected void Application_Start(object sender, EventArgs e)
        {
            ComponentRegistration.Register();
        }
 
        protected void Session_Start(object sender, EventArgs e) {}
        protected void Application_BeginRequest(object sender, EventArgs e) {}
        protected void Application_AuthenticateRequest(object sender, EventArgs e) {}
        protected void Application_Error(object sender, EventArgs e) {}
        protected void Session_End(object sender, EventArgs e) {}
        protected void Application_End(object sender, EventArgs e) {}
    }
}
[/csharp]
</div>

I also have a Service.svc file which contains the following:

<div>
[xml]
&lt;%@ ServiceHost Language=&quot;C#&quot; Debug=&quot;true&quot; Service=&quot;Agatha.ServiceLayer.WCF.WcfRequestProcessor&quot; %&gt;
[/xml]
</div>

My web.config has the following WCF configuration:

<div>
[xml]
  &lt;system.serviceModel&gt;
    &lt;services&gt;
      &lt;service name=&quot;Agatha.ServiceLayer.WCF.WcfRequestProcessor&quot; behaviorConfiguration=&quot;RequestProcessorBehavior&quot;&gt;
        &lt;endpoint address=&quot;&quot; contract=&quot;Agatha.Common.WCF.IWcfRequestProcessor&quot; binding=&quot;basicHttpBinding&quot;
                  bindingConfiguration=&quot;RequestProcessorBinding&quot;/&gt;
      &lt;/service&gt;
    &lt;/services&gt;
    &lt;bindings&gt;
      &lt;basicHttpBinding&gt;
        &lt;binding name=&quot;RequestProcessorBinding&quot; maxReceivedMessageSize=&quot;2147483647&quot; receiveTimeout=&quot;00:30:00&quot; sendTimeout=&quot;00:30:00&quot;&gt;
          &lt;readerQuotas maxStringContentLength=&quot;2147483647&quot; maxArrayLength=&quot;2147483647&quot;/&gt;
          &lt;security mode=&quot;None&quot; /&gt;
        &lt;/binding&gt;
      &lt;/basicHttpBinding&gt;
    &lt;/bindings&gt;
    &lt;behaviors&gt;
      &lt;serviceBehaviors&gt;
        &lt;behavior name=&quot;RequestProcessorBehavior&quot;&gt;
          &lt;serviceMetadata httpGetEnabled=&quot;true&quot;/&gt;
          &lt;serviceDebug includeExceptionDetailInFaults=&quot;true&quot;/&gt;
          &lt;dataContractSerializer maxItemsInObjectGraph=&quot;2147483647&quot;/&gt;
          &lt;serviceThrottling maxConcurrentCalls=&quot;500&quot; maxConcurrentInstances=&quot;500&quot;/&gt;
        &lt;/behavior&gt;
      &lt;/serviceBehaviors&gt;
    &lt;/behaviors&gt;
  &lt;/system.serviceModel&gt;
[/xml]
</div>

And that is it.  The service layer is now functional and being hosted through IIS.  You can obviously use whatever WCF hosting option you prefer, though the example only uses IIS as a host.  Feel free to contribute other hosts ;)

Then we have a .NET client which is able to call the service layer both synchronously and asynchronously.  In my sample, it's just a console app but it could just as well be another ASP.NET Web Application, a Windows Server, a WPF application, a Winforms application (if you're down with the whole retro thing, that is), or any other .NET process you can think of.

In the case of my sample console app, i need to reference Agatha.Common and Agatha.Castle.  In my app.config file, i need to add the following WCF configuration block:

<div>
[xml]
  &lt;system.serviceModel&gt;
    &lt;bindings&gt;
      &lt;basicHttpBinding&gt;
        &lt;binding name=&quot;RequestProcessorBinding&quot; maxBufferSize=&quot;2147483647&quot; maxReceivedMessageSize=&quot;2147483647&quot;
                 receiveTimeout=&quot;00:30:00&quot; sendTimeout=&quot;00:30:00&quot;&gt;
          &lt;security mode=&quot;None&quot; /&gt;
        &lt;/binding&gt;
      &lt;/basicHttpBinding&gt;
    &lt;/bindings&gt;
    &lt;client&gt;
      &lt;endpoint address=&quot;http://localhost/Sample.ServiceLayer.Host/Service.svc&quot;
            binding=&quot;basicHttpBinding&quot; bindingConfiguration=&quot;RequestProcessorBinding&quot;
            contract=&quot;Agatha.Common.WCF.IWcfRequestProcessor&quot; name=&quot;Agatha_IWcfRequestProcessor&quot; /&gt;
    &lt;/client&gt;
  &lt;/system.serviceModel&gt;
[/xml]
</div>

Before i can call the service, my console app obviously needs to initialize Agatha:

<div>
[csharp]
        private static void InitializeAgatha()
        {
            new ClientConfiguration(typeof(HelloWorldRequest).Assembly, typeof(Agatha.Castle.Container)).Initialize();
        }
[/csharp]
</div>

That should be self-explanatory.

Then i can start making service calls:

<div>
[csharp]
                requestDispatcher = IoC.Container.Resolve&lt;IRequestDispatcher&gt;();
                var response = requestDispatcher.Get&lt;HelloWorldResponse&gt;(new HelloWorldRequest());
                Console.WriteLine(response.Message);
[/csharp]
</div>

And of course, asynchronous usage is also possible:

<div>
[csharp]
        private static void CallTheServiceAsynchronously()
        {
            var requestDispatcher = IoC.Container.Resolve&lt;IAsyncRequestDispatcher&gt;();
            requestDispatcher.Add(new HelloWorldRequest());
            requestDispatcher.ProcessRequests(ResponsesReceived, e =&gt; Console.WriteLine(e.ToString()));
        }
 
        private static void ResponsesReceived(ReceivedResponses receivedResponses)
        {
            Console.WriteLine(receivedResponses.Get&lt;HelloWorldResponse&gt;().Message);
        }
[/csharp]
</div>

In case you're wondering about the usage of IoC.Container in this code: Agatha provides a static IoC class which has an IContainer getter property.  If you configured Agatha to use <em>your</em> container instance, then you can simply resolve the IRequestDispatcher or the IAsyncRequestDispatcher through your usual methods, or preferably, have them injected in your components.  And if you're not into the whole IOC thing, you can just use the RequestDispatcherFactory or the AsyncRequestDispatcherFactory directly.

And that is it.  Yeah, probably the most over-engineered Hello World app ever, but still, i think it shows nicely how <em>easy</em> it is to use Agatha in your projects.

Oh wait, i almost forgot about the Silverlight sample... In your Silverlight application, reference Agatha.Common.Silverlight, and add the following ServiceReferences.ClientConfig file:

<div>
[xml]
&lt;configuration&gt;
  &lt;system.serviceModel&gt;
    &lt;bindings&gt;
      &lt;basicHttpBinding&gt;
        &lt;binding name=&quot;RequestProcessorBinding&quot; maxBufferSize=&quot;2147483647&quot; maxReceivedMessageSize=&quot;2147483647&quot;
                 receiveTimeout=&quot;00:30:00&quot; sendTimeout=&quot;00:30:00&quot;&gt;
          &lt;security mode=&quot;None&quot; /&gt;
        &lt;/binding&gt;
      &lt;/basicHttpBinding&gt;
    &lt;/bindings&gt;
    &lt;client&gt;
      &lt;endpoint binding=&quot;basicHttpBinding&quot; bindingConfiguration=&quot;RequestProcessorBinding&quot;
            contract=&quot;Agatha.Common.WCF.IWcfRequestProcessor&quot;
            name=&quot;Agatha_IWcfRequestProcessor&quot;
            address=&quot;http://localhost/Sample.ServiceLayer.Host/Service.svc&quot; /&gt;
    &lt;/client&gt;
  &lt;/system.serviceModel&gt;
&lt;/configuration&gt;
[/xml]
</div>

provide a way to initialize Agatha in your Silverlight app like this:

<div>
[csharp]
using Agatha.Common;
using Sample.Common.RequestsAndResponses;
 
namespace Sample.SilverlightClient
{
    public static class ComponentRegistration
    {
        public static void Register()
        {
            new ClientConfiguration(typeof(HelloWorldRequest).Assembly, typeof(Agatha.Unity.Container)).Initialize();
        }
    }
}
[/csharp]
</div>

Make sure the ComponentRegistration.Register method is called when the silverlight client starts, and then you can do something like this:

<div>
[csharp]
            var requestDispatcher = IoC.Container.Resolve&lt;IAsyncRequestDispatcher&gt;();
            requestDispatcher.Add(new HelloWorldRequest());
            requestDispatcher.ProcessRequests(responses =&gt; MyTextBlock.Text = responses.Get&lt;HelloWorldResponse&gt;().Message,
                exception =&gt; { throw new Exception(exception.ToString()); });
[/csharp]
</div>

Now you tell me, is that easy or what? :)