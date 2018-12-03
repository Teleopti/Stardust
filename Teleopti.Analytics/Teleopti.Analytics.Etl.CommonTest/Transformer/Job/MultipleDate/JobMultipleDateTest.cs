using System;
using NUnit.Framework;
using Teleopti.Analytics.Etl.Common.Interfaces.Transformer;
using Teleopti.Analytics.Etl.Common.Transformer.Job.MultipleDate;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;


namespace Teleopti.Analytics.Etl.CommonTest.Transformer.Job.MultipleDate
{
    [TestFixture]
    public class JobMultipleDateTest
    {
        private IJobMultipleDate _target;
        private readonly TimeZoneInfo _timeZone = TimeZoneInfo.FindSystemTimeZoneById("W. Europe Standard Time");

        [SetUp]
        public void Setup()
        {
            _target = new JobMultipleDate(_timeZone);
        }


        [Test]
        public void VerifyMaxMinLocal()
        {
            var dt1 = new DateTime(2006, 1, 1);
            var dt2 = new DateTime(2006, 3, 1);
            var dt3 = new DateTime(2006, 2, 1);
            var dt4 = new DateTime(2006, 4, 1);
            
            _target.Add(dt1, dt2, JobCategoryType.Forecast);
            _target.Add(dt3, dt4, JobCategoryType.AgentStatistics);

            Assert.AreEqual(dt1, _target.GetJobMultipleDateItem(JobCategoryType.Forecast).StartDateLocal);
            Assert.AreEqual(dt2, _target.GetJobMultipleDateItem(JobCategoryType.Forecast).EndDateLocal);
            Assert.AreEqual(dt3, _target.GetJobMultipleDateItem(JobCategoryType.AgentStatistics).StartDateLocal);
            Assert.AreEqual(dt4, _target.GetJobMultipleDateItem(JobCategoryType.AgentStatistics).EndDateLocal);

            IJobMultipleDateItem jobMultipleDateItem = _target.MinMaxDatesLocal;
            Assert.AreEqual(dt1, jobMultipleDateItem.StartDateLocal);
            Assert.AreEqual(dt4, jobMultipleDateItem.EndDateLocal);

        }

        [Test]
        public void VerifyMaxMinUtc()
        {
            var dt1 = new DateTime(2006, 1, 1);
            var dt2 = new DateTime(2006, 3, 1);
            var dt3 = new DateTime(2006, 2, 1);
            var dt4 = new DateTime(2006, 4, 1);

            _target.Add(dt1, dt2, JobCategoryType.Forecast);
            _target.Add(dt3, dt4, JobCategoryType.AgentStatistics);

            IJobMultipleDateItem jobMultipleDateItem = _target.MinMaxDatesUtc;
            Assert.AreEqual(_timeZone.SafeConvertTimeToUtc(dt1), jobMultipleDateItem.StartDateUtc);
            Assert.AreEqual(_timeZone.SafeConvertTimeToUtc(dt4), jobMultipleDateItem.EndDateUtc);
        }

        [Test]
        public void VerifyClearAndCount()
        {
            var dt1 = new DateTime(2006, 1, 1);
            var dt2 = new DateTime(2006, 3, 1);
            var dt3 = new DateTime(2006, 2, 1);
            var dt4 = new DateTime(2006, 4, 1);

            _target = new JobMultipleDate(_timeZone);
            _target.Add(dt1, dt2, JobCategoryType.Forecast);
            _target.Add(dt3, dt4, JobCategoryType.AgentStatistics);

            Assert.AreEqual(2, _target.Count);
            _target.Clear();
            Assert.AreEqual(0, _target.Count);
        }

        [Test]
        public void VerifyKeysAndValuesCollection()
        {
            var dt1 = new DateTime(2006, 1, 1);
            var dt2 = new DateTime(2006, 3, 1);
            var dt3 = new DateTime(2006, 2, 1);
            var dt4 = new DateTime(2006, 4, 1);
            var dt5 = new DateTime(2006, 1, 1);
            var dt6 = new DateTime(2006, 4, 1);
            IJobMultipleDateItem jobMultipleDateItem = new JobMultipleDateItem(DateTimeKind.Local, dt5, dt6, _timeZone);

            _target = new JobMultipleDate(_timeZone);
            _target.Add(dt1, dt2, JobCategoryType.Forecast);
            _target.Add(dt3, dt4, JobCategoryType.AgentStatistics);
            _target.Add(jobMultipleDateItem, JobCategoryType.QueueStatistics);

            Assert.AreEqual(3, _target.JobCategoryCollection.Count);
            Assert.AreEqual(3, _target.AllDatePeriodCollection.Count);
            Assert.AreEqual(JobCategoryType.Forecast, _target.JobCategoryCollection[0]);
            Assert.AreEqual(JobCategoryType.QueueStatistics, _target.JobCategoryCollection[2]);
            Assert.AreEqual(dt1, _target.AllDatePeriodCollection[0].StartDateLocal);
            Assert.AreEqual(dt6, _target.AllDatePeriodCollection[2].EndDateLocal);
        }
    }
}