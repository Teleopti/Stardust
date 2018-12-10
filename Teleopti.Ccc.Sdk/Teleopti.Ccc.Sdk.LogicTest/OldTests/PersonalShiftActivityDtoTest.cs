using System;
using NUnit.Framework;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;


namespace Teleopti.Ccc.Sdk.LogicTest.OldTests
{
    [TestFixture]
    public class PersonalShiftActivityDtoTest
    {
        private PersonalShiftActivityDto _target;
        private IActivity _activity;
        private DateTimePeriod _period;

        [SetUp]
        public void Setup()
        {
            DateTime startDateTime = new DateTime(2008, 10, 1, 5, 44, 0, DateTimeKind.Utc);
            DateTime endDateTime = startDateTime.AddHours(2);
            _period = new DateTimePeriod(startDateTime, endDateTime);
            _activity = new Activity("PersonalShift");
            
            _target = new PersonalShiftActivityDto
                          {
                              Activity = new ActivityDto {Description = _activity.Description.Name},
                              Period = new DateTimePeriodDto {UtcStartTime = startDateTime, UtcEndTime = endDateTime}
                          };
        }

        [Test]
        public void VerifyConstructor()
        {
            Assert.AreEqual("PersonalShift", _target.Activity.Description);
            Assert.AreEqual(_period.StartDateTime, _target.Period.UtcStartTime);
            Assert.AreEqual(_period.EndDateTime, _target.Period.UtcEndTime);
        }
    }
}