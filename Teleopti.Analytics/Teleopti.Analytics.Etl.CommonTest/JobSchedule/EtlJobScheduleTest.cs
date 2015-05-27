using System;
using NUnit.Framework;
using Teleopti.Analytics.Etl.Common.Interfaces.Common;
using Teleopti.Analytics.Etl.Common.Interfaces.Transformer;
using Teleopti.Analytics.Etl.Common.JobLog;
using Teleopti.Analytics.Etl.Common.JobSchedule;
using Teleopti.Analytics.Etl.CommonTest.Infrastructure;

namespace Teleopti.Analytics.Etl.CommonTest.JobSchedule
{
    [TestFixture]
    public class EtlJobScheduleTest
    {
        private EtlJobScheduleCollection _etlJobScheduleCollection;

        [SetUp]
        public void Setup()
        {
            IJobLogRepository jobLogRepository = new RepositoryStub();

            IEtlJobLogCollection etlJobLogCollection = new EtlJobLogCollection(jobLogRepository);
            _etlJobScheduleCollection = new EtlJobScheduleCollection(new RepositoryStub(), etlJobLogCollection, DateTime.Now);
        }

        [Test]
        public void VerifyJobSchedule()
        {
            // Occurs once per day
            IEtlJobSchedule jobSchedule = _etlJobScheduleCollection[0];
            Assert.AreEqual(1, jobSchedule.ScheduleId);
            Assert.AreEqual("OccursDaily", jobSchedule.ScheduleName);
            Assert.AreEqual(true, jobSchedule.Enabled);
            Assert.AreEqual(JobScheduleType.OccursDaily, jobSchedule.ScheduleType);
            Assert.AreEqual(60, jobSchedule.OccursOnceAt);
            Assert.AreEqual("Intraday", jobSchedule.JobName);
            Assert.AreEqual(-14, jobSchedule.RelativePeriodStart);
            Assert.AreEqual(14, jobSchedule.RelativePeriodEnd);
            Assert.AreEqual(1, jobSchedule.DataSourceId);
            Assert.AreEqual("Occurs daily at x.", jobSchedule.Description);

            // Occurs daily every x minute
            jobSchedule = _etlJobScheduleCollection[1];
            Assert.AreEqual(2, jobSchedule.ScheduleId);
            Assert.AreEqual("Periodic", jobSchedule.ScheduleName);
            Assert.AreEqual(true, jobSchedule.Enabled);
            Assert.AreEqual(JobScheduleType.Periodic, jobSchedule.ScheduleType);
            Assert.AreEqual(15, jobSchedule.OccursEveryMinute);
            Assert.AreEqual(180, jobSchedule.OccursEveryMinuteStartingAt);
            Assert.AreEqual(1430, jobSchedule.OccursEveryMinuteEndingAt);
            Assert.AreEqual("Forecast", jobSchedule.JobName);
            Assert.AreEqual(-7, jobSchedule.RelativePeriodStart);
            Assert.AreEqual(7, jobSchedule.RelativePeriodEnd);
            Assert.AreEqual(1, jobSchedule.DataSourceId);
            Assert.AreEqual("Occurs daily every x minute.", jobSchedule.Description);
        }

        [Test]
        public void VerifyJobSchedulePeriod()
        {
            IEtlJobSchedule schedule1 = _etlJobScheduleCollection[0];
            IEtlJobSchedule schedule2 = _etlJobScheduleCollection[1];

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
            IEtlJobSchedule etlJobScheduleCannotChange = _etlJobScheduleCollection[0];
            IEtlJobSchedule etlJobScheduleCanChange = _etlJobScheduleCollection[2];
            const int newScheduleId = 9999;
            etlJobScheduleCannotChange.SetScheduleIdOnPersistedItem(newScheduleId);
            etlJobScheduleCanChange.SetScheduleIdOnPersistedItem(newScheduleId);

            Assert.AreNotEqual(newScheduleId, etlJobScheduleCannotChange.ScheduleId);
            Assert.AreEqual(newScheduleId, etlJobScheduleCanChange.ScheduleId);


        }
    }
}