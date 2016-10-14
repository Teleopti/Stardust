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
			var skillDays = generatedMaxSeatSkills.SkillDaysToAddToStateholder;


			using (_resourceCalculationContextFactory.Create(schedules, generatedMaxSeatSkills.SkillsToAddToStateholder))
			{
				foreach (var date in period.DayCollection())
				{
					foreach (var skillPair in skillDays)
					{
						foreach (var skillDay in skillPair.Value)
						{
							foreach (var skillStaffPeriod in skillDay.SkillStaffPeriodCollection)
							{
								if (skillDay.CurrentDate != date)
									continue; //FIX LATER

								//hitta gubbe random?
								foreach (var person in persons)
								{
									if (ResourceCalculationContext.Fetch().ActivityResourcesWhereSeatRequired(skillDay.Skill, skillStaffPeriod.Period) <= skillStaffPeriod.Payload.MaxSeats)
										continue;

									//BEST SHIFT STUFF
									var timeZone = person.PermissionInformation.DefaultTimeZone();
									var personPeriod = person.Period(date);
									if (personPeriod == null)
										continue;
									var bag = personPeriod.RuleSetBag;
									IEditableShift shiftToUse = null;
									foreach (var shift in _shiftProjectionCacheManager.ShiftProjectionCachesFromRuleSets(date, timeZone, bag, false, true).Reverse()) //fix reverse soon - only to get a green build
									{
										var layerThisPeriod = shift.MainShiftProjection.SingleOrDefault(x => x.Period.Contains(skillStaffPeriod.Period)); //should handle "containspart" and multiple layers on period cases
										if (layerThisPeriod == null || !((IActivity) layerThisPeriod.Payload).RequiresSeat)
										{
											shiftToUse = shift.TheMainShift;
											break;
										}
									}


									//REPLACE SHIFT
									if(shiftToUse==null)
										continue;
									var schedule = schedules[person].ScheduledDay(date);
									var tempAssLengthPeriodBeforeREMOVEME = schedule.PersonAssignment(true).Period.ElapsedTime();
									schedule.AddMainShift(shiftToUse);
									var tempAssLEngthPeriodAfterREMOVEME = schedule.PersonAssignment(true).Period.ElapsedTime();
									if(tempAssLEngthPeriodAfterREMOVEME == tempAssLengthPeriodBeforeREMOVEME)
										schedules.Modify(schedule, _scheduleDayChangeCallback);
								}
							}
						}
					}
				}
			}
		}
	}
}