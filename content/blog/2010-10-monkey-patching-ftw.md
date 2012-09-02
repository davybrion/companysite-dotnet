Today, i had to get a CI build of an IronRuby project that a coworker and me have been working on up and running.  We have a TeamCity server and i'm pretty familiar with it, at least as far as our .NET projects are concerned.  But this is the first project we're using IronRuby on, and for now it's exclusively Ruby code that has to run on IronRuby.  Our requirements of the CI build are very simple: check out the latest version of the code from Subversion, run the tests and make sure we can consult the test results from the TeamCity web interface.  That's it. How hard can that be, right?  

The thing is... we're not using an official IronRuby version.  I basically get the latest code from IronRuby's GitHub repository from time to time, build it, and we use that.  I've included all of the necessary files into our subversion repository so we can just refer to the correct IronRuby version with relative paths.  And no, it's not because we're trying to be cool or hardcore, it's because we depend on a fix that has been implemented in IronRuby already but that isn't present in one of the releases.  So my coworker sent me a link from the TeamCity documentation that mentioned that you could just use a Rakefile with IronRuby.  Easy peasy!  Well, except for the fact that it would require me to install the IronRuby build that we happen to be using on the build agents, and that i'd have to update it whenever i update the IronRuby binaries that we're using.  Not exactly an approach i'd prefer.

So i was already thinking along the lines of "great, we're gonna have to write yet another custom test runner to report the test results back to TeamCity".  We did it back when nobody cared about writing tests for Silverlight code, so i guess we could do it again.  But i just sort of looked up to it.  And then my coworker said "why not just monkey patch the test runner so it outputs the results in the format that TeamCity can understand?".  And he was right.  There's no reason whatsoever not to use a <a href="http://en.wikipedia.org/wiki/Monkey_patch">monkey patch</a> to get out of this bind.

The final result is a pretty minimal amount of code that didn't take long to write which gets the results we need.  Granted, i lost some time because at first i was monkey patching Test::Unit's console TestRunner only to find out that it's not really being used anymore if you're on Ruby 1.9... it's been replaced with MiniTest, which unfortunately (yet understandably) trades clean code for runtime performance.   If Test::Unit's console TestRunner was used, the final result would've been less than 20 lines of code in total.  Now, it's a bit more but it's still pretty minimal.

First of all, it's important to know the format that TeamCity can understand from your custom build output.  You can find all you need to know about that <a href="http://confluence.jetbrains.net/display/TCD5/Build+Script+Interaction+with+TeamCity#BuildScriptInteractionwithTeamCity-ReportingTests">here</a>.  Once you know the expected format, the solution is actually pretty easy: change the behavior of the testrunner at runtime so that it formats the output in a way that TeamCity can do something with it instead of its regular output.  Turns out i could limit my monkey patch to just one of MiniTest's classes, that being the MiniTest::Unit class.  First of all, we need to add some helper methods that we can use to take care of some of TeamCity's formatting requirements:

<div>
[ruby]
  def tc_output(string)
    tc_string = &quot;##teamcity[#{string}]&quot;
    puts tc_string
    tc_string
  end
     
  def tc_escape(string)
    string
      .gsub(&quot;|&quot;, &quot;||&quot;)
      .gsub(&quot;'&quot;, &quot;|'&quot;)
      .gsub(&quot;\n&quot;, &quot;|n&quot;)
      .gsub(&quot;\r&quot;, &quot;|r&quot;)
      .gsub(&quot;]&quot;, &quot;|]&quot;) 
  end
[/ruby] 
</div>

With those methods added to the MiniTest::Unit class, we can now modify the behavior of 2 methods of this class to get the result that we want and need.  First up, is the puke method, and no, i'm not joking... the method is actually called 'puke':

<div>
[ruby]
  def puke(klass, method, error)
    error = case error
      when MiniTest::Skip then
        @skips += 1
        tc_output &quot;testIgnored name='#{method}' message='test ignored'&quot;
      when MiniTest::Assertion then
        @failures += 1
        trace = MiniTest::filter_backtrace(error.backtrace).join(&quot;\n&quot;)
        tc_output &quot;testFailed name='#{method}' message='#{tc_escape(error.message)}' details='#{tc_escape(trace)}'&quot;
      else
        @errors += 1
        trace = MiniTest::filter_backtrace(error.backtrace).join(&quot;\n&quot;)
        tc_output &quot;testFailed name='#{method}' message='#{tc_escape(error.message)}' details='#{tc_escape(trace)}'&quot;
    end
      
    error[0,1]  
  end
[/ruby]
</div>

This method is called by MiniTest whenever a test has failed... ignoring (or skipping in the MiniTest terminology) a test is a 'failure' (and i can't really argue with that).  And obviously, both assertion failures or runtime exceptions are considered to be test failures as well.  In either of these 3 cases, the puke method is called and it is supposed to output something to the user to notify him/her of the problems.  So i basically just took the existing code, and modified it so its output would be in the format that TeamCity can work with.  Next up is the run_test_suites method, which is responsible for, you guessed it, running the tests in the various test suites.

<div>
[ruby]
  def run_test_suites(filter=/./)
    @test_count, @assertion_count = 0, 0
    old_sync, @@out.sync = @@out.sync, true if @@out.respond_to? :sync=
    TestCase.test_suites.each do |suite|
      tc_output &quot;testSuiteStarted name='#{suite}'&quot;
      suite.test_methods.grep(filter).each do |test|
        inst = suite.new test
        inst._assertions = 0
        tc_output &quot;testStarted name='#{test}'&quot;
        @start_time = Time.now
        result = inst.run(self)
        duration = &quot;%f&quot; % ((Time.now - @start_time)*1000)
        tc_output &quot;testFinished name='#{test}' duration='#{duration}'&quot;
        @test_count += 1
        @assertion_count += inst._assertions
      end
    end
    @@out.sync = old_sync if @@out.respond_to? :sync=
    [@test_count, @assertion_count]
  end
[/ruby]
</div>

Again, i just took the existing code and changed its output so that TeamCity can work with it. 

And the final result is this:

<img src="http://davybrion.com/blog/wp-content/uploads/2010/10/ci.png" alt="" title="ci" width="238" height="74" class="aligncenter size-full wp-image-2760" />

As you can see, build #2 didn't give you any feedback on the tests, even though they were being executed properly.  Build #3 reported 2 failing tests, which were my temporary test cases to see how failed assertions or actual errors would be reported by TeamCity.  Build #4 reports that all tests passed.  In case you're interested, our 'build script' looks like this:

<div>
[code]
..\ironruby\bin\dotnet\ir -w tests\suite.rb
[/code]
</div>

And that's it... pretty simple, no?

That just goes to show that while monkey patching is considered by a lot of people to be 'evil', it certainly has its benefits from time to time.  I'm not saying you should use it as much as possible.  But when it makes sense to do so, and if you're aware of the downsides and the pitfalls, then there's nothing wrong with it at all.  Though it does require a language that treats you like an adult and expects you to know what you're doing ;)