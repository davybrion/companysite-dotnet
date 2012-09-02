Before i post the tests and a further explanation of the code i listed in <a href="http://davybrion.com/blog/2009/02/challenge-do-you-truly-understand-this-code">the readable code challenge</a>, i wanted to post only the commented version of this code:

<div>
[csharp]
    public class Broadcaster : IBroadcaster
    {
        private readonly object monitor = new object();
 
        // This reference will be overwritten with a new instance of a clients List whenever
        // we need to add or remove a client from the list. The reason for this is because
        // we need to be able to loop through the clients list, but during this loop clients
        // might try to register or might be removed from the clients list.
        // Using the same instance of the clients List and synchronizing all access with the
        // monitor object would not be sufficient because if both the loop and the removal
        // would lock on monitor, and the loop and removal would happen on the same
        // thread then one of the operations won't block, because that thread already has
        // acquired the lock. At that point we get a concurrency exception because clients'
        // Enumerator would have been modified while looping through it.
        private List&lt;IClientProxy&gt; clients;
 
        private readonly IClientProxyFactory clientProxyFactory;
 
        public Broadcaster(IClientProxyFactory clientProxyFactory)
        {
            this.clientProxyFactory = clientProxyFactory;
            clients = new List&lt;IClientProxy&gt;();
        }
 
        public void Register()
        {
            var client = clientProxyFactory.CreateClientProxyForCurrentContext(&quot;Nokeos/IBroadcastOverWcf/Receive&quot;);
            client.Faulted += Client_Faulted;
            AddClientToRegisteredClients(client);
        }
 
        private void AddClientToRegisteredClients(IClientProxy client)
        {
            lock (monitor)
            {
                // we create a new List instance based on the previous list plus the new client
                // and then we assign the new list to the clients reference that the rest of
                // this class uses. This happens behind a lock to make sure that the clients
                // reference isn't overwritten by the RemoveClientFromRegisteredClients method
                // simultaneously
                clients = new List&lt;IClientProxy&gt;(clients) { client };
            }
        }
 
        public void Broadcast(Notification notification)
        {
            // When we enter the foreach loop, we get a reference to the enumerator of
            // the _current_ clients reference (the reference might be overwritten while
            // we loop, but our enumerator will never be modified) which means we don't
            // need to use a lock here
            foreach (var client in clients)
            {
                // if the send operation fails, the client's Faulted event will be
                // triggered which causes removal of the client from our clients list.
                // this can happen either while we are in this loop, or afterwards in
                // a background thread but in both cases, we don't need to worry about it
                // here
                client.SendNotificationAsynchronously(notification);
            }
        }
 
        private void Client_Faulted(object sender, System.EventArgs e)
        {
            var client = (IClientProxy)sender;
            client.Faulted -= Client_Faulted;
            RemoveClientFromRegisteredClients(client);
        }
 
        private void RemoveClientFromRegisteredClients(IClientProxy client)
        {
            lock (monitor)
            {
                // we create a new List instance based on the previous list minus the client
                // that needs to be removed and then we assign the new list to the clients
                // reference. This happens behind a lock to avoid a simultaneuos overwrite
                // of the clients reference by the AddClientToRegisteredClients method
                var clientList = new List&lt;IClientProxy&gt;(clients);
                clientList.Remove(client);
                clients = clientList;
            }
        }
    }

[/csharp]
</div>

I hope the comments clarify the problem sufficiently. If not, tell me what's not clear to you because it might mean i need to clarify the comments more.  Also, suggestions on how to make this code more readable and reducing the need for comments would be very welcome :)

I'll post the tests tomorrow, and then you can all decide what you think is more communicative: the comments, or the tests.