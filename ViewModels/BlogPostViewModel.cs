namespace ThatExtraMile.be.ViewModels
{
    public class BlogPostViewModel
    {
        public BlogPost Post { get; set; }
        public string Content { get; set; }
        public bool TitleAsLink { get; set; }
        public bool ShowDisqusCommentCount { get; set; }
    }
}