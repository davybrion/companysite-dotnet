If you've ever read something about Test Driven Development or Continuous Integration then you've undoubtedly read that one of the key advantages of these practices is to shorten the feedback loop of your development process.  Basically, that means being able to detect bugs or mistakes as soon as possible and being able to fix them as quickly as possible with as little effort as possible while keeping the quality and sanity of your code base as high as possible.

Those are obviously important goals. There's another reason why a fast build (with that i mean building the code and running all of the tests) is important.  I believe the duration of a build can be a good indication of the complexity and the quality of <strong>the code base</strong> of your project. If a build takes a long time, it's usually a strong indication that something is definitely not right in the code. 

Let's apply the technique of <a href="http://en.wikipedia.org/wiki/Root_cause_analysis">Root Cause Analysis</a> to the problem of a slow build of a fictional (yet very common) project:

Question: Why is the build so slow?
Answer: Because the tests take a long time to run

Question: Why do the tests take so long?
Answer: Because we need a lot of set up in the code to be able to run the tests

Question: Why do we need all that set up?
Answer: Because the code under test requires a lot of stuff to be in place before we can run it

Question: Why does the code under test require a lot of stuff to be in place before we can run it?
Answer: Because those classes do a lot of stuff... they get data from the database, do something with it and put it back into the database.

Question: Why do those classes do so much?
Answer: They instantiate Data Access Components and they don't work without real data in the database.

Bingo.

On a code level, what is the problem here? The last 2 answers indicate that the classes that are being tested have too many responsibilities and that there probably is a lot of highly coupled code in there.  By looking at the build time and asking those questions to the developers, you're able to learn a lot about the quality of the code without having any prior knowledge about the project or the code. 

Obviously, it's not always possible to reach valid conclusions based purely on the build time of a project.  But again, it can be a great indication.

Now, the project mentioned above might actually have a really low number of bugs in it.  The application might even be perfect according to its users.  But that doesn't mean that the code of the application is in good shape.  If the code were in better shape, it might have been cheaper to develop the application.  It might mean that the maintenance of the code would be cheaper too.  But the slow build and the answers to the question do indicate complexity in the code.  We can't always eliminate complexity but we should strive to minimize it, because it leads to cheaper development in the long term.

Granted, a fast build time doesn't always mean that a code base is in great shape either.  That's a conclusion you could only reach by looking at the code and going through the tests.  But for slow builds, you'd be amazed what you could quickly learn from a few questions.