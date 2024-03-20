using System;

namespace SharpIgnite
{
    public static class HtmlHelper
    {
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
        
//        public static string AnchorSpan(string text, string url)
//        {
//            return AnchorSpan(text, url, "");
//        }
//        
//        public static string AnchorSpan(string text, string url, string attributes)
//        {
//            return AnchorSpan(text, url, attributes, false);
//        }
//        
//        public static string AnchorSpan(string text, string url, string attributes, bool random)
//        {
//            if (random) {
//                url = string.Format("{0}?Rnd={1}", url, NumberHelper.Random());
//                return string.Format("<a href='{0}' {1}><span>{2}</span></a>", url, attributes, text);
//            } else {
//                return string.Format("<a href='{0}' {1}><span>{2}</span></a>", url, attributes, text);
//            }
//        }
//        
//        public static string AnchorImage(string url, string image)
//        {
//            return AnchorImage(url, image, false);
//        }
//
//        public static string AnchorImage(string url, string image, string attributes)
//        {
//            return string.Format("<a href='{0}' {2}><img src='{1}'></a>", url, image, attributes);
//        }
//        
//        public static string AnchorImage(string url, string image, bool random)
//        {
//            if (random) {
//                return string.Format("<a href='{0}&Rnd={2}'><img src='{1}'></a>", url, image, NumberHelper.Random());
//            }
//            return string.Format("<a href='{0}'><img src='{1}'></a>", url, image);
//        }
        
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
        
//        public static string DropDownList<T>(IList<T> lists)
//        {
//            StringBuilder s = new StringBuilder();
//            s.AppendLine("<ul>");
//            foreach (var l in lists) {
//                s.AppendLine(string.Format("<li>{0}</li>", l));
//            }
//            s.AppendLine("</ul>");
//            return s.ToString();
//        }
//
//        public static void Write(object obj, HttpResponse reponse)
//        {
//            if (obj is MemoryStream) {
//                reponse.BinaryWrite(((MemoryStream)obj).ToArray());
//                reponse.End();
//            } else if (obj is string) {
//                reponse.Write((string)obj);
//            }
//        }
//        
//        public static void AddHeaderIf(bool condition, string name, string value, HttpResponse response)
//        {
//            if (condition) {
//                response.AddHeader(name, value);
//            }
//        }
    }
}
