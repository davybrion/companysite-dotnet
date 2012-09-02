Note: This post is part of a series. Be sure to read the introduction <a href="http://davybrion.com/blog/2009/11/requestresponse-service-layer-series/">here</a>.

In a typical WCF service, you'll have one or more Service Contracts, and each Service Contract will define one or more Service Operations.  These operations are just methods that you can call on the service.  An operation can have parameters and it can have a return value.  Most people typically expose multiple operations on their services for each piece of functionality they want their service to offer to consumers.

The Request/Response Service Layer (RRSL) takes a different approach.  It has one service contract, with only one operation defined:

<div>
[csharp]
    public interface IRequestProcessor : IDisposable
    {
        Response[] Process(params Request[] requests);
    }
[/csharp]
</div>

Notice the lack of the typical WCF attributes such as ServiceContract and OperationContract.  The RRSL is actually completely independent from WCF and only uses WCF as a transport medium.  That's the topic of another post though, so let's get back to the topic at hand: requests and responses.

As you can see, the Process method receives an array of Request objects and returns an array of Response objects.  So what exactly is a Request and what is a Response?  This is what they look like:

<div>
[csharp]
    public class Request
    {
    }
 
    public class Response
    {
        public ExceptionInfo Exception { get; set; }
        public ExceptionType ExceptionType { get; set; }
    }
[/csharp]
</div>

Both are classes that you should derive specific Request and Response types from.  The Response class automatically comes with an ExceptionInfo class (which is pretty much identical to WCF's ExceptionDetail class and only exists because Silverlight 2 did not support ExceptionDetail) and an ExceptionType enum:

<div>
[csharp]
    public enum ExceptionType
    {
        None,
        Business,
        Security,
        EarlierRequestAlreadyFailed,
        Unknown
    }
[/csharp]
</div>

The ExceptionInfo class (or ExceptionDetail if you would prefer to use that) does not contain the original Exception object, so the ExceptionType enumeration can be used to handle each type of exception differently on the client-side.  Obviously, if a Request was handled without problems, the ExceptionInfo property of its Response will be null, and the ExceptionType property will be set to ExceptionType.None.

The idea is basically that instead of defining operations on your services, you create a specific Request class and a corresponding Response class for each operation that you would normally expose on your service.  The request type <em>is</em> the operation, its properties are the operation's parameters and the Response type is the operation's return value.  Note that you can define multiple properties on your Response types, effectively giving you the ability to support multiple return values for an operation without having to use out parameters.

A simple example of a Request and its Response can look like this:

<div>
[csharp]
    public class GetProductOverviewsRequest : Request
    {
        public string NamePattern { get; set; }
        public int? CategoryId { get; set; }
        public int? SupplierId { get; set; }
    }
 
    public class GetProductOverviewsResponse : Response
    {
        public ProductOverviewDto[] Products { get; set; }
    }
[/csharp]
</div>

Note that by default, if you're using at least .NET 3.5 SP1, you do not need to mark your Request/Response types as Serializable or apply the DataContract/DataMember attributes on them <em>if</em> the assembly containing the Request/Response types can be shared between both your service and your clients.  If your service needs to be interoperable with other platforms or you can't share the assembly containing the types, then you obviously do need to use the DataContract/DataMember attributes but if you're only communicating with .NET clients and you can share the assembly, it's not needed.  Provided that all of the clients also use at least .NET 3.5 SP1 obviously.

At this point you might be wondering: "what exactly is the benefit of doing this?".  Well, at this point in the series there is only one benefit that you can see already: the fact that the Process method of the service receives <em>an array</em> of requests and returns <em>an array</em> of corresponding responses.  That means that you can easily send multiple requests to the service with only one roundtrip, which can greatly reduce the network communication overhead that you can otherwise suffer with typical Service Contracts, especially if their interface is too chatty.

There are many more benefits to using the RRSL, but you'll have to read the rest of the posts in the series to learn about those :)