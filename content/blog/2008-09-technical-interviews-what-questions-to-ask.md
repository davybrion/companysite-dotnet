I recently had to do an impromptu technical interview with an applicant at work. I spent the last few years working at clients instead of our home office so it had been a while since i had to conduct a technical interview.  So unfortunately, i couldn't really think of great questions to ask.  I did ask a few good questions (at least, i think so), but it got me thinking afterward about what would be a great way to conduct a technical interview.  I came up with an approach that i think will be pretty good.  I haven't tried it yet, but next time i'll definitely do it like this:

<ol>
	<li><strong>Ask if they know the Single Responsibility Principle (SRP)</strong>
<p>
Some people will know, some don't.  If they don't, explain it to them.  If they do, let them explain it. 
</p>
</li>

<li><strong>Ask them to talk about a class in the system they're currently working on that violates the SRP</strong>
<p>
In case they didn't know about the SRP, this will quickly tell you how easily they can understand new concepts since you've just explained it.  If they did know about it and were able to explain it correctly, they should be able to give you a good example.  If they were able to explain it, but can't give you a good example, that's a sign that something isn't right.  It might be an indication that the applicant has trouble mixing theory with practice.
</p>
</li>

<li><strong>Ask them what they would do to fix the problems of that class when given the chance to clean it up</strong>

<p>
This should give you some valuable insight as to how the applicant thinks about how code should be structured.  I'm not even talking about big design stuff, just simple class design.  The answer to this question will most likely enable you to start an interesting discussion about class design and writing code in general, while not distracting the applicant with fictional example classes. After all, we're talking about code the applicant should know pretty well.
</p>
</li>

</ol>

And that's it. The discussion that the final question should lead into should tell you most of the things you need to know about the applicant.  You can talk about TDD, writing code, refactoring, design, pretty much everything...  Depending on the position that's available and the skill-level of the applicant you could also start talking about architecture stuff. 

I think this approach allows you to learn a great deal about the applicant's technical skills in about 20 minutes.  I personally don't care how well an applicant knows a certain API or library, i'm interested in finding out how well the applicant can deal with new stuff, concepts that he/she may not have any experience with.  Basically, how well the applicant can adapt to different technical situations.  

Oh, and a quick tip to future applicants: mentioning that you read my blog is a tricky approach.  While flattering, i may or may not feel the urge to figure out if you're really reading it or if you're just giving the posts a quick read :P
