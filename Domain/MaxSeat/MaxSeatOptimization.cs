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
									var bestShift = _shiftProjectionCacheManager.ShiftProjectionCachesFromRuleSets(date, timeZone, bag, false, true).Last().TheMainShift;
									//


									//REPLACE SHIFT
									var schedule = schedules[person].ScheduledDay(date);
									var tempAssLengthPeriodBeforeREMOVEME = schedule.PersonAssignment(true).Period.ElapsedTime();
									schedule.AddMainShift(bestShift);
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