One question that occasionally comes up is how a client can use multiple Agatha services. While it’s not possible with Agatha’s out-of-the-box configuration and usage patterns, it can be done if you’re willing to write a little bit of glue code. So, let’s go through the steps to make it work. I'm gonna use one of our Silverlight applications for this example, but it works just the same for regular .NET clients.

First of all, the Silverlight client’s WCF config has 2 defined endpoints:

<script src="https://gist.github.com/3693352.js?file=s1.xml"></script> 

Notice that each endpoint has a defined name. 

Now you can create 2 new service proxies which inherit from Agatha’s proxy:

<script src="https://gist.github.com/3693352.js?file=s2.cs"></script>

Once you have those proxies, you can create new request dispatchers, which also inherit from Agatha’s:

<script src="https://gist.github.com/3693352.js?file=s3.cs"></script>

If you register those types with your container (make sure it’s the same container instance that Agatha uses) and you can have the correct dispatchers injected for whichever service you want to communicate with. But in the case of asynchronous request dispatchers, you typically use a factory to get them, so you probably want to add something like this:

<script src="https://gist.github.com/3693352.js?file=s4.cs"></script>

And there you go… inject the proper factory (or both of them) and just use whichever one you need to get a dispatcher for the service you want.