using System;
using System.Collections.Generic;
using System.Linq;
using SharpTestsEx;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Restriction;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.InfrastructureTest.Persisters.Schedules
{
	public class Bug39612 : ScheduleRangeConflictTest
	{
		private readonly DateOnly date = new DateOnly(2001, 1, 2);

		protected override IEnumerable<IPersistableScheduleData> Given()
		{
			var restriction = new StudentAvailabilityRestriction
			{
				StartTimeLimitation = new StartTimeLimitation(TimeSpan.FromHours(10), TimeSpan.FromHours(14)),
				EndTimeLimitation = new EndTimeLimitation(TimeSpan.FromHours(10), TimeSpan.FromHours(14)),
				WorkTimeLimitation = new WorkTimeLimitation(TimeSpan.FromHours(10), TimeSpan.FromHours(14))
			};
			var studAvail = new StudentAvailabilityDay(Person, date, new[] { restriction });
			return new[] { studAvail };
		}

		protected override void WhenImChanging(IScheduleRange myScheduleRange)
		{
			var day = myScheduleRange.ScheduledDay(date);
			day.Remove((IStudentAvailabilityDay)day.PersonRestrictionCollection().Single());

			var restriction = new StudentAvailabilityRestriction
			{
				StartTimeLimitation = new StartTimeLimitation(TimeSpan.FromHours(1), TimeSpan.FromHours(2)),
				EndTimeLimitation = new EndTimeLimitation(TimeSpan.FromHours(1), TimeSpan.FromHours(2)),
				WorkTimeLimitation = new WorkTimeLimitation(TimeSpan.FromHours(1), TimeSpan.FromHours(2))
			};
			var studAvail = new StudentAvailabilityDay(Person, date, new[] { restriction });
			day.Add(studAvail);
			DoModify(day);
		}

		protected override void Then(IEnumerable<PersistConflict> conflicts)
		{
			conflicts.Should().Be.Empty();
		}

		protected override void Then(IScheduleRange myScheduleRange)
		{
			myScheduleRange.ScheduledDay(date).PersonRestrictionCollection().Count
				.Should().Be.EqualTo(1);
		}
	}
}