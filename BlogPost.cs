using System;

namespace ThatExtraMile.be
{
    public class BlogPost
    {
        public string Title { get; set; }
        public string Link { get; set; }
        public DateTime Date { get; set; }
        public string DisqusThreadId { get; set; }
        public string[] Categories { get; set; }
    }
}