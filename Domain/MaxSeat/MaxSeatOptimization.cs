using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Cascading;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.SeatLimitation;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.MaxSeat
{
	//bara testeri, testara just nu...
	public class MaxSeatOptimization
	{
		private readonly MaxSeatSkillCreator _maxSeatSkillCreator;
		private readonly ResourceCalculationContextFactory _resourceCalculationContextFactory;
		private readonly CascadingResourceCalculation _resourceCalculation;
		private readonly IShiftProjectionCacheManager _shiftProjectionCacheManager;

		public MaxSeatOptimization(MaxSeatSkillCreator maxSeatSkillCreator,
														CascadingResourceCalculationContextFactory resourceCalculationContextFactory,
														CascadingResourceCalculation resourceCalculation,
														IShiftProjectionCacheManager shiftProjectionCacheManager)
		{
			_maxSeatSkillCreator = maxSeatSkillCreator;
			_resourceCalculationContextFactory = resourceCalculationContextFactory;
			_resourceCalculation = resourceCalculation;
			_shiftProjectionCacheManager = shiftProjectionCacheManager;
		}

		public void Optimize(DateOnlyPeriod period, IEnumerable<IPerson> persons, IScheduleDictionary schedules, IScenario scenario)
		{
			var generatedMaxSeatSkills = _maxSeatSkillCreator.CreateMaxSeatSkills(period, scenario, persons.ToArray(), Enumerable.Empty<ISkill>());
			var skillDays = generatedMaxSeatSkills.SkillDaysToAddToStateholder;

			using (_resourceCalculationContextFactory.Create(schedules, generatedMaxSeatSkills.SkillsToAddToStateholder))
			{
				_resourceCalculation.ResourceCalculate(period, new ResourceCalculationData(schedules, generatedMaxSeatSkills.SkillsToAddToStateholder, skillDays, false, false));

				foreach (var date in period.DayCollection())
				{
					foreach (var skillPair in skillDays)
					{
						foreach (var skillDay in skillPair.Value)
						{
							if (skillDay.CurrentDate != date)
								continue; //FIX LATER

							//hitta gubbe random?
							foreach (var person in persons)
							{
								//BEST SHIFT STUFF
								var timeZone = person.PermissionInformation.DefaultTimeZone();
								var personPeriod = person.Period(skillDay.CurrentDate);
								var bag = personPeriod.RuleSetBag;
								var bestShift = _shiftProjectionCacheManager.ShiftProjectionCachesFromRuleSets(skillDay.CurrentDate, timeZone, bag, false, true).Last().TheMainShift;
								//


								//REPLACE SHIFT
								var schedule = schedules[person].ScheduledDay(skillDay.CurrentDate);
								schedule.AddMainShift(bestShift);
								schedules.Modify(schedule, new DoNothingScheduleDayChangeCallBack()); //ska vara den andra
																																											//
							}
						}
					}
				}
			}
		}
	}
}