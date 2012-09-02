I spent a little time this weekend working on something that would be of use to me: a simple utility on Node.js that calls <a href="http://www.jslint.com/">JSLint</a> on all JavaScript files within a given path (including its subdirectories), and only produces output if errors are found. It was pretty easy to write, and you can find it <a href="https://github.com/davybrion/node-jslint-all">here</a> in case you're interested.

There was one problem i ran into though. On Node, all I/O calls are non-blocking. Well, there are synchronous versions of some calls available, but you should use them as little as possible because they block the Node event-loop. One of the things i needed to do was to scan a given path recursively to find all JavaScript files in that path. And i just couldn't get it working the way i wanted to. The best i could do was a function that would invoke the callback once for every folder that was found. But i really wanted one that would only invoke the callback once, once the entire tree had been searched. I actually spent a few hours trying to get it working, and even looked for some good solutions in other projects. Most solutions i found also invoked the callback multiple times, others resorted to partially synchronous implementations. For a command-line tool like this, synchronous calls aren't a big deal but i'm trying to get better at typical Node programming, so i'm trying to avoid synchronous I/O.

So i tried yet another variation on my implementation, and it too didn't work properly. I got frustrated and said to myself "how hard can this be? i just want a list of files like 'find' would give me". Then it hit me that i could completely avoid the problem by just doing this:

<div>
[javascript]
var exec = require('child_process').exec;

function getJsFilesRecursively(startPath, callback) {
	exec('find ' + startPath, function(err, stdout) {
		var jsFiles = [];
		stdout.split('\n').forEach(function(f) {
			if (!/node_modules\//.test(f) &amp;&amp; /.js$/.test(f)) {
				jsFiles.push(f);
			};
		});

		callback(null, jsFiles);
	});
};
[/javascript]
</div>

No blocking I/O on the Node event-loop, and it took about 2 minutes to write. The only downside that it has is that it's not cross-platform because it uses the 'find' command which every *NIX-based system has, but Windows doesn't. So it will fail if you run it on Windows but for now, it'll do just fine and at least it enabled me to move on to the other stuff i needed to implement to get to the goal i initially set out for this tool.

Of course, by now i'm completely obsessed with nailing the recursive asynchronous folder-walking function so i will replace this with a proper version once i finally figure it out.