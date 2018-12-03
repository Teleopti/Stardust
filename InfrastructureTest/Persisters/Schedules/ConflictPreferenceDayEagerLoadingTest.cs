using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Restriction;
using Teleopti.Ccc.Infrastructure.Foundation;


namespace Teleopti.Ccc.InfrastructureTest.Persisters.Schedules
{
	public class ConflictPreferenceDayEagerLoadingTest : ScheduleRangeConflictTest
	{
		private readonly DateOnly date = new DateOnly(2000, 1, 1);

		protected override IEnumerable<IPersistableScheduleData> Given()
		{
			var restriction = new PreferenceRestriction { Absence = Absence };
			restriction.AddActivityRestriction(new ActivityRestriction(Activity));
			restriction.DayOffTemplate = DayOffTemplate;
			restriction.ShiftCategory = ShiftCategory;
			restriction.StartTimeLimitation = new StartTimeLimitation(TimeSpan.FromHours(10), TimeSpan.FromHours(11));
			restriction.EndTimeLimitation = new EndTimeLimitation(TimeSpan.FromHours(10), TimeSpan.FromHours(11));
			var prefDay = new PreferenceDay(Person, date, restriction);
			return new[] { prefDay };
		}

		protected override void WhenOtherHasChanged(IScheduleRange othersScheduleRange)
		{
			var day = othersScheduleRange.ScheduledDay(date);
			var prefDay = (IPreferenceDay)day.PersonRestrictionCollection().Single();
			prefDay.Restriction.StartTimeLimitation = new StartTimeLimitation(TimeSpan.FromHours(8), TimeSpan.FromHours(9));
			DoModify(day);
		}

		protected override void WhenImChanging(IScheduleRange myScheduleRange)
		{
			var day = myScheduleRange.ScheduledDay(date);
			var prefDay = (IPreferenceDay)day.PersonRestrictionCollection().Single();
			prefDay.Restriction.StartTimeLimitation = new StartTimeLimitation(TimeSpan.FromHours(8), TimeSpan.FromHours(9));
			DoModify(day);
		}

		protected override void Then(IEnumerable<PersistConflict> conflicts)
		{
			var prefDay = (IPreferenceDay)conflicts.Single().DatabaseVersion;
			LazyLoadingManager.IsInitialized(prefDay.Restriction.ActivityRestrictionCollection);
			LazyLoadingManager.IsInitialized(prefDay.Restriction.Absence);
			LazyLoadingManager.IsInitialized(prefDay.Restriction.DayOffTemplate);
			LazyLoadingManager.IsInitialized(prefDay.Restriction.StartTimeLimitation);
			LazyLoadingManager.IsInitialized(prefDay.Restriction.EndTimeLimitation);
			LazyLoadingManager.IsInitialized(prefDay.Restriction);
			LazyLoadingManager.IsInitialized(prefDay.Restriction);
		}
	}
}