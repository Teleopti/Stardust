using System;
using NUnit.Framework;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;

namespace Teleopti.Ccc.Sdk.LogicTest.OldTests
{
    [TestFixture]
    public class AdherenceInfoDtoTest
    {
        [Test]
        public void VerifyCanCreate()
        {
			var target = new AdherenceInfoDto();
			Assert.IsNotNull(target);
        }

        [Test]
        public void VerifySetAndGetProperties()
        {
			var availableTime = TimeSpan.FromMinutes(12).Ticks;
	        var dateOnlyDto = new DateOnlyDto {DateTime = DateTime.Today};
	        var idleTime = TimeSpan.FromMinutes(5).Ticks;
			var loggedInTime = TimeSpan.FromMinutes(22).Ticks;
			var scheduleWorkCtiTime = TimeSpan.FromMinutes(17).Ticks;

			var target = new AdherenceInfoDto();
			target.AvailableTime = availableTime;
            target.DateOnlyDto = dateOnlyDto;
            target.IdleTime = idleTime;
            target.LoggedInTime = loggedInTime;
            target.ScheduleWorkCtiTime = scheduleWorkCtiTime;

            Assert.AreEqual(availableTime, target.AvailableTime);
            Assert.AreEqual(dateOnlyDto, target.DateOnlyDto);
            Assert.AreEqual(idleTime, target.IdleTime);
            Assert.AreEqual(loggedInTime, target.LoggedInTime);
            Assert.AreEqual(scheduleWorkCtiTime, target.ScheduleWorkCtiTime);
        }
    }
}
