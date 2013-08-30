using System;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using NUnit.Framework;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Scheduling.Assignment
{
    [TestFixture]
    public class RestoreSchedulePartServiceTest
    {
        [Test]
        public void ShouldRestoreAssignmentAndKeepIdAndVersionFromCurrent()
        {
					var current = ScheduleDayFactory.Create(new DateOnly(2000, 1, 1));
					var historical = (IScheduleDay)current.Clone();
					var target = new RestoreSchedulePartService();
					var currentAss = PersonAssignmentFactory.CreateAssignmentWithDayOff(current.Scenario, current.Person, current.DateOnlyAsPeriod.DateOnly, new DayOffTemplate());
					currentAss.SetId(Guid.NewGuid());
					currentAss.SetVersion(12);
					var historicalAss = PersonAssignmentFactory.CreateAssignmentWithMainShift(historical.Scenario, historical.Person, new DateTimePeriod(2000,1,1,2000,1,2));
					historicalAss.SetId(Guid.NewGuid());
					historicalAss.SetVersion(123);
					current.Add(currentAss);
					historical.Add(historicalAss);

					target.Restore(current, historical);

	        current.PersonAssignment().Id.Should().Be.EqualTo(currentAss.Id);
	        current.PersonAssignment().Version.Should().Be.EqualTo(currentAss.Version);
	        current.PersonAssignment().DayOff().Should().Be.Null();
        }
    }
}
