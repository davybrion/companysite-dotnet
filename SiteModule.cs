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
        private static readonly List<BlogPost> IndexedListOfPosts;
        private static readonly List<BlogPost> ReversedIndexedListOfPosts;

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

            Get["/blog"] = p => RenderBlogPage();
            Get["/blog/new-here"] = p => RenderMarkdown("New here?", "Blog", "new_here");
            Get["/blog/recommended-books"] = p => RenderMarkdown("Recommended books", "Blog", "recommended_books");
            Get["/blog/(?<year>[\\d]{4})/(?<month>[\\d]{4})/{slug}"] = p => RenderPost(p);
            Get["/blog/page/(?<page>[\\d]*)"] = p => RenderPostArchivePage(p.page);
            Get["/blog/category/{category}"] = p => RenderCategoryPage(p.category, 1);
            Get["/blog/category/{category}/page/(?<page>[\\d]*)"] = p => RenderCategoryPage(p.category, p.page);
        }

        private dynamic RenderMarkdown(string title, string section, string contentName)
        {
            return View["NormalContentPage", new
            {
                Title = title,
                Section = section,
                Html = ContentTransformer.GetTransformedContent(contentName)
            }];
        }

        private dynamic RenderPost(dynamic p)
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

        private dynamic RenderBlogPage()
        {
            var model = BuildBlogPostsOverviewViewModelForPage(ReversedIndexedListOfPosts, 1, "Blog archive", "/blog/page");
            model.IntroductionContent = ContentTransformer.GetTransformedContent("blog_archive");
            return View["BlogPostsOverviewPage", model];
        }

        private dynamic RenderPostArchivePage(int page)
        {
            return View["BlogPostsOverviewPage", BuildBlogPostsOverviewViewModelForPage(ReversedIndexedListOfPosts, page, "Blog archive", "/blog/page")];
        }

        private dynamic RenderCategoryPage(string category, int page)
        {
            return View["BlogPostsOverviewPage", BuildBlogPostsOverviewViewModelForPage(ReversedIndexedListOfPosts.Where(p => p.Categories.Contains(category)), 
                page, string.Format("Category archive: {0}", category), string.Format("/blog/category/{0}/page", category))];
        }

        private static BlogPostsOverviewViewModel BuildBlogPostsOverviewViewModelForPage(IEnumerable<BlogPost> posts, int page, string title, string nextAndPreviousPageRoute)
        {
            const int pageSize = 5;
            var totalNumberOfPosts = posts.Count();
            posts = posts.Skip((page - 1) * pageSize).Take(pageSize);

            var postModels = posts.Select(p => new BlogPostViewModel()
                {
                    Post = p,
                    Content = ContentTransformer.GetTransformedContent(p.GetContentReference()),
                    TitleAsLink = true,
                    ShowDisqusCommentCount = true
                });

            return new BlogPostsOverviewViewModel
                {
                    Title = string.Format(title + ", page {0}", page),
                    PostModels = postModels,
                    Section = "Blog",
                    PreviousPageIndex = page == 1 ? null : (int?)(page - 1),
                    NextPageIndex = (totalNumberOfPosts / (double)pageSize) <= page ? null : (int?)(page + 1),
                    NextAndPreviousPageRoute = nextAndPreviousPageRoute
                };
        }
    }
}