using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Text;
using System.Threading;
using NUnit.Framework;
using Teleopti.Logging;
using Teleopti.Logging.Core;

namespace Teleopti.Messaging.Tests
{

    [TestFixture]
    public class LoggingTest
    {

        [SetUp]
        public void SetUp()
        {
        }

        [Test]
        public void LoggingToFileTests()
        {
            string fileName = Path.GetTempPath();
            fileName = String.Format(CultureInfo.InvariantCulture, "{0}{1}", fileName, "logtest.csv");
            BaseLogger logger = BaseLogger.GetInstance("Teleopti.Logging.Core.CustomTraceListener", fileName, false);
            logger.WriteLine(EventLogEntryType.Error, GetType(), "Testing Error");
            logger.WriteLine(EventLogEntryType.Warning, GetType(), "Testing Warning");
            logger.WriteLine(EventLogEntryType.Information, GetType(), "Testing Info");
            string newFormat = fileName.Replace("logtest", "testlog");
            File.Copy(fileName, newFormat, true);
            FileStream file = new FileStream(newFormat, FileMode.Open, FileAccess.Read);
            file.Position = 0;
            byte[] buffer = new byte[file.Length];
            file.Read(buffer, 0, Convert.ToInt32(buffer.Length));
            string logFile = GetString(buffer);
            string[] logFileRows = logFile.Split(new char[]{'\r','\n'});
            foreach (string logFileRow in logFileRows)
            {
                Console.WriteLine(logFileRow);
                Assert.IsNotNull(logFileRow);
            }
        }

        [Test]
        public void LoggingToDatabaseTests()
        {
            BaseLogger.Instance.WriteLine(EventLogEntryType.Error, GetType(), "Testing Error");
            BaseLogger.Instance.WriteLine(EventLogEntryType.Warning, GetType(), "Testing Warning");
            BaseLogger.Instance.WriteLine(EventLogEntryType.Information, GetType(), "Testing Info");
            Thread.Sleep(1000);
        }


        public string GetString(byte[] values)
        {
            StringBuilder sb = new StringBuilder();
            foreach (byte b in values)
            {
                char c = Convert.ToChar(b, CultureInfo.InvariantCulture);
                sb.Append(c);
            }
            return sb.ToString();
        }

        [Test]
        public void RollOverTests()
        {
            CustomTraceListener customTraceListener  = new CustomTraceListener();
            customTraceListener.Initialise("RollOverTests.csv", 0.025);
            DateTime now = DateTime.Now;
            while(now.AddMinutes(0.05) > DateTime.Now)
            {
                customTraceListener.Write("Testing");
                customTraceListener.WriteLine(" trace listner ...");
                customTraceListener.WriteLine("Tracing some more ...", "Category");
            }
            customTraceListener.Dispose();
        }

        [TearDown]
        public void TearDown()
        {
        }

    }
}
