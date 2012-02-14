using System;
using NUnit.Framework;
using Teleopti.Analytics.Etl.Common.Database.EtlLogs;
using Teleopti.Analytics.Etl.Interfaces.Common;

namespace Teleopti.Analytics.Etl.CommonTest.Database.EtlLogs
{
    [TestFixture]
    public class LogTest
    {
        

        [SetUp]
        public void Setup()
        {
            _etlLog = new EtlLog(3, new DateTime(2001, 01, 01, 0, 0, 0, DateTimeKind.Utc),
                                 new DateTime(2002, 02, 02, 0, 0, 0, DateTimeKind.Utc));
        }

        

        private IEtlLog _etlLog;

        [Test]
        public void VerifyEtlLog()
        {
            Assert.AreEqual(new DateTime(2001, 01, 01, 0, 0, 0, DateTimeKind.Utc), _etlLog.StartTime);
            Assert.AreEqual(new DateTime(2002, 02, 02, 0, 0, 0, DateTimeKind.Utc), _etlLog.EndTime);
            Assert.AreEqual(3, _etlLog.ScheduleId);
        }
    }
}