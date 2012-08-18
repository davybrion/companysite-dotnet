using Nancy;

namespace ThatExtraMile.be
{
    public class SiteModule : NancyModule
    {
        private static readonly ContentProvider _contentProvider = new ContentProvider();

        public SiteModule()
        {
            Get["/"] = p => RenderMarkdown("", "Home", "home");
            Get["/contracting"] = p => RenderMarkdown("Contracting", "Contracting", "contracting");
            Get["/consulting"] = p => RenderMarkdown("Consulting", "Consulting", "consulting");
            Get["/training"] = p => RenderMarkdown("Training", "Training", "training");
            Get["/training/nhibernate"] = p => RenderMarkdown("NHibernate Training", "Training", "nhibernate_training");
            Get["/reviews"] = p => RenderMarkdown("Reviews", "Reviews", "reviews");
        }

        private Response RenderMarkdown(string title, string section, string contentName)
        {
            return View["MarkDown", new
                {
                    Title = title,
                    Section = section,
                    Html = _contentProvider.GetContent(contentName)
                }];
        }
    }
}