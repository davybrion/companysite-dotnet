After the <a href="http://davybrion.com/blog/2009/02/challenge-do-you-truly-understand-this-code">uncommented code</a>, and then the <a href="http://davybrion.com/blog/2009/02/the-commented-version-of-the-readable-code-challeng/">commented version of the code</a>, you finally get to see the tests that verify that solution protects the code from the issue it was facing.  I think all 3 posts (and the comments on them) sufficiently explain the problem and the solution so i won't go through the trouble of explaining everything in this post.  The tests however, might not be very clear to everyone.  I'm only posting 3 tests, though there are more but then the post would just be way too long.

These tests use the following 2 fields:

<script src="https://gist.github.com/3684351.js?file=s1.cs"></script>

Which are set up before each test like this:

<script src="https://gist.github.com/3684351.js?file=s2.cs"></script>

First of all, take a look at some of the utility methods that these tests use:

<script src="https://gist.github.com/3684351.js?file=s3.cs"></script>

And then the actual tests:

<script src="https://gist.github.com/3684351.js?file=s4.cs"></script>

Note: i'm not sure if this is actually the best way to test this code... there will probably be better solutions for testing threading issues.