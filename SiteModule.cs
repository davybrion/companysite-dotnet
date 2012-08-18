using Nancy;

namespace ThatExtraMile.be
{
    public class SiteModule : NancyModule
    {
        public SiteModule()
        {
            Get["/"] = parameters => View["MarkDown", new
                {
                    Title = "Hello world",
                    Section = "Home",
                    Html = "Hello world!"
                }];
        }
    }
}