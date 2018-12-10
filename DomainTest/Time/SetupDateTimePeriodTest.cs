using System;
using System.Collections.Generic;
using NUnit.Framework;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Time;
using Teleopti.Ccc.TestCommon.FakeData;


namespace Teleopti.Ccc.DomainTest.Time
{
    [TestFixture]
    public class SetupDateTimePeriodTest
    {
        private SchedulePartFactoryForDomain _partFactory;
        private SchedulePartFactoryForDomain _factory;
        private SetupDateTimePeriodToSelectedSchedules target;
        
        [SetUp]
        public void Setup()
        {
            _partFactory = new SchedulePartFactoryForDomain();
        }

        [Test]
        public void VerifyGetsValueFromSchedulePartIfDefaultPeriodIsNull()
        {
            IScheduleDay scheduleDay = _partFactory.CreatePart();
            IList<IScheduleDay> schedules = new List<IScheduleDay>{scheduleDay};

            target = new SetupDateTimePeriodToSelectedSchedules(schedules);

            Assert.IsTrue(scheduleDay.Period.Contains(target.Period),"I just want to know that it gets the period from the schedule");
        }

        [Test]
        public void VerifyGetsTotalPeriodFromScheduleParts()
        {
            _factory = new SchedulePartFactoryForDomain();
            IScheduleDay scheduleDay1 = _factory.CreatePart();
            IScheduleDay scheduleDay2 = _factory.CreatePartWithDifferentPeriod(3);
            IList<IScheduleDay> schedules = new List<IScheduleDay> { scheduleDay1, scheduleDay2 };
            target = new SetupDateTimePeriodToSelectedSchedules(schedules);

            Assert.AreEqual(scheduleDay1.Period.StartDateTime, target.Period.StartDateTime);
            Assert.AreEqual(scheduleDay2.Period.EndDateTime.Subtract(TimeSpan.FromMinutes(1)), target.Period.EndDateTime, "Make sure its  23:59 on the last schedule day.");
        }

        [Test]
        public void ShouldThrowExceptionWhenNoPeriodAvailable()
        {
            Assert.Throws<InvalidOperationException>(() => target = new SetupDateTimePeriodToSelectedSchedules(new List<IScheduleDay>()));
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic"), Test]
        public void VerifyDefaultPeriod()
        {
            var otherTarget = new SetupDateTimePeriodToDefaultPeriod(null, null);
            
            DateTimePeriod comparePeriod = new DateTimePeriod(DateTime.UtcNow.Subtract(TimeSpan.FromDays(1)), DateTime.UtcNow.Add(TimeSpan.FromDays(1)));

            Assert.IsTrue(comparePeriod.Contains(otherTarget.Period), " Just make sure its about now and not to long by default");
            Assert.IsTrue(otherTarget.Period.ElapsedTime() > TimeSpan.FromMinutes(5), "Should not be to short by default");
        }
    }
}
