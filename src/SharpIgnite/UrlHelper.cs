﻿using System;
using System.Web;

namespace SharpIgnite
{
    public class UrlHelper
    {
        public static void Redirect(string url)
        {
            Redirect(url, false);
        }

        public static void Redirect(string url, bool random)
        {
            if (random) {
                HttpContext.Current.Response.Redirect(string.Format("{0}?Rnd={1}", url, NumberHelper.Random()), true);
            } else {
                HttpContext.Current.Response.Redirect(url, true);
            }
        }
    }

    public class UrlEventArgs : EventArgs
    {
        public string Url { get; set; }

        public UrlEventArgs(string url)
        {
            this.Url = url;
        }

        public UrlEventArgs() : this("")
        {
        }
    }
}
