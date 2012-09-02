As some of you already know, i'm a long-time Mac user.  At work i obviously use Windows, but at home i only use Windows (through VMWare) when i want to code in Visual Studio.  Other than that, i stay away from Windows as much as possible because i simply don't like it.   The recent <a href="http://monodevelop.com/">MonoDevelop</a> 2.2 beta release promises OS X support so i wanted to try it out.

Unfortunately, there is no integrated installer for both Mono and MonoDevelop, so you'll need to download and install Mono separately.  I used the stable 2.4.2 version which i downloaded <a href="http://www.go-mono.com/mono-downloads/download.html">here</a>.  After that, i downloaded and installed the 2.2 beta1 release of MonoDevelop <a href="http://monodevelop.com/Download">here</a>.

Installation is extremely quick and easy, so after a couple of minutes you have this on your screen:

<a href="http://davybrion.com/blog/wp-content/uploads/2009/09/01.png"><img src="http://davybrion.com/blog/wp-content/uploads/2009/09/01.png" alt="01" title="01" width="1021" height="711" class="aligncenter size-full wp-image-1661" /></a>

Doesn't really look like a typical OS X application due to the GTK+ usage, but we can live with that :)

I then wanted to create a new project:

<a href="http://davybrion.com/blog/wp-content/uploads/2009/09/02.png"><img src="http://davybrion.com/blog/wp-content/uploads/2009/09/02.png" alt="02" title="02" width="708" height="529" class="aligncenter size-full wp-image-1662" /></a>

There are some interesting project templates there (notice the iPhone and Moonlight options) but i just selected a regular console project.  After doing so, you get some interesting packaging options for your project:

<a href="http://davybrion.com/blog/wp-content/uploads/2009/09/03.png"><img src="http://davybrion.com/blog/wp-content/uploads/2009/09/03.png" alt="03" title="03" width="708" height="529" class="aligncenter size-full wp-image-1663" /></a>

After that you can start working on your project:

<a href="http://davybrion.com/blog/wp-content/uploads/2009/09/04.png"><img src="http://davybrion.com/blog/wp-content/uploads/2009/09/04.png" alt="04" title="04" width="974" height="675" class="aligncenter size-full wp-image-1664" /></a>

I wanted to run it first to make sure everything was working: 

<a href="http://davybrion.com/blog/wp-content/uploads/2009/09/05.png"><img src="http://davybrion.com/blog/wp-content/uploads/2009/09/05.png" alt="05" title="05" width="974" height="675" class="aligncenter size-full wp-image-1665" /></a>

I also wanted to try the integrated debugging so i put a breakpoint in the code:

<a href="http://davybrion.com/blog/wp-content/uploads/2009/09/06.png"><img src="http://davybrion.com/blog/wp-content/uploads/2009/09/06.png" alt="06" title="06" width="974" height="675" class="aligncenter size-full wp-image-1666" /></a>

Unfortunately, running it with the debugger (that icon isn't shown in the screenshot because it's not visible when you reduce the size of the window to the size shown in the picture) didn't make it break on the breakpoint... it just showed the output as it does when not running with the debugger.  I then looked in the solution options to see if i had to enable debugging or anything like that.  Didn't really find any debugging related settings (i was hoping for a 'make it work' checkbox or something like that) but i did see this:

<a href="http://davybrion.com/blog/wp-content/uploads/2009/09/07.png"><img src="http://davybrion.com/blog/wp-content/uploads/2009/09/07.png" alt="07" title="07" width="722" height="524" class="aligncenter size-full wp-image-1667" /></a>

You can set a few formatting options (obviously not as many as a Resharper user is used to) for your C# code, which is definitely a nice and important touch.  Unfortunately i got the following exception when saving my changes:

<a href="http://davybrion.com/blog/wp-content/uploads/2009/09/08.png"><img src="http://davybrion.com/blog/wp-content/uploads/2009/09/08.png" alt="08" title="08" width="622" height="410" class="aligncenter size-full wp-image-1668" /></a>

A very nice addition here would be a button which allows you to automatically report the bug with its stackstrace to the MonoDevelop team.  It would benefit both the developers and the users so i hope they will add this soon.

I also looked into some project specific settings and you'll find plenty of familiar options there:

<a href="http://davybrion.com/blog/wp-content/uploads/2009/09/09.png"><img src="http://davybrion.com/blog/wp-content/uploads/2009/09/09.png" alt="09" title="09" width="801" height="527" class="aligncenter size-full wp-image-1669" /></a>

Alright, enough with the settings... let's start coding!  Once i added the new file, i was slightly disappointed with the unnecessary whitespace in the file:

<a href="http://davybrion.com/blog/wp-content/uploads/2009/09/10.png"><img src="http://davybrion.com/blog/wp-content/uploads/2009/09/10.png" alt="10" title="10" width="979" height="689" class="aligncenter size-full wp-image-1670" /></a>

Most people will just delete this every time anyway, so it would be better if empty classes were created with as little excessive whitespace as possible.

And then i started coding.  Well, i tried to.  The editor was unbearably slow on my Macbook.  Ok, it's not the fastest machine (Macbook 2.1, 2.16Gh Core 2 Duo and 3GB RAM) but it should definitely be capable enough to write and edit code in a usable manner.  The editor would lag so much behind my typing that it was just completely unusable.  It's not because i type fast or anything, because it was unusable when typing slowly as well.  After a couple of lines of code, i simply gave up.  From what i did saw, it has simple code completion and intellisense support so that's nice.  But i really hope they can seriously speed up the editor soon.

All in all, i am hopeful that MonoDevelop on OS X will become a reasonably viable solution in the near future.  It's a simple IDE, nowhere near as powerful as Visual Studio (let alone one with Resharper) but it does have most of the important basics that you'll need.   At this point, i don't consider it usable (after all, writing and editing code is fairly important) but i really hope that the MonoDevelop team will fix this soon.  It would be very nice to be able to use this successfully on OS X.