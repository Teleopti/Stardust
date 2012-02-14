using log4net;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Common;

namespace Teleopti.Ccc.DomainTest.Common
{
    [TestFixture]
    public class LogWriterTest
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
        [Test]
        public void VerifyTwoLogWriterSharesSameLogObject()
        {
            LogWriter<object> writer1 = new LogWriter<object>();
            LogWriter<object> writer2 = new LogWriter<object>();
            Assert.AreSame(writer1.Log, writer2.Log);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
        [Test]
        public void VerifyWriterOfDifferentTypeHaveDifferentLogObject()
        {
            LogWriter<Person> writer1 = new LogWriter<Person>();
            LogWriter<object> writer2 = new LogWriter<object>();
            Assert.AreNotSame(writer1.Log, writer2.Log);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic"), 
        System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters", MessageId = "Teleopti.Ccc.Domain.Common.LogWriter<System.Object>.LogInfo(System.String)")]
        [Test]
        public void VerifyInnerLogCalled()
        {
            MockRepository mockRepository = new MockRepository();
            ILog innerLogger = mockRepository.StrictMock<ILog>();
            LogWriter<object> writer1 = new LogWriter<object>();
            LogWriter<object>.SetExplicitLog(innerLogger);
            string message = "Log Message";
            
            using(mockRepository.Record())
            {
                innerLogger.Info(message);
            }
            using(mockRepository.Playback())
            {
                writer1.LogInfo(message);
            }
        }
    }
}
