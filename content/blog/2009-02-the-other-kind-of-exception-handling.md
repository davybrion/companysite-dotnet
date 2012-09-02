<div>
[csharp]
    public class Developer
    {
        // lots of other methods here....
 
        public void FigureOutException(Exception e)
        {
            var exception = GetMostInformativeStuff(e);
            Read(exception.Message);
            GoToThePartOfCodeThatCausedTheException(exception.StackTrace);
 
            if (YouUnderstandTheProblem())
            {
                FixIt();
            }
            else
            {
                AskForHelpFromCoworkerOrGoogle();
            }
        }
 
        private Exception GetMostInformativeStuff(Exception exception)
        {
            if (exception.InnerException != null)
            {
                return GetMostInformativeStuff(exception.InnerException);
            }
 
            return exception;
        }
 
        private void Read(string message)
        {
            // in most cases, the exception message is pretty clear as to what the problem is
            // this isn't always the case though, but you should at least read the message!
        }
 
        private void GoToThePartOfCodeThatCausedTheException(string stackTrace)
        {
            // open the first file in our code that occurs in the stacktrace, beginning from the top and look at the
            // line number that is listed within that line... this is usually the first place to look
        }
 
        private bool YouUnderstandTheProblem()
        {
            // if the exception message makes sense, and the stacktrace points to line that threw the exception, you
            // will be able to figure it out (in most cases)
 
            // not really correct randomness, but it shouldn't be truly random in real life either
            return new Random().Next(0, 2) == 1;
        }
 
        private void FixIt()
        {
            // ...
        }
 
        private void AskForHelpFromCoworkerOrGoogle()
        {
            // ...
        }
    }
[/csharp]
</div>

Shouldn't this be common sense?