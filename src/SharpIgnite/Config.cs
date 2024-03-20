using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;

namespace SharpIgnite
{
    public static class Config
    {
        public static string Item(string key)
        {
            return ConfigurationManager.AppSettings[key];
        }
    }
}