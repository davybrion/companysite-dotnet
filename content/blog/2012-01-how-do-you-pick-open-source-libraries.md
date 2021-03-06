I'm currently looking into which library I'm going to use to handle authentication in a breakable toy project. Now, despite it being just a breakable toy, I want to do it with as few constraints on technical quality as possible because I want to maximize the learning experience I'm going to get from it. That means I don't just want to quickly put something together that just works. I want something that works, but that would also hold up in real world scenarios, even though the project will at best only be used by myself. Which means that I'm going to be picky about any libraries that I take a dependency on, just as I would if this were a project that I'd be getting paid to work on. 

So as I was browsing through a few possible alternatives for my authentication needs, I started thinking about my thought process when evaluating libraries/frameworks to use. I generally base my decision on the following items, listed in order of importance (to me).

## How well does it work for my scenario?

If a library satisfies all other items on this list, that certainly doesn't mean it's an automatic lock. How it works and the impact it has on my code is definitely the most important factor.

## Popularity

I've noticed that I let the number of watchers/forks on sites like Github influence my opinion. If a project has many watchers and many forks, odds are high that there's a relatively large group of happy users as well as people involved with the project. It also increases the odds that the project will be around for a while. Of course, inactive Open Source projects often remain available as well but if nobody's working on it, I'm not exactly tempted to take a dependency on it. Log4net is a notable exception to this, obviously. But when a project has a lot of people interested in it, or better yet, contributing to it, it's a good sign that you'll easily get help if needed, it's only going to get better in the long run and that it might get forked should the original developers stop working on it. As the author of an Open Source project that doesn't have a lot of watchers/forks ([Agatha](https://github.com/davybrion/Agatha)), I'm aware that my point of view on this is rather hypocritical but hey, it is what it is.

## Code quality

I don't have the time to do an in-depth review of the code as I'm sure most of us don't do either. But I do like to glance over the code to get a general feel of the quality of the code. I focus mostly on the clarity of the code and also keep an eye open for sloppiness or downright WTF's. I guess the questions I'm mostly trying to answer when doing this are: "is this code I'd like to try to improve or fix if I need to?" and "how easy would it be to debug this when I need to troubleshoot some non-obvious issues?". 

## Location of code and issue tracker

A lot of people will probably take issue with this, but I consider it to be a major plus if the project is on Github. Not just because of my personal preference of Github, but because they truly encourage people to collaborate and contribute to projects and they make it very easy to do so. Also, the site is fast! I cringe when I have to look over issues of projects on Codeplex because it's just terribly slow. And the UI doesn't come close to that of Github either. I've heard that Bitbucket is pretty similar to Github, but I've never even looked for projects there. In any case: I want to be able to download the *latest* version of the code at any time, or of a particular branch if I need to, as easily as possible. I also prefer an issue tracker which is fast, responsive and easy to search. It doesn't have to be Github, but those 2 requirements are important to me.

## License

If it's GPL, I don't use it. Also, I check whether or not a commercial license needs to be purchased when you want to use the library/framework in production. Pay attention to dual-licensed projects because that Open Source license might not apply to commercial/production use!

I'd love to hear your thoughts on this. Did I miss any important factors? I just quickly put this post together so it's likely that I missed some good ones :)