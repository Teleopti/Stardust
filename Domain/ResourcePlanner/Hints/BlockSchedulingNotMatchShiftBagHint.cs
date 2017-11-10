using System;
using System.Linq;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.UserTexts;

namespace Teleopti.Ccc.Domain.ResourcePlanner.Hints
{
	public class BlockSchedulingNotMatchShiftBagHint : IScheduleHint
	{
		private readonly IScheduleDayEquator _scheduleDayEquator;
		private readonly IShiftProjectionCacheManager _shiftProjectionCacheManager;

		public BlockSchedulingNotMatchShiftBagHint(IScheduleDayEquator scheduleDayEquator, IShiftProjectionCacheManager shiftProjectionCacheManager)
		{
			_scheduleDayEquator = scheduleDayEquator;
			_shiftProjectionCacheManager = shiftProjectionCacheManager;
		}

		public void FillResult(HintResult hintResult, HintInput input)
		{
			var people = input.People;
			var period = input.Period;
			var blockPreferenceProvider = input.BlockPreferenceProvider;
			var schedules = input.Schedules ?? input.CurrentSchedule;
			foreach (var schedule in schedules)
			{
				var person = schedule.Key;
				if (!people.Contains(person)) continue;
				var blockOption = blockPreferenceProvider.ForAgent(person, period.StartDate);
				if (!blockOption.UseTeamBlockOption) continue;
				var shiftBag = person.Period(period.StartDate).RuleSetBag;
				var shiftProjectionCaches = _shiftProjectionCacheManager.ShiftProjectionCachesFromRuleSets(new DateOnlyAsDateTimePeriod(period.StartDate, person.PermissionInformation.DefaultTimeZone()), shiftBag.RuleSetCollection, false);
				var allStartTime = shiftProjectionCaches.Select(x => x.WorkShiftStartTime).Distinct().ToList();
				var allShifts = shiftProjectionCaches.Select(x=>x.TheWorkShift).Distinct().ToList();
				
				var scheduleDays = schedule.Value.ScheduledDayCollection(period);
				foreach (var scheduleDay in scheduleDays)
				{
					if (scheduleDay?.PersonAssignment() == null || scheduleDay.HasDayOff() || !scheduleDay.PersonAssignment().ShiftLayers.Any()) continue;
					var timezone = scheduleDay.TimeZone;

					if (blockOption.UseBlockSameShiftCategory)
					{
						var allShiftCategories = shiftBag.ShiftCategoriesInBag();
						var shiftCategory = scheduleDay.PersonAssignment().ShiftCategory;
						if (allShiftCategories.All(x => x.Id.GetValueOrDefault() != shiftCategory.Id.GetValueOrDefault()))
						{
							hintResult.Add(new PersonHintError
							{
								PersonName = person.Name.ToString(),
								PersonId = person.Id.Value,
								ValidationError = string.Format(Resources.ShiftCategoryNotMatchingShiftBag, shiftCategory.Description.ShortName, scheduleDay.DateOnlyAsPeriod.DateOnly.ToShortDateString(), shiftBag.Description.Name)
							}, GetType());
							break;
						}
					}

					if (blockOption.UseBlockSameStartTime)
					{
						var timeOfDay = scheduleDay.PersonAssignment().Period.StartDateTimeLocal(timezone).TimeOfDay;
						if (allStartTime.All(x=> x != timeOfDay))
						{
							hintResult.Add(new PersonHintError
							{
								PersonName = person.Name.ToString(),
								PersonId = person.Id.Value,
								ValidationError = string.Format(Resources.StartTimeNotMatchingShiftBag, timeOfDay, scheduleDay.DateOnlyAsPeriod.DateOnly.ToShortDateString(), shiftBag.Description.Name)
							}, GetType());
							break;
						}
					}
					
					if (blockOption.UseBlockSameShift)
					{
						var dateOnlyAsDateTimePeriod = scheduleDay.DateOnlyAsPeriod;
						var editorShiftFromScheduleDay = scheduleDay.GetEditorShift();

						if (editorShiftFromScheduleDay?.ShiftCategory != null)
						{
							var found = false;
							foreach (var workShift in allShifts)
							{
								var editorShiftFromShiftBag = workShift.ToEditorShift(dateOnlyAsDateTimePeriod, TimeZoneInfo.Utc);
								if (_scheduleDayEquator.MainShiftEquals(editorShiftFromScheduleDay, editorShiftFromShiftBag))
								{
									found = true;
									break;
								}
							}
							if (!found)
							{
								hintResult.Add(new PersonHintError
								{
									PersonName = person.Name.ToString(),
									PersonId = person.Id.Value,
									ValidationError = string.Format(Resources.ShiftNotMatchingShiftBag, scheduleDay.DateOnlyAsPeriod.DateOnly.ToShortDateString(), shiftBag.Description.Name)
								}, GetType());
								break;
							}
						}
					}
				}
			}
		}
	}
}