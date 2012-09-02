After the <a href="http://davybrion.com/blog/2009/02/challenge-do-you-truly-understand-this-code">uncommented code</a>, and then the <a href="http://davybrion.com/blog/2009/02/the-commented-version-of-the-readable-code-challeng/">commented version of the code</a>, you finally get to see the tests that verify that solution protects the code from the issue it was facing.  I think all 3 posts (and the comments on them) sufficiently explain the problem and the solution so i won't go through the trouble of explaining everything in this post.  The tests however, might not be very clear to everyone.  I'm only posting 3 tests, though there are more but then the post would just be way too long.

These tests use the following 2 fields:

<div>
[csharp]
        private Broadcaster broadcaster;
        private IClientProxyFactory clientFactory;
[/csharp]
</div>

Which are set up before each test like this:

<div>
[csharp]
            clientFactory = MockRepository.GenerateMock&lt;IClientProxyFactory&gt;();
            broadcaster = new Broadcaster(clientFactory);
[/csharp]
</div>

First of all, take a look at some of the utility methods that these tests use:

<div>
[csharp]
        private List&lt;IClientProxy&gt; GetBroadcastersClients()
        {
            var clientsFieldInfo = typeof(Broadcaster).GetField(&quot;clients&quot;, BindingFlags.NonPublic | BindingFlags.Instance);
            return (List&lt;IClientProxy&gt;)clientsFieldInfo.GetValue(broadcaster);
        }


        private Exception GetExceptionThrownBy(Action yourCode)
        {
            try { yourCode(); } catch (Exception e) { return e; }
            return null;
        }
 
        private void RegisterClientWithBroadcaster(IClientProxy client)
        {
            clientFactory.Stub(f =&gt; f.CreateClientProxyForCurrentContext(null))
                .IgnoreArguments().Return(client).Repeat.Once();
 
            broadcaster.Register();
        }
 
        private IClientProxy RegisterClientWithImplementationForSend(Action implementation)
        {
            var client = MockRepository.GenerateMock&lt;IClientProxy&gt;();
 
            client.Stub(c =&gt; c.SendNotificationAsynchronously(null))
                .IgnoreArguments().WhenCalled(obj =&gt; implementation());
 
            RegisterClientWithBroadcaster(client);
 
            return client;
        }
 
        private IClientProxy RegisterClientWithEmptySendImplementation()
        {
            return RegisterClientWithImplementationForSend(() =&gt; { });
        }
 
        private void RegisterClientsWithImplementationForSend(int number, Action implementation)
        {
            var clients = new IClientProxy[number];
 
            for (int i = 0; i &lt; number; i++)
            {
                clients[i] = RegisterClientWithImplementationForSend(implementation);
            }
        }
[/csharp]
</div>

And then the actual tests:

<div>
[csharp]
        [Test]
        public void RegisterClientWhileBroadcasting_ClientIsAddedAndBroadcastingDidntThrowException()
        {
            RegisterClientsWithImplementationForSend(5, () =&gt; Thread.Sleep(50));
 
            Exception exceptionFromBroadcastThread = null;
            var broadcastThread =
                new Thread(() =&gt; exceptionFromBroadcastThread = GetExceptionThrownBy(() =&gt; broadcaster.Broadcast(null)));
 
            broadcastThread.Start();
            Thread.Sleep(50);
 
            var newClient = RegisterClientWithEmptySendImplementation();
 
            broadcastThread.Join();
 
            Assert.IsNull(exceptionFromBroadcastThread);
            Assert.That(GetBroadcastersClients().Contains(newClient));
        }
 
        [Test]
        public void ClientFaultedWhileBroadcasting_FaultedClientIsRemovedFromClientsList()
        {
            RegisterClientsWithImplementationForSend(2, () =&gt; { });
 
            var faultyClient = MockRepository.GenerateMock&lt;IClientProxy&gt;();
 
            faultyClient.Stub(c =&gt; c.SendNotificationAsynchronously(null))
                .IgnoreArguments().WhenCalled(obj =&gt; faultyClient.Raise(c =&gt; c.Faulted += null, faultyClient, EventArgs.Empty));
 
            RegisterClientWithBroadcaster(faultyClient);
            RegisterClientsWithImplementationForSend(2, () =&gt; { });
 
            broadcaster.Broadcast(null);
 
            Assert.IsFalse(GetBroadcastersClients().Contains(faultyClient));
        }
 
        [Test]
        public void ClientFaultedInSeparateThreadWhileBroadcasting_FaultedClientIsRemovedWithoutExceptionDuringBroadcasting()
        {
            var faultyClient = RegisterClientWithEmptySendImplementation();
            RegisterClientsWithImplementationForSend(10, () =&gt; Thread.Sleep(50));
 
            Exception exceptionFromBroadcastThread = null;
            var broadcastThread =
                new Thread(() =&gt; exceptionFromBroadcastThread = GetExceptionThrownBy(() =&gt; broadcaster.Broadcast(null)));
            broadcastThread.Start();
            Thread.Sleep(150);
 
            faultyClient.Raise(c =&gt; c.Faulted += null, faultyClient, EventArgs.Empty);
 
            broadcastThread.Join();
 
            Assert.IsNull(exceptionFromBroadcastThread);
            Assert.IsFalse(GetBroadcastersClients().Contains(faultyClient));
        }
[/csharp]
</div>

Note: i'm not sure if this is actually the best way to test this code... there will probably be better solutions for testing threading issues.