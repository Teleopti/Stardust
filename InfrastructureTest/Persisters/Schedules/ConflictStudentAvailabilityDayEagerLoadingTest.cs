using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Restriction;
using Teleopti.Ccc.Infrastructure.Foundation;


namespace Teleopti.Ccc.InfrastructureTest.Persisters.Schedules
{
	public class ConflictStudentAvailabilityDayEagerLoadingTest : ScheduleRangeConflictTest
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

		protected override void WhenOtherHasChanged(IScheduleRange othersScheduleRange)
		{
			var day = othersScheduleRange.ScheduledDay(date);
			var restriction = (IStudentAvailabilityDay)day.PersonRestrictionCollection().Single();
			restriction.RestrictionCollection.Single().StartTimeLimitation = new StartTimeLimitation(TimeSpan.FromHours(6),TimeSpan.FromHours(11));
			DoModify(day);
		}

		protected override void WhenImChanging(IScheduleRange myScheduleRange)
		{
			var day = myScheduleRange.ScheduledDay(date);
			var restriction = (IStudentAvailabilityDay)day.PersonRestrictionCollection().Single();
			restriction.RestrictionCollection.Single().EndTimeLimitation = new EndTimeLimitation(TimeSpan.FromHours(11), TimeSpan.FromHours(12));
			DoModify(day);
		}

		protected override void Then(IEnumerable<PersistConflict> conflicts)
		{
			var conflict = (IStudentAvailabilityDay)conflicts.Single().DatabaseVersion;
			LazyLoadingManager.IsInitialized(conflict.RestrictionCollection);
			LazyLoadingManager.IsInitialized(conflict.RestrictionCollection.Single().StartTimeLimitation);
			LazyLoadingManager.IsInitialized(conflict.RestrictionCollection.Single().EndTimeLimitation);
			LazyLoadingManager.IsInitialized(conflict.RestrictionCollection.Single().WorkTimeLimitation);
		}
	}
}