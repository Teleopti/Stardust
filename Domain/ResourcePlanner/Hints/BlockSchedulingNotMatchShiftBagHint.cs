using System.Linq;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.UserTexts;

namespace Teleopti.Ccc.Domain.ResourcePlanner.Hints
{
	public class BlockSchedulingNotMatchShiftBagHint : ISchedulePostHint
	{
		private readonly ShiftProjectionCacheManager _shiftProjectionCacheManager;

		public BlockSchedulingNotMatchShiftBagHint(ShiftProjectionCacheManager shiftProjectionCacheManager)
		{
			_shiftProjectionCacheManager = shiftProjectionCacheManager;
		}

		public void FillResult(HintResult hintResult, SchedulePostHintInput input)
		{
			var people = input.People.ToHashSet();
			var period = input.Period;
			var blockPreferenceProvider = input.BlockPreferenceProvider;
			if (blockPreferenceProvider == null)
				return;
			
			foreach (var schedule in input.Schedules)
			{
				var personPeriods = schedule.Key.PersonPeriods(input.Period);
				if (personPeriods.Any(x => x.PersonContract.Contract.EmploymentType == EmploymentType.HourlyStaff)) continue;

				var person = schedule.Key;
				if (!people.Contains(person)) continue;
				var blockOption = blockPreferenceProvider.ForAgent(person, period.StartDate);
				if (blockOption.BlockTypeValue == BlockFinderType.SingleDay) continue;

				var shiftBag = personPeriods.FirstOrDefault()?.RuleSetBag;

				if (shiftBag == null || personPeriods.Any(p => p.RuleSetBag != shiftBag))
					continue;
				var shiftProjectionCaches = _shiftProjectionCacheManager.ShiftProjectionCachesFromRuleSets(new DateOnlyAsDateTimePeriod(period.StartDate, person.PermissionInformation.DefaultTimeZone()), shiftBag.RuleSetCollection, false);
				var allStartTime = shiftProjectionCaches.Select(x => x.WorkShiftStartTime()).Distinct().ToList();

				var scheduleDays = schedule.Value.ScheduledDayCollection(period);
				foreach (var scheduleDay in scheduleDays)
				{
					if (scheduleDay?.PersonAssignment() == null || scheduleDay.HasDayOff() || !scheduleDay.PersonAssignment().ShiftLayers.Any()) continue;
					var timezone = scheduleDay.TimeZone;

					if (blockOption.UseBlockSameShiftCategory && scheduleDay.PersonAssignment().ShiftCategory != null)
					{
						var allShiftCategories = shiftBag.ShiftCategoriesInBag();
						var shiftCategory = scheduleDay.PersonAssignment().ShiftCategory;
						if (allShiftCategories.All(x => x.Id.GetValueOrDefault() != shiftCategory.Id.GetValueOrDefault()))
						{
							hintResult.Add(new PersonHintError
							{
								PersonName = person.Name.ToString(),
								PersonId = person.Id.Value,
								ErrorResource = nameof(Resources.ShiftCategoryNotMatchingShiftBag),
								ErrorResourceData = new object[] { shiftCategory.Description.ShortName, scheduleDay.DateOnlyAsPeriod.DateOnly.Date, shiftBag.Description.Name }.ToList()
							}, GetType(), ValidationResourceType.BlockScheduling);
							break;
						}
					}

					if (blockOption.UseBlockSameStartTime)
					{
						var timeOfDay = scheduleDay.PersonAssignment().Period.StartDateTimeLocal(timezone);
						if (allStartTime.All(x=> x != timeOfDay.TimeOfDay))
						{
							hintResult.Add(new PersonHintError
							{
								PersonName = person.Name.ToString(),
								PersonId = person.Id.Value,
								ErrorResource = nameof(Resources.StartTimeNotMatchingShiftBag),
								ErrorResourceData = new object[] { timeOfDay, scheduleDay.DateOnlyAsPeriod.DateOnly.Date, shiftBag.Description.Name }.ToList()
							}, GetType(), ValidationResourceType.BlockScheduling);
							break;
						}
					}
				}
			}
		}
	}
}