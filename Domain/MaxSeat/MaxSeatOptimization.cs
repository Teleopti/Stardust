using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling.SeatLimitation;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.MaxSeat
{
	public class MaxSeatOptimization
	{
		private readonly MaxSeatSkillCreator _maxSeatSkillCreator;
		private readonly ResourceCalculationContextFactory _resourceCalculationContextFactory;
		private readonly IShiftProjectionCacheManager _shiftProjectionCacheManager;
		private readonly IScheduleDayChangeCallback _scheduleDayChangeCallback;

		public MaxSeatOptimization(MaxSeatSkillCreator maxSeatSkillCreator,
														CascadingResourceCalculationContextFactory resourceCalculationContextFactory,
														IShiftProjectionCacheManager shiftProjectionCacheManager,
														IScheduleDayChangeCallback scheduleDayChangeCallback)
		{
			_maxSeatSkillCreator = maxSeatSkillCreator;
			_resourceCalculationContextFactory = resourceCalculationContextFactory;
			_shiftProjectionCacheManager = shiftProjectionCacheManager;
			_scheduleDayChangeCallback = scheduleDayChangeCallback;
		}

		public void Optimize(DateOnlyPeriod period, IEnumerable<IPerson> persons, IScheduleDictionary schedules, IScenario scenario)
		{
			var generatedMaxSeatSkills = _maxSeatSkillCreator.CreateMaxSeatSkills(period, scenario, persons.ToArray(), Enumerable.Empty<ISkill>());
			var allSkillDays = generatedMaxSeatSkills.SkillDaysToAddToStateholder.SelectMany(x => x.Value);
			using (_resourceCalculationContextFactory.Create(schedules, generatedMaxSeatSkills.SkillsToAddToStateholder))
			{
				foreach (var date in period.DayCollection())
				{
					foreach (var skillDay in allSkillDays.Where(x => x.CurrentDate == date))
					{
						foreach (var skillStaffPeriod in skillDay.SkillStaffPeriodCollection)
						{
							//hitta gubbe random?
							foreach (var person in persons)
							{
								if (ResourceCalculationContext.Fetch().ActivityResourcesWhereSeatRequired(skillDay.Skill, skillStaffPeriod.Period) <= skillStaffPeriod.Payload.MaxSeats)
									continue;

								//BEST SHIFT STUFF
								IEditableShift shiftToUse = null;
								var shiftLengthBeforeTEMPremoveMe = schedules[person].ScheduledDay(date).PersonAssignment(true).Period.ElapsedTime();
								foreach (var shift in _shiftProjectionCacheManager.ShiftProjectionCachesFromRuleSets(date, person.PermissionInformation.DefaultTimeZone(), person.Period(date).RuleSetBag, false, true))
								{
									var layerThisPeriod = shift.MainShiftProjection.SingleOrDefault(x => x.Period.Contains(skillStaffPeriod.Period)); //should handle "containspart" and multiple layers on period cases
									if ((layerThisPeriod == null || !((IActivity)layerThisPeriod.Payload).RequiresSeat)
										&& shift.MainShiftProjection.Period().Value.ElapsedTime() == shiftLengthBeforeTEMPremoveMe) //remove this line later
									{
										shiftToUse = shift.TheMainShift;
										break;
									}
								}

								//REPLACE SHIFT
								if (shiftToUse == null)
									continue;
								var schedule = schedules[person].ScheduledDay(date);
								schedule.AddMainShift(shiftToUse);
								schedules.Modify(schedule, _scheduleDayChangeCallback);
							}
						}
					}
				}
			}
		}
	}
}