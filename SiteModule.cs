using Nancy;

namespace ThatExtraMile.be
{
    public class SiteModule : NancyModule
    {
        public SiteModule()
        {
            Get["/"] = parameters => "Hello world";
        }
    }
}