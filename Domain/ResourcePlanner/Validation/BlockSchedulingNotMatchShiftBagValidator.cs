using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.UserTexts;

namespace Teleopti.Ccc.Domain.ResourcePlanner.Validation
{
	public class BlockSchedulingNotMatchShiftBagValidator : IScheduleValidator
	{
		private readonly IScheduleDayEquator _scheduleDayEquator;
		private readonly IShiftCreatorService _shiftCreatorService;

		public BlockSchedulingNotMatchShiftBagValidator(IScheduleDayEquator scheduleDayEquator, IShiftCreatorService shiftCreatorService)
		{
			_scheduleDayEquator = scheduleDayEquator;
			_shiftCreatorService = shiftCreatorService;
		}
		public void FillResult(ValidationResult validationResult, ValidationInput input)
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
				var allStartTime =new List<DateTime>();
				var allShifts = new List<IWorkShift>();
				foreach (var workShiftRuleSet in shiftBag.RuleSetCollection)
				{
					var workShiftCollections = _shiftCreatorService.Generate(workShiftRuleSet, null);
					foreach (var workShiftCollection in workShiftCollections)
					{
						allStartTime.AddRange(workShiftCollection.Select(x=>x.Projection.Period().Value.StartDateTime));
						allShifts.AddRange(workShiftCollection);
					}
				}
				allStartTime = allStartTime.Distinct().ToList();
				allShifts = allShifts.Distinct().ToList();
				
				var scheduleDays = schedule.Value.ScheduledDayCollection(period);
				foreach (var scheduleDay in scheduleDays)
				{
					if (scheduleDay?.PersonAssignment() == null || scheduleDay.HasDayOff()) continue;
					var timezone = scheduleDay.TimeZone;
					if (blockOption.UseBlockSameStartTime)
					{
						var timeOfDay = scheduleDay.PersonAssignment().Period.StartDateTimeLocal(timezone).TimeOfDay;
						if (allStartTime.All(x=> x.TimeOfDay != timeOfDay))
						{
							validationResult.Add(new PersonValidationError
							{
								PersonName = person.Name.ToString(),
								PersonId = person.Id.Value,
								ValidationError =
									string.Format(Resources.StartTimeNotMatchingShiftBag, scheduleDay.PersonAssignment().Date,
										shiftBag.Description.Name)
							}, GetType());
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
								validationResult.Add(new PersonValidationError
								{
									PersonName = person.Name.ToString(),
									PersonId = person.Id.Value,
									ValidationError =
										string.Format(Resources.ShiftNotMatchingShiftBag, scheduleDay.PersonAssignment().Date,
											shiftBag.Description.Name)
								}, GetType());
							}
						}
					}
				}
			}
		}
	}
}