using System;
using NUnit.Framework;
using Teleopti.Analytics.Etl.Common.JobLog;
using Teleopti.Analytics.Etl.Interfaces.Common;

namespace Teleopti.Analytics.Etl.CommonTest.Database.EtlLogs
{
    [TestFixture]
    public class LogTest
    {
        

        [SetUp]
        public void Setup()
        {
            _etlJobLog = new EtlJobLog(3, new DateTime(2001, 01, 01, 0, 0, 0, DateTimeKind.Utc),
                                 new DateTime(2002, 02, 02, 0, 0, 0, DateTimeKind.Utc));
        }

        

        private IEtlJobLog _etlJobLog;

        [Test]
        public void VerifyEtlLog()
        {
            Assert.AreEqual(new DateTime(2001, 01, 01, 0, 0, 0, DateTimeKind.Utc), _etlJobLog.StartTime);
            Assert.AreEqual(new DateTime(2002, 02, 02, 0, 0, 0, DateTimeKind.Utc), _etlJobLog.EndTime);
            Assert.AreEqual(3, _etlJobLog.ScheduleId);
        }
    }
}