using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web.Hosting;
using Nancy;
using ServiceStack.Text;
using ThatExtraMile.be.ViewModels;

namespace ThatExtraMile.be
{
    public class SiteModule : NancyModule
    {
        private static readonly ContentTransformer ContentTransformer;
        private static readonly Dictionary<string, BlogPost> PostsPerLink;
        private static List<BlogPost> IndexedListOfPosts;
        private static List<BlogPost> ReversedIndexedListOfPosts; 

        static SiteModule()
        {
            ContentTransformer = new ContentTransformer();

            PostsPerLink = JsonSerializer.DeserializeFromString<BlogPost[]>(
                File.ReadAllText(HostingEnvironment.MapPath("~/content/blog_metadata.json")))
                .ToDictionary(b => b.Link, b => b);

            IndexedListOfPosts = new List<BlogPost>(PostsPerLink.Values);
            ReversedIndexedListOfPosts = new List<BlogPost>(IndexedListOfPosts);
            ReversedIndexedListOfPosts.Reverse();
        }

        public SiteModule()
        {
            Get["/contracting"] = p => RenderMarkdown("Contracting", "Contracting", "contracting");
            Get["/consulting"] = p => RenderMarkdown("Consulting", "Consulting", "consulting");
            Get["/training"] = p => RenderMarkdown("Training", "Training", "training");
            Get["/training/nhibernate"] = p => RenderMarkdown("NHibernate Training", "Training", "nhibernate_training");
            Get["/reviews"] = p => RenderMarkdown("Reviews", "Reviews", "reviews");
            Get["/"] = p => RenderMarkdown("", "Home", "home");

            //Get["/blog"] = p => Render
            Get["/blog/(?<year>[\\d]{4})/(?<month>[\\d]{4})/{slug}"] = p => RenderPost(p);
            Get["/blog/page/(?<year>[\\d]*)"] = p => RenderPostArchivePage(p.year);
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

        private Response RenderPost(dynamic p)
        {
            var post = PostsPerLink[(string)string.Format("/blog/{0}/{1}/{2}/", p.year, p.month, p.slug)];
            var indexOfPost = IndexedListOfPosts.IndexOf(post);
            var previousPost = indexOfPost != 0 ? IndexedListOfPosts[indexOfPost - 1] : null;
            var nextPost = indexOfPost != IndexedListOfPosts.Count - 1 ? IndexedListOfPosts[indexOfPost + 1] : null;

            return View["BlogPostPage", new
                {
                    Title = post.Title,
                    Post = post,
                    PreviousPost = previousPost,
                    NextPost = nextPost,
                    Section = "Blog",
                    Content = ContentTransformer.GetTransformedContent(post.GetContentReference())
                }];
        }

        private Response RenderPostArchivePage(int page)
        {
            const int pageSize = 5;
            var posts = ReversedIndexedListOfPosts.Skip((page - 1) * pageSize).Take(pageSize);

            var postModels = posts.Select(p => new BlogPostViewModel()
                        {Post = p, Content = ContentTransformer.GetTransformedContent(p.GetContentReference())});

            return View["BlogPostsOverviewPage", new
                {
                    Title = string.Format("Blog archive, page {0}", page),
                    PostModels = postModels,
                    Section = "Blog"
                }];
        }
    }
}