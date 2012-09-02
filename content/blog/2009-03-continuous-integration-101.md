More and more people are working in teams where Continuous Integration is used.  Not everyone truly 'gets' it though... Below is a quick list of things you, as a member of the team, need to keep in mind.

Before you commit your changes to the repository:

<ol>
	<li>You update your local copy to get all of the changes that were committed since you last updated your local copy</li>
	<li>If there are conflicts, you resolve them in your local copy... you DO NOT simply overwrite other people's changes</li>
	<li>You compile your code (You'd think nobody would forget this one, right? Think again...)</li>
	<li>If it doesn't compile, you fix it (Again, you'd think nobody would forget this one....)</li>
	<li>If it compiles, you run the tests.  At least run the fast tests, and i do mean all of them, not the ones you just wrote or modified.  If you have thousands of slow tests, then you obviously can't run them every single time you need to commit.  But you can and should definitely run the fast tests. Yes, all of them!</li>
	<li>If a test fails, fix the code that broke it, or fix the test if it's no longer correct.  Repeat this process until none of the fast tests fail.</li>
	<li>Commit your code, and begin working on your next task, take a break, go home, whatever is most appropriate at the time.</li>
</ol>

For the next section, i'm going to assume that everyone has at least something that continuously visualizes the state of the automated build, either TV's on the wall (like we have), or at least a system tray notifier (like we also have).

When the build breaks:

<ol>
	<li>If none of your team members beat you to it, look at the build results to figure out why it broke</li>
	<li>If it's a compile error, alert the team (so they don't accidentally update their local copy with the broken code), and figure out who's responsible.  MAKE SURE THE COMPILE ERROR(S) ARE FIXED ASAP.  As a team, punish the guilty one for wasting the team's time (make the guilty one buy donuts, pizza, make them wear a dress for a day, whatever works...)</li>
	<li>If it broke because of failing tests, look at the failures.  Where you the last one to touch those tests or the code that was being tested by them? Then fix it ASAP.  Was it someone else?  Discuss this with the person who's responsible.  If the developer who's responsible has time to fix it, then he/she must fix it at once.  If the developer can't fix it instantly, then someone else has to step up and fix it.  DO NOT WAIT FOR SOMEBODY ELSE TO FIX THEM.  Discuss it with the team and agree on someone to fix the tests.  If you're not sitting with the rest of the team, pick up the phone, use IM, whatever.  But at least do something!</li>
	<li>Repeat these steps until the build is green again.</li>
</ol>

As a member of the team, you have certain responsibilities.  Every team member has them.  You have to take your responsibilities seriously, if not, you're just wasting your team members' time.  At the same time, you should hold your team members accountable when they fail to take their responsibility serious.  Don't fight about it, but talk about it as a team.  When people don't do what they are supposed to do, discuss it with the entire team and after a couple of times (at most) people will feel that social pressure to not let the team down again.  If they don't, well then maybe they shouldn't be part of the team in the first place but that's a whole other story. 
