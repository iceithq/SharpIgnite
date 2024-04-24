using System;
using NUnit.Framework;

namespace SharpIgnite.Tests
{
    [TestFixture]
    public class LoggingHelperTests
    {
        [Test]
        public void TestMethod()
        {
            LoggingHelper.Info("This is info");
            LoggingHelper.Debug("This is debug");
            LoggingHelper.Error("This is error");
        }
    }
}
