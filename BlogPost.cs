using System;

namespace ThatExtraMile.be
{
    public class BlogPost
    {
        public string Title { get; set; }
        public string Link { get; set; }
        public DateTime Date { get; set; }
        public string[] Categories { get; set; }

        public string GetContentReference()
        {
            var temp = Link.Substring(6) // skip the '/blog/' part
                .Replace('/', '-'); // this also replaces the closing / with a -, but we'll fix that in the next step
            return temp.Remove(temp.Length - 1); // remove last -
        }
    }
}