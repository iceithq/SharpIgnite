using System;
using System.Reflection;
using log4net;
using log4net.Config;

namespace SharpIgnite
{
    public static class LoggingHelper
    {
        static ILog log;
        
        static LoggingHelper()
        {
            XmlConfigurator.Configure();
            log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        }
        
        public static void Error(string message)
        {
            log.Error(message);
        }
        
        public static void Debug(string message)
        {
            log.Debug(message);
        }
        
        public static void Info(string message)
        {
            log.Info(message);
        }
    }
}
