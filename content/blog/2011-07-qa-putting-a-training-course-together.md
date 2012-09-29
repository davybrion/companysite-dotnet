<a href="http://blog.functionalfun.net/" target="_blank">Samuel Jack</a> is considering putting together a training course on WPF and mailed me some questions on how I approached putting together my NHibernate course (and don't worry, this post will be the last time I write about the training for at least a couple of months :)). I thought it would be interesting to answer his questions in a post, so here we are.

Q1: *What experience did you have before running your first courses?*

None whatsoever. I hadn't even done any short training sessions or anything like that. I also had limited experience with public speaking, since I've only done one public talk back in 2009. But the key thing I picked up during that talk was that it was actually very easy as long as you just stay yourself. And to make matters worse, I've never actually followed any professional training so I didn't really know how things were usually 'done'. I was sorta nervous about that before I started working on the material, but after giving it some thought I decided to focus on just trying to put a course together that *i* could enjoy if I were attending it.

Q2: *What do you feel works best by way of written course material? I know you've put the code from your course up on GitHub. Do you also provide a handbook to go with the course?*

I didn't write a handbook, but I do use slides. Not too many of them because I thought it would kill the pace, so I have about 45 slides a day. Some of them contain more text than I usually prefer to have on a slide, but I tried to keep that to a minimum. I do think it increases the value of the material in terms of being a useful reference to go back to later on in combination with the example code. My course leans heavily on the example project and the output and observable behavior of the automated tests, so that helps in reducing the amount of information you'd otherwise provide in some kind of written form.

Q3: *What balance do you use in terms of talk, demos, and hands-on time?*

I try to switch between talk and demos as much as possible. I have 2 (continuous) hours of hands-on exercise time a day, and the other 5 hours or so (subtracting breaks) is pretty much just going back and forth between talking and showing the code and discussing its behavior. The slides always list the actual code files I want to show for every specific item that is covered, so I can switch between talking and showing code/running tests/discussing output without losing much time. It seems to keep people focused, and (hopefully) reduces boring moments to a minimum.

Q4: *How did you go about picking the content and structuring it?*

This was one of the hardest parts. I started out with picking a set of topics that I thought were most important for *efficient* NHibernate usage. That was the easy part. The hard part is trying to put things in the right order, while trying to keep it varied and trying to minimize the number of times you have to say you'll explain a certain thing later on. You're essentially trying to build up a story with enough variety and as few gaps as possible. With something like NHibernate where so many of the features are related or influence each other, that was a bit of a tough nut to crack.

My advice would be to write down the most important things you want the attendees to remember after the course, and then pick the content and build up your story to support that. Don't try to include everything that can be covered, because the story will quickly lose its focus. When that happens, your attendees will lose theirs as well :) 

Q5: *Any tips on speaking or presentation style? Icebreakers?*

The most important thing is to speak calmly and with confidence. Maintain eye-contact with the attendees and try to spread it evenly amongst them. The benefits of that can not be understated. You can often tell when people don't quite understand something they've just heard so you get plenty of clues on whether or not you should rephrase what you just said. Tell them in advance that everybody is free to ask questions whenever they pop up, and encourage them to ask their questions. Try to answer them as well as you can and stay calm and friendly at all times. And obviously: don't be a dick and don't act condescending. If the attendees can sympathize with you, they'll try harder to stay focused during the boring parts. If they don't like you, they're much more likely to tune out.

Don't be afraid to make the occasional joke here or there, but keep in mind that you're not being payed to do stand-up. And if you don't get the laugh you thought you would, move on quickly and don't let it shake you ;). And obviously, stay away from anything that could be offensive to anyone, no matter how funny you think it might be.

Q6: *You mentioned a while back finding preparation for the course a bit tedious - how long did it take to prepare, and any tips on getting though it?*

I seriously underestimated how long it would take me to prepare. I originally estimated about 24 hours for each day of the course but I think I ended up slightly over twice that (i didn't list the actual hours, wish I had though). I wouldn't be surprised if I put 160 hours into it, which is a full month's worth of work. Of course, the biggest reason it took so long was because the material was so example-heavy. To give you an idea: the full project has about 250 automated tests, and only about 30 of them are for repetitive CRUD actions.

Unfortunately, I did all of this after-hours and in weekends because I was still doing my regular hours at a client so it was pretty hectic for a while. It certainly didn't help that the prep work was generally pretty boring. Not sure if I can give you good tips on getting through it... I had told a bunch of people I was going to do the course before I actually started working on it and I didn't want to lose face so I had no choice but to get it done :)