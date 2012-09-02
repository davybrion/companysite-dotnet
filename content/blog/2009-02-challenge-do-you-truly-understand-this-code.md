I think this might be an interesting challenge for all of you. The code that will be listed below contains a non-obvious solution.  The reason why i went with the non-obvious solution, or what exactly the non-obvious part is, will not be mentioned in this post.  What i want to know is: do you guys truly understand this code without any sort of "why i did this" comment in the code?  I think the code is very readable, and pretty communicative.  So according to many coding purists, it should not contain any comments.  I am particularly interested in finding out if these coding purists think that this code should contain a "why i did it like this" comment or not.

Now, it certainly is possible that there is a more communicative way to write the same code.  If so, i would be very interested to hear about any possible improvements you can come up with :)

I will do a follow-up post about this soon.  Perhaps tomorrow, or in a few days, depending on how clear or unclear this code turns out to be.

So, this is the code:

<div>
[csharp]
    public class Broadcaster : IBroadcaster
    {
        private readonly object monitor = new object();
        private List&lt;IClientProxy&gt; clients;
        private readonly IClientProxyFactory clientProxyFactory;
 
        public Broadcaster(IClientProxyFactory clientProxyFactory)
        {
            this.clientProxyFactory = clientProxyFactory;
            clients = new List&lt;IClientProxy&gt;();
        }
 
        public void Register()
        {
            var client = clientProxyFactory.CreateClientProxyForCurrentContext(&quot;MyCoolNamespace/IBroadcastOverWcf/Receive&quot;);
            client.Faulted += Client_Faulted;
            AddClientToRegisteredClients(client);
        }
 
        private void AddClientToRegisteredClients(IClientProxy client)
        {
            lock (monitor)
            {
                clients = new List&lt;IClientProxy&gt;(clients) { client };
            }
        }
 
        public void Broadcast(Notification notification)
        {
            foreach (var client in clients)
            {
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
                var clientList = new List&lt;IClientProxy&gt;(clients);
                clientList.Remove(client);
                clients = clientList;
            }
        }
    }
[/csharp]
</div>

Update: you can find the commented version of this code <a href="http://davybrion.com/blog/2009/02/the-commented-version-of-the-readable-code-challeng/">here</a>