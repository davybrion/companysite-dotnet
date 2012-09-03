I've only been using the server that's hosting this blog for a week or two, so I'm still keeping a close eye on it. I check usage graphs (cpu, disk I/O and network) a couple of times a day to verify whether things are still running smoothly. This morning, I saw a noticeable increase in CPU usage and network activity that lasted for about 11 hours. I logged into the machine, checked some logs and found out that someone had conducted an 11 hour lasting brute-force SSH attack. It doesn't make much sense to try that on my server since my SSH daemon doesn't allow password authentication, and indeed there was no successful login during the attack so no harm done, right? 

Even if such an attack is not successful, it does consume resources on the targeted server(s). And wasteful, unnecessary resource usage has always been a bit of a pet peeve of mine so I wanted to prevent this from happening again. For this particular scenario, it's pretty easy. I installed DenyHosts which routinely checks for repeated (configured at 5) failed log-in attempts, and adds the offending IP addresses to /etc/hosts.deny so every other attempted SSH connection from those IP addresses will be denied immediately. Each offending IP address will be purged from /etc/hosts.deny after 1 week. Then I added a firewall rule that prevents you from connecting through SSH more than 5 times in 60 seconds. If you go over 5 connections, it just starts dropping packets, and by the time the drop behavior for your IP address expires, you'll have been added to /etc/hosts.deny already. As I said, pretty easy in this scenario because there are great tools I can rely on.

But what would you do if you had to implement a strategy to deal with this yourself? The most interesting approach I've heard of is to add an incremental delay on each failed authentication attempt. If the user fails the authentication check, delay the response with 1 second. If the user fails the second time, delay the response with 2 seconds. Third failure means a delay of 3 seconds, and so on. This pretty much makes a brute-force or dictionary attack impossible. The key is though, that you can't block any of your request-handling threads because then you open yourself up to an easy DoS attack.

Implementing this for a web application built on Node.js and Express.js is incredibly easy (there's an ASP.NET MVC example later in this post btw). I took the [authorization example](https://github.com/visionmedia/express/blob/master/examples/auth/app.js) of Express.js and made just a few minor changes. First of all, I added the delayAuthenticationResponse function:

<script src="https://gist.github.com/3610630.js?file=s1.js"></script>

This is the most important part of the implementation. Every time we get here, we increment the number of attempts for this user by one and store the number in the user's session. Side note: this is one of the few things you'd actually want to use a session for: **session-related data**. Then we schedule the callback to be executed after the number of attempts * 1000 milliseconds have passed. The important part to remember here is that Node's event loop is not blocked by this, so our ability to handle other requests is *not impaired in any way*. The only one who suffers here is the attacker. Note that in a real world implementation, you'd probably only want to start increasing the delay after 5 attempts or so, in order to not piss off users who're just having problems remembering their password.

Then I changed the authenticate function so that it receives a session as the first parameter, and uses our delayAuthenticationResponse function whenever something goes wrong:

<script src="https://gist.github.com/3610630.js?file=s2.js"></script>

After that, it's just a matter of changing the function that is assigned to the login route:

<script src="https://gist.github.com/3610630.js?file=s3.js"></script>

And there we go. This effectively makes it impossible to brute-force your way into this web application, and I'm sure you can agree it was rather easy to do so. Of course, this is only because Node.js is inherently non-blocking. In an environment where non-blocking is the exception rather than the rule, you have to keep a few more things into account when trying to implement this strategy.

For instance, ASP.NET MVC is a typical blocking web framework. There's a certain number of threads that are waiting to handle requests, and once they receive a request, they process that request in its entirety. That means that if your code has to wait on something, the request handling thread is blocked and can't handle any other requests. So obviously, if you'd like to implement this strategy for dealing with repeated failed log-ins, you really want to avoid doing something like this: 

<script src="https://gist.github.com/3610630.js?file=s4.cs"></script>

(note: this is a slightly modified LogOn method from the default AccountController when selecting 'internet application' in the MVC project wizard)

While this looks like it does the same as the Node/Express example, it certainly doesn't. The experience for the attacker is the same, because each failed attempt causes the response time to be increased with an extra second. But on your server, the thread handling the request is blocking the whole time and is thus incapable of handling extra requests while you're making the attacker wait.

Luckily, you can use ASP.NET MVC's asynchronous controllers to provide an asynchronous implementation of an action without blocking the request handling thread:

<script src="https://gist.github.com/3610630.js?file=s5.cs"></script>

Your controller has to inherit from AsyncController instead of Controller to make this work. Of course, it's much more complicated and requires more ceremony compared to the Node/Express approach, but then again, ASP.NET MVC isn't optimized for this kind of usage whereas Node/Express definitely is.

Either way, no matter what web framework you use, if you can add an incremental delay to the response of each failed log-in attempt without blocking a request-handling-thread, you've added a very effective and low-cost protection against brute-force and dictionary attacks.
