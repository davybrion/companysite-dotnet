using System.Collections.Concurrent;
using System.IO;
using System.Web.Hosting;
using MarkdownSharp;

namespace ThatExtraMile.be
{
    public class ContentProvider
    {
        private readonly ConcurrentDictionary<string, string> _content = new ConcurrentDictionary<string, string>();   
        private readonly Markdown _markdown = new Markdown();

        public string GetContent(string contentName)
        {
            if (!_content.ContainsKey(contentName))
            {
                _content[contentName] =
                    _markdown.Transform(File.ReadAllText(HostingEnvironment.MapPath("~/content/" + contentName + ".md")));
            }

            return _content[contentName];
        }
    }
}