using System;
using System.Collections.Generic;
using NUnit.Framework;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Time;
using Teleopti.Ccc.TestCommon.FakeData;


namespace Teleopti.Ccc.DomainTest.Time
{
    [TestFixture]
	[TestWithStaticDependenciesDONOTUSE]
	public class SetupDateTimePeriodToSelectedSchedulesTest
    {
        private SchedulePartFactoryForDomain _partFactoryForDomain;

        [SetUp]
        public void Setup()
        {
            _partFactoryForDomain = new SchedulePartFactoryForDomain();
        }

        [Test]
        public void VerifyTakesTotalPeriodOfSchedules()
        {
            IScheduleDay s1 = _partFactoryForDomain.CreatePart();
            IScheduleDay s2 = _partFactoryForDomain.CreatePartWithMainShift(); 
            IScheduleDay s3 = _partFactoryForDomain.CreatePartWithDifferentPeriod(5);

            DateTimePeriod expectedtResult = new DateTimePeriod(s1.Period.StartDateTime, s3.Period.EndDateTime.Subtract(TimeSpan.FromMinutes(1)));

            var target = new SetupDateTimePeriodToSelectedSchedules(new List<IScheduleDay> {s1, s2, s3});
            Assert.AreEqual(expectedtResult, target.Period);

           
        }

        [Test]
        public void VerifyUsesTheMainShiftIfThereIsOnlyOneSchedulePart()
        {
            IScheduleDay schedule = _partFactoryForDomain.CreatePartWithMainShift();

            DateTimePeriod expectedtResult = schedule.GetEditorShift().ProjectionService().CreateProjection().Period().Value;

            var target = new SetupDateTimePeriodToSelectedSchedules(new List<IScheduleDay> { schedule });
            Assert.AreEqual(expectedtResult, target.Period);
        }

        [Test]
        public void VerifyUsesSchedulePartIfNoMainShift()
        {
            IScheduleDay schedule = _partFactoryForDomain.CreatePartWithoutMainShift();

            DateTimePeriod expectedtResult = schedule.Period.ChangeEndTime(TimeSpan.FromMinutes(-1));

            var target = new SetupDateTimePeriodToSelectedSchedules(new List<IScheduleDay> { schedule });
            Assert.AreEqual(expectedtResult, target.Period);
        }
    }
}