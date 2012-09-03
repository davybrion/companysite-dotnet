using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Web.Hosting;
using MarkdownSharp;
using ServiceStack.Text;

namespace ThatExtraMile.be
{
    public class ContentProvider
    {
        private readonly ConcurrentDictionary<string, string> _content = new ConcurrentDictionary<string, string>();   
        private readonly Markdown _markdown = new Markdown();
        private readonly IEnumerable<BlogPost> _blogPosts;

        public ContentProvider()
        {
            _blogPosts = JsonSerializer.DeserializeFromString<BlogPost[]>(File.ReadAllText(HostingEnvironment.MapPath("~/content/blog_metadata.json")));
        }

        public string GetContent(string contentName)
        {
#if !DEBUG
            if (!_content.ContainsKey(contentName))
            {
#endif
                var split = contentName.Split('-');
                var fileName = split.Length == 1 ? HostingEnvironment.MapPath("~/content/" + contentName + ".md") : 
                                                   HostingEnvironment.MapPath("~/content/blog/" + split.Join("-") + ".md");

                if (!File.Exists(fileName)) return null;
#if DEBUG
                return _markdown.Transform(File.ReadAllText(fileName));
#else
                _content[contentName] = _markdown.Transform(File.ReadAllText(fileName));
            }

            return _content[contentName];
#endif
        }
    }
}