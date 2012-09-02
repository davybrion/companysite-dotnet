Here's the situation: i have a domain model that is continuously kept in memory. There is a facade in front of this domain model which exposes some methods to make changes to the objects of the domain model.   These methods can be called by consumers of the facade at any time.  The in-memory domain model is also periodically updated by an update process.  While this update process is running, any change that is requested through the facade needs to be queued for later execution (when the update process is finished).  There is no persistent storage and any queued method calls do not need to be persisted either. 

At first, i thought about mapping incoming method calls on the facade to 'message' objects which would contain all the needed information to perform the operation. The operation could then be performed by a piece of code that uses the values in the 'message' object.  It sounds like a sound approach, and it would probably be necessary to use this if the queued method calls would need to be persisted.  But that's not a requirement.  With that in mind, i figured i could take advantage of some of the new C# features to avoid having to map method calls to 'message' objects.

What i wanted was to create a class you can just pass a piece of code too, and depending on a condition that you define, that piece of code is either executed immediately, or queued for later execution.  Obviously, the caller needs some kind of feedback... you want to know if your operation succeeded, if it failed, what the possible failure was or if it was queued.  And if it was queued, you want to be able to retrieve the result of the operation later on.  So i started playing around... the first thing i'd need was the following class:

<code>

<div style="font-family: Consolas; font-size: 10pt; color: black; background: white;">
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; <span style="color: blue;">public</span> <span style="color: blue;">class</span> <span style="color: #2b91af;">ExecutionResult</span></p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; {</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span style="color: blue;">public</span> <span style="color: blue;">bool</span> Succeeded { <span style="color: blue;">get</span>; <span style="color: blue;">set</span>; }</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span style="color: blue;">public</span> <span style="color: blue;">bool</span> Failed { <span style="color: blue;">get</span>; <span style="color: blue;">set</span>; }</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span style="color: blue;">public</span> <span style="color: blue;">bool</span> Queued { <span style="color: blue;">get</span>; <span style="color: blue;">set</span>; }</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span style="color: blue;">public</span> <span style="color: #2b91af;">Exception</span> Exception { <span style="color: blue;">get</span>; <span style="color: blue;">set</span>; }</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span style="color: blue;">public</span> <span style="color: #2b91af;">Guid</span> QueuedOperationId { <span style="color: blue;">get</span>; <span style="color: blue;">set</span>; }</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; }</p>
</div>

</code>

If i use instances of this class as the return value of the operations, i can get all the feedback i need.  I also need something to group a piece of code with an identifier:

<code>

<div style="font-family: Consolas; font-size: 10pt; color: black; background: white;">
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span style="color: blue;">public</span> <span style="color: blue;">class</span> <span style="color: #2b91af;">Operation</span></p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; {</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span style="color: blue;">public</span> <span style="color: #2b91af;">Action</span> Action { <span style="color: blue;">get</span>; <span style="color: blue;">set</span>; }</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span style="color: blue;">public</span> <span style="color: #2b91af;">Guid</span> Id { <span style="color: blue;">get</span>; <span style="color: blue;">set</span>; }</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; }</p>
</div>

</code>

The Action property can contain any piece of code which you can then identify through the Guid Id property.

Now we can implement the ConditionallyQueuedExecutor class (the name sucks, but i couldn't find one that sucked less).  First, we need a queue of Operations and we also need to store the results of operations that were queued and executed later on:

<code>

<div style="font-family: Consolas; font-size: 10pt; color: black; background: white;">
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span style="color: blue;">private</span> <span style="color: blue;">readonly</span> <span style="color: #2b91af;">Queue</span>&lt;<span style="color: #2b91af;">Operation</span>&gt; operationQueue = </p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span style="color: blue;">new</span> <span style="color: #2b91af;">Queue</span>&lt;<span style="color: #2b91af;">Operation</span>&gt;();</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span style="color: blue;">private</span> <span style="color: blue;">readonly</span> <span style="color: #2b91af;">Dictionary</span>&lt;<span style="color: #2b91af;">Guid</span>, <span style="color: #2b91af;">ExecutionResult</span>&gt; resultsOfQueuedOperations = </p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span style="color: blue;">new</span> <span style="color: #2b91af;">Dictionary</span>&lt;<span style="color: #2b91af;">Guid</span>, <span style="color: #2b91af;">ExecutionResult</span>&gt;();</p>
</div>

</code>

I also want to be able to provide a default condition that should be checked when we try to execute a piece of code:

<code>

<div style="font-family: Consolas; font-size: 10pt; color: black; background: white;">
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span style="color: blue;">public</span> <span style="color: #2b91af;">Func</span>&lt;<span style="color: blue;">bool</span>&gt; DefaultCondition { <span style="color: blue;">get</span>; <span style="color: blue;">set</span>; }</p>
</div>

</code>

And then of course, we must offer a way to either perform the given piece of code, or to add it to the queue:

<code>

<div style="font-family: Consolas; font-size: 10pt; color: black; background: white;">
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span style="color: blue;">public</span> <span style="color: #2b91af;">ExecutionResult</span> DoOrQueue(<span style="color: #2b91af;">Action</span> action)</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; {</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span style="color: blue;">return</span> DoOrQueue(DefaultCondition, action);</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; }</p>
<p style="margin: 0px;">&nbsp;</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span style="color: blue;">public</span> <span style="color: #2b91af;">ExecutionResult</span> DoOrQueue(<span style="color: #2b91af;">Func</span>&lt;<span style="color: blue;">bool</span>&gt; condition, <span style="color: #2b91af;">Action</span> action)</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; {</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span style="color: blue;">if</span> (condition())</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; {</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span style="color: blue;">return</span> TryToRun(action);</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; }</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span style="color: blue;">else</span></p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; {</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span style="color: blue;">return</span> AddToQueue(action);</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; }</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; }</p>
</div>

</code>

I added an overload that allows you to pass in a specific condition for greater flexibility.  So what does this do? As you can see, it merely checks the passed in condition, and if it returns true it will try to run the given piece of code.  If it returns false, we add the piece of code to the queue.

The TryToRun and AddToQueue methods look like this:

<code>

<div style="font-family: Consolas; font-size: 10pt; color: black; background: white;">
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span style="color: blue;">private</span> <span style="color: #2b91af;">ExecutionResult</span> TryToRun(<span style="color: #2b91af;">Action</span> action)</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; {</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span style="color: blue;">var</span> result = <span style="color: blue;">new</span> <span style="color: #2b91af;">ExecutionResult</span>();</p>
<p style="margin: 0px;">&nbsp;</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span style="color: blue;">try</span></p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; {</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; action();</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; result.Succeeded = <span style="color: blue;">true</span>;</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; }</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span style="color: blue;">catch</span> (<span style="color: #2b91af;">Exception</span> e)</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; {</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; result.Failed = <span style="color: blue;">true</span>;</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; result.Exception = e;</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; }</p>
<p style="margin: 0px;">&nbsp;</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span style="color: blue;">return</span> result;</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; }</p>
</div>
<br />
<div style="font-family: Consolas; font-size: 10pt; color: black; background: white;">
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span style="color: blue;">private</span> <span style="color: #2b91af;">ExecutionResult</span> AddToQueue(<span style="color: #2b91af;">Action</span> action)</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; {</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span style="color: blue;">var</span> result = <span style="color: blue;">new</span> <span style="color: #2b91af;">ExecutionResult</span> { Queued = <span style="color: blue;">true</span>, QueuedOperationId = <span style="color: #2b91af;">Guid</span>.NewGuid() };</p>
<p style="margin: 0px;">&nbsp;</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; operationQueue.Enqueue(<span style="color: blue;">new</span> <span style="color: #2b91af;">Operation</span> { Id = result.QueuedOperationId, Action = action });</p>
<p style="margin: 0px;">&nbsp;</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span style="color: blue;">return</span> result;</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; }</p>
</div>

</code>

The TryToRun method is pretty straightforward... it tries to execute the given piece of code, and if it worked without exceptions it returns a successful ExecutionResult instance.  If an exception was caught, it returns a failed ExecutionResult which contains the original exception.  The AddToQueue method is also pretty straightforward: it combines the given code with a Guid and stores the two values in an Operation instance.  That instance is added to the queue and the returned ExecutionResult contains the identifier of the queued operation, which can be used later on to retrieve the ExecutionResult once the operation in the queue has actually been executed.

So how are the operations in the queue executed? Instead of going for a smart and fancy auto-detection scheme based on the condition i went for a simple public method that has to be called explicitly:

<code>

<div style="font-family: Consolas; font-size: 10pt; color: black; background: white;">
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span style="color: blue;">public</span> <span style="color: blue;">void</span> RunQueuedOperations()</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; {</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span style="color: #2b91af;">Operation</span> operation;</p>
<p style="margin: 0px;">&nbsp;</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span style="color: blue;">while</span> ((operation = GetOperationToExecute()) != <span style="color: blue;">null</span>)</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; {</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span style="color: blue;">var</span> result = TryToRun(operation.Action);</p>
<p style="margin: 0px;">&nbsp;</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; resultsOfQueuedOperations.Add(operation.Id, result);</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; }</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; }</p>
<p style="margin: 0px;">&nbsp;</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span style="color: blue;">private</span> <span style="color: #2b91af;">Operation</span> GetOperationToExecute()</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; {</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span style="color: blue;">if</span> (operationQueue.Count == 0) <span style="color: blue;">return</span> <span style="color: blue;">null</span>;</p>
<p style="margin: 0px;">&nbsp;</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span style="color: blue;">return</span> operationQueue.Dequeue();</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; }</p>
</div>

</code>

As you can see, it tries to execute each operation in the queue, and it stores the ExecutionResult in a dictionary with the operation's identifier as the key value.

The original caller can then retrieve the final ExecutionResult through this method:

<code>

<div style="font-family: Consolas; font-size: 10pt; color: black; background: white;">
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span style="color: blue;">public</span> <span style="color: #2b91af;">ExecutionResult</span> GetResultOfQueuedOperation(<span style="color: #2b91af;">Guid</span> id)</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; {</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span style="color: blue;">if</span> (!resultsOfQueuedOperations.ContainsKey(id)) <span style="color: blue;">return</span> <span style="color: blue;">null</span>;</p>
<p style="margin: 0px;">&nbsp;</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span style="color: #2b91af;">ExecutionResult</span> result = resultsOfQueuedOperations[id];</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; resultsOfQueuedOperations.Remove(id);</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span style="color: blue;">return</span> result;</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; }</p>
</div>

</code>

And that's it basically. You can now use this class like this:

<code>

<div style="font-family: Consolas; font-size: 10pt; color: black; background: white;">
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span style="color: blue;">var</span> result = executor.DoOrQueue(</p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; () =&gt; !<span style="color: #2b91af;">UpdateService</span>.IsRunning(), </p>
<p style="margin: 0px;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; () =&gt; <span style="color: blue;">new</span> <span style="color: #2b91af;">ChangeHandler</span>().DoYourStuff());</p>
</div>

</code>

Obviously this is just a fake example, but you get the idea.

I wrote this code last week, and i'm still not sure what to think of it.  I kept it all pretty explicitely: the queued operations need to be triggered by a method call, the final ExecutionResults need to be retrieved by a method call... Surely, it would be possible to make this all work a bit more automagically, no? It actually is possible but i'm affraid it would make the whole thing much harder to understand. And it might be hard to understand as it is already.  The example above also isn't thread-safe (the real version is, but i kept the thread-safety stuff out of this code for clarity) and you currently have no way of dealing with return values of the code that you want to execute.  But you could easily make the ExecutionResult a generic class with a generic ReturnValue property to store the given code's return value in if that were necessary.
