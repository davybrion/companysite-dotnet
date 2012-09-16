using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web.Hosting;
using Nancy;
using ServiceStack.Text;

namespace ThatExtraMile.be
{
    public class SiteModule : NancyModule
    {
        private static readonly ContentTransformer ContentTransformer;
        private static readonly Dictionary<string, BlogPost> PostsPerLink;
        private static List<BlogPost> IndexedListOfPosts; 

        static SiteModule()
        {
            ContentTransformer = new ContentTransformer();

            PostsPerLink = JsonSerializer.DeserializeFromString<BlogPost[]>(
                File.ReadAllText(HostingEnvironment.MapPath("~/content/blog_metadata.json")))
                .ToDictionary(b => b.Link, b => b);

            IndexedListOfPosts = new List<BlogPost>(PostsPerLink.Values);
        }

        public SiteModule()
        {
            Get["/blog/(?<year>[\\d]{4})/(?<month>[\\d]{4})/{slug}"] = p => RenderPost(p);


            Get["/contracting"] = p => RenderMarkdown("Contracting", "Contracting", "contracting");
            Get["/consulting"] = p => RenderMarkdown("Consulting", "Consulting", "consulting");
            Get["/training"] = p => RenderMarkdown("Training", "Training", "training");
            Get["/training/nhibernate"] = p => RenderMarkdown("NHibernate Training", "Training", "nhibernate_training");
            Get["/reviews"] = p => RenderMarkdown("Reviews", "Reviews", "reviews");
            Get["/"] = p => RenderMarkdown("", "Home", "home");
        }

        private Response RenderPost(dynamic p)
        {
            var post = PostsPerLink[(string)string.Format("/blog/{0}/{1}/{2}/", p.year, p.month, p.slug)];
            var indexOfPost = IndexedListOfPosts.IndexOf(post);
            var previousPost = indexOfPost != 0 ? IndexedListOfPosts[indexOfPost - 1] : null;
            var nextPost = indexOfPost != IndexedListOfPosts.Count - 1 ? IndexedListOfPosts[indexOfPost + 1] : null;

            var contentReference = String.Format("{0}-{1}-{2}", p.year, p.month, p.slug);

            return View["BlogPostPage", new
                {
                    Title = post.Title,
                    Post = post,
                    PreviousPost = previousPost,
                    NextPost = nextPost,
                    Section = "Blog",
                    Content = ContentTransformer.GetTransformedContent(contentReference)
                }];
        }

        private Response RenderMarkdown(string title, string section, string contentName)
        {
            return View["NormalContent", new
                {
                    Title = title,
                    Section = section,
                    Html = ContentTransformer.GetTransformedContent(contentName)
                }];
        }
    }
}