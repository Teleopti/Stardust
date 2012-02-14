using System;
using NUnit.Framework;
using Teleopti.Analytics.Etl.Common.Database.EtlLogs;
using Teleopti.Analytics.Etl.Common.Database.EtlSchedules;
using Teleopti.Analytics.Etl.Interfaces.Common;
using Teleopti.Analytics.Etl.Interfaces.Transformer;

namespace Teleopti.Analytics.Etl.CommonTest.Database.EtlSchedules
{
    [TestFixture]
    public class EtlScheduleTest
    {
        private EtlScheduleCollection _etlScheduleCollection;

        [SetUp]
        public void Setup()
        {
            //IScheduleRepository scheduleRepository = new RepositoryStub();
            ILogRepository logRepository = new RepositoryStub();

            IEtlLogCollection etlLogCollection = new EtlLogCollection(logRepository);
            _etlScheduleCollection = new EtlScheduleCollection(new RepositoryStub(), etlLogCollection, DateTime.Now);
        }

        [Test]
        public void VerifyJobSchedule()
        {
            // Occurs once per day
            IEtlSchedule schedule = _etlScheduleCollection[0];
            Assert.AreEqual(1, schedule.ScheduleId);
            Assert.AreEqual("OccursDaily", schedule.ScheduleName);
            Assert.AreEqual(true, schedule.Enabled);
            Assert.AreEqual(JobScheduleType.OccursDaily, schedule.ScheduleType);
            Assert.AreEqual(60, schedule.OccursOnceAt);
            Assert.AreEqual("Intraday", schedule.JobName);
            Assert.AreEqual(-14, schedule.RelativePeriodStart);
            Assert.AreEqual(14, schedule.RelativePeriodEnd);
            Assert.AreEqual(1, schedule.DataSourceId);
            Assert.AreEqual("Occurs daily at x.", schedule.Description);

            // Occurs daily every x minute
            schedule = _etlScheduleCollection[1];
            Assert.AreEqual(2, schedule.ScheduleId);
            Assert.AreEqual("Periodic", schedule.ScheduleName);
            Assert.AreEqual(true, schedule.Enabled);
            Assert.AreEqual(JobScheduleType.Periodic, schedule.ScheduleType);
            Assert.AreEqual(15, schedule.OccursEveryMinute);
            Assert.AreEqual(180, schedule.OccursEveryMinuteStartingAt);
            Assert.AreEqual(1430, schedule.OccursEveryMinuteEndingAt);
            Assert.AreEqual("Forecast", schedule.JobName);
            Assert.AreEqual(-7, schedule.RelativePeriodStart);
            Assert.AreEqual(7, schedule.RelativePeriodEnd);
            Assert.AreEqual(1, schedule.DataSourceId);
            Assert.AreEqual("Occurs daily every x minute.", schedule.Description);
        }

        [Test]
        public void VerifyJobSchedulePeriod()
        {
            IEtlSchedule schedule1 = _etlScheduleCollection[0];
            IEtlSchedule schedule2 = _etlScheduleCollection[1];

            Assert.AreEqual(4, schedule1.RelativePeriodCollection.Count);
            Assert.AreEqual(1, schedule2.RelativePeriodCollection.Count);

            Assert.AreEqual(JobCategoryType.Schedule, schedule1.RelativePeriodCollection[0].JobCategory);
            Assert.AreEqual(-7, schedule1.RelativePeriodCollection[0].RelativePeriod.Minimum);
            Assert.AreEqual(7, schedule1.RelativePeriodCollection[0].RelativePeriod.Maximum);
        }

       

        [Test]
        public void VerifySetScheduleIdOnPersistedItem()
        {
            //You should ONLY be able to change ScheduleId on a newly persisted item that has ScheuleId = -1
            IEtlSchedule etlScheduleCannotChange = _etlScheduleCollection[0];
            IEtlSchedule etlScheduleCanChange = _etlScheduleCollection[2];
            const int newScheduleId = 9999;
            etlScheduleCannotChange.SetScheduleIdOnPersistedItem(newScheduleId);
            etlScheduleCanChange.SetScheduleIdOnPersistedItem(newScheduleId);

            Assert.AreNotEqual(newScheduleId, etlScheduleCannotChange.ScheduleId);
            Assert.AreEqual(newScheduleId, etlScheduleCanChange.ScheduleId);


        }
    }
}