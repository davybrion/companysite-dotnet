using System.Collections.Generic;

namespace ThatExtraMile.be.ViewModels
{
    public class BlogPostsOverviewViewModel
    {
        public string Title { get; set; }
        public IEnumerable<BlogPostViewModel> PostModels { get; set; }
        public string Section { get; set; }
        public int? PreviousPageIndex { get; set; }
        public int? NextPageIndex { get; set; }
    }
}