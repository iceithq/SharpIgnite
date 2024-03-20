using System;
using System.Web;

namespace SharpIgnite
{
    public class Input
    {
        public string Post(string key)
        {
            return HttpContext.Current.Request.Form[key];
        }
        
        public string Get(string key)
        {
            return HttpContext.Current.Request.QueryString[key];
        }
    }
    
    public class Output
    {
        bool profiler;
        
        public void EnableProfiler(bool profiler)
        {
            this.profiler = profiler;
        }
    }
}
