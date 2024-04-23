using System;
using System.Web;

namespace SharpIgnite
{
    public class Input
    {
        public Input()
        {
        }
        
        public string Post(string key)
        {
            return HttpContext.Current.Request.Form[key];
        }
        
        public string Get(string key)
        {
            return HttpContext.Current.Request.QueryString[key];
        }

        public string Get(string key, string defaultValue)
        {
            if (HttpContext.Current.Request.QueryString[key] != null) {
                return HttpContext.Current.Request.QueryString[key];
            }
            return defaultValue;
        }
    }
    
    public class Output
    {
        internal bool profiler;
        string output;
        
        public void EnableProfiler(bool profiler)
        {
            this.profiler = profiler;
        }
        
        public void AppendOutput(string output)
        {
            this.output += output;
        }
        
        public string GetOutput()
        {
            return this.output;
        }
        
        public void SetOutput(string output)
        {
            this.output = output;
        }
    }
}
