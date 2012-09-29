A mistake that you commonly see developers make (especially young ones) is to try to achieve perfection when they need to design something. Whether it’s just a reusable base class, a component, or even a small library or framework, a lot of people get too caught up in details that don’t really matter in most cases.

It is understandable though, and I think most developers have gone down that path quite a few times in their careers. Especially early on. You know how it goes… you need to create something that is going to be reused by others, and you want to make sure that it is just perfect. That nobody can say anything bad about it. That everyone will agree without discussion that that piece of code is simply great.

So a lot of people in that situation start thinking about things like:

- Shouldn’t I seal this class? Nobody’s ever going to need to derive from this because I've left plenty of other extensibility points
- Shouldn’t I seal this method? After all, nobody should ever change the way this particular piece of code works
- This class will never be used by anyone outside of this assembly, so I probably should make it internal, right?
- This particular method will be a bottleneck so I should really make it as fast as I can
- Whenever I can pull some common logic in a base class, or introduce more base classes I should do so!
- Etc…

The reality of the situation is that despite your best intentions, focusing too much on details like that will quite frequently lead to very inflexible code that a lot of people will find hard or annoying to use. Unless you are very experienced with this kind of stuff (and for the record, I'm not saying that I am) you’re very likely to get these decisions wrong if you think about them up front so it really isn’t worth spending so much time on ‘details’ like that. In fact, the best thing to do is often to keep things as simple as possible until you actually have a reason to make them more complicated. Making things more complicated than they need to be in advance never works, and you’re probably going to over-complicate things in places where it turns out not to matter. If you’re really unlucky, that will actually make it harder to modify or extend other parts that over time really do need to be modified or extended in some ways.

Generally speaking, I think you’re better off focusing on the following goals/principles:

- Avoid writing classes that are <a href="/blog/2009/10/slutty-types/" target="_blank">slutty</a>
- Make sure that your consumers primarily communicate with your classes through interfaces. Though you don’t need to put everything behind an interface either… pretty much anything that people might need/want to change in some way typically are good candidates.
- Use Dependency Injection so implementations can easily be switched with others
- Use virtual methods unless you can think of a really good reason not to 
- Don’t make your classes internal unless you can think of a good reason to do so (and keep in mind that we can still do whatever we want with them through reflection, and will do so if that turns out to be the best way to get something working the way we need it to if you failed to provide proper extension points)
- Do not seal classes unless you can think of a really good reason to do so
- Do not worry about performance up front, unless for those places where you are going out-of-process (remote services, databases, file systems, etc…)
- Keep your classes small and focused
- Learn about the SOLID principles, and apply them. Keep in mind that going for 100% SOLID code is typically not worth it either. Go for the 80/20 rule here.

If you keep those things in mind, you will typically end up with code that is flexible to use, and easy to change. Obviously, all of this assumes that you are in a position where you can go back to the code and make changes. If you’re releasing frameworks/libraries/components that will be used by a lot of people and can’t afford to break backwards compatibility, then you probably need to be more strict about these things because then you can’t always just go back to change something. I don’t think it’s a stretch to claim that most developers are not in this situation however, so most of us often don’t need to waste time thinking about those things in advance ;)