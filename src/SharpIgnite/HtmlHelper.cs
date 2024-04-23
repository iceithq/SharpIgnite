using System;
using System.Collections.Generic;

namespace SharpIgnite
{
    public static class HtmlHelper
    {
        public static string Anchor(string text, string url)
        {
            return Anchor(text, url, "");
        }
        public static string Anchor(string text, string url, string attributes)
        {
            return Anchor(text, url, attributes, false);
        }
        
        public static string Anchor(string text, string url, string attributes, bool random)
        {
            if (random) {
                url = string.Format("{0}?Rnd={1}", url, NumberHelper.Random());
                return string.Format("<a href='{0}' {1}>{2}</a>", url, attributes, text);
            } else {
                return string.Format("<a href='{0}' {1}>{2}</a>", url, attributes, text);
            }
        }
        
        public static string Image(string url)
        {
            return Image(url, "");
        }
        
        public static string Image(string url, string alt)
        {
            return string.Format("<img src='{0}' alt='{1}'/>", url, alt);
        }
        
        public static string Image(string url, string alt, int width, int height)
        {
            return string.Format("<img src='{0}' width='{1}' height='{2}' alt='{3}'/>", url, width, height, alt);
        }
    }
}
