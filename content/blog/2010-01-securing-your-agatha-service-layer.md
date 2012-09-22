The question of how to implement security for an Agatha Service Layer is one that comes up frequently. First of all, you need to remember that if you’re using Agatha with WCF you can use any of the WCF features that you’d normally use to secure your WCF service. There’s already plenty of information available online or in books on implementing security for WCF services so I'm not going to spend time on covering those options. What I am going to cover is the approach that we typically use for our Agatha service layers.

I don’t like to tie myself to WCF-specific features, so I always plug in custom authentication into either a custom Request Processor, or a base Request Handler class that all other Request Handlers must inherit from. But first, how do we get the authentication-related data from the client to the service? In each project I use Agatha in, I always have a MyProjectRequest and MyProjectResponse base class:

<script src="https://gist.github.com/3693029.js?file=s1.cs"></script>

Each request in the project inherits from this base request, and each response inherits from the base response. The base response class is often empty, though this does make it very easy if you ever need to send something back with every response. Now obviously, if every single request that is sent to your service layer needs values for the UserName and PasswordHash properties you want this to be done in only one place. I do this by using a custom request dispatcher:

<script src="https://gist.github.com/3693029.js?file=s2.cs"></script>

The IUserContext dependency is just a component that is registered in my IOC container and will be injected automatically whenever you get an instance of IAsyncRequestDispatcher. Now, in this example you can see that I add the authentication data to <em>every request</em> in a batch, even though the batch will be sent in one roundtrip. If you want, you can add the authentication data only to the first request and then only use the first request to do the authentication within your service layer. I prefer to add the authentication data to each request and then authenticate every single request (even subsequent requests in a batch) within the service layer. I'll get back to this point later on.

Now, the only thing we need to do to make sure that your requests will always have the authentication data contained within them is to instruct Agatha to always use instances of your MyProjectAsyncRequestDispatcher class whenever an IAsyncRequestDispatcher is needed. This is easily done during Agatha’s client-side configuration:

<script src="https://gist.github.com/3693029.js?file=s3.cs"></script>

Keep in mind that you still have to register your IUserContext with the container on your own though. 

With that in place, we are sure that each request that comes from <em>our clients</em> always contains the proper authentication data. Now we need to make sure that we actually authenticate these requests within the service layer. You basically have 2 options: either implement your own Request Processor which adds authentication checks, or create a base Request Handler that your other Request Handlers inherit from.

We’ll first cover the option of using a custom Request Processor. You could create one like this:

<script src="https://gist.github.com/3693029.js?file=s4.cs"></script>

The BeforeHandle virtual method is called right before the request is passed to its Request Handler to be handled. Note that this Request Processor would authenticate <em>every</em>request. If you want a Request Processor that only authenticates the first request, you could do so like this:

<script src="https://gist.github.com/3693029.js?file=s5.cs"></script>

The BeforeProcessing virtual method is called before any of the requests are handled, so you could authenticate only the first request in a batch and then proceed with regular processing if the first one is authenticated. Now, the big problem that I have with this approach is that you aren’t really in control of what is sent to your service layer. Yes, you can guarantee that each request coming from <em>your clients</em> contains the proper authentication data. What you simply can’t guarantee however is what other people or custom clients can send to your service layer. If they send you a batch of 2 requests, the first containing valid credentials of a normal user for a benign request, it will pass the authentication just fine. If the second request in that batch contains invalid credentials (to trick your authorization into believing it’s from a user with higher privileges for instance) for a request that could cause some damage (deleting important information or triggering certain tasks or whatever), then you can’t really do anything to prevent that. Unless of course, you reject this approach and authenticate every single request.

As for the MySecurityException type, that’s up to you as well. When you configure your Agatha service layer, you can set the SecurityExceptionType property to the type of exception that should be considered as a security exception. When the Request Processor catches an exception of that type, it will set the ExceptionType property of the response to ExceptionType.Security so you can check for that specific situation in your client. To configure Agatha to use your custom Request Processor, your configuration code would look like this:

<script src="https://gist.github.com/3693029.js?file=s6.cs"></script>

Another alternative is to create a base Request Handler for your project and to have each Request Handler inherit from it. Something like this, for instance:

<script src="https://gist.github.com/3693029.js?file=s7.cs"></script>
  
In case you’re wondering why I'm using Setter-injection here instead of Constructor-injection, read <a href="/blog/2009/11/constructor-injection-vs-sette-injection/" target="_blank">this</a>.

I typically prefer the custom Request Handler approach for authentication. In most applications that we write, authentication is not enough and we need custom authorization checks for many requests. So I'm going to need a base Request Handler which introduces the virtual Authorize method anyway. So I might as well do my authentication check right before it. 

With the custom Request Handler approach, you probably still want to configure Agatha to use your custom security exception type:

<script src="https://gist.github.com/3693029.js?file=s8.cs"></script>

And then you just need to let your own Request Handlers inherit from your MyProjectRequestHandler. Authentication will be performed for each request by default, and you can easily add specific authorization logic for every request. And those are pretty much the options you have to secure your Agatha Service Layer. Oh, and be sure to only expose your Service Layer through SSL, obviously.