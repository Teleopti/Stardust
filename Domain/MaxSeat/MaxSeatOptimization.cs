using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling.SeatLimitation;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.MaxSeat
{
	public class MaxSeatOptimization
	{
		private readonly MaxSeatSkillDataFactory _maxSeatSkillDataFactory;
		private readonly ResourceCalculationContextFactory _resourceCalculationContextFactory;
		private readonly IShiftProjectionCacheManager _shiftProjectionCacheManager;
		private readonly IScheduleDayChangeCallback _scheduleDayChangeCallback;

		public MaxSeatOptimization(MaxSeatSkillDataFactory maxSeatSkillDataFactory, 
														CascadingResourceCalculationContextFactory resourceCalculationContextFactory,
														IShiftProjectionCacheManager shiftProjectionCacheManager,
														IScheduleDayChangeCallback scheduleDayChangeCallback)
		{
			_maxSeatSkillDataFactory = maxSeatSkillDataFactory;
			_resourceCalculationContextFactory = resourceCalculationContextFactory;
			_shiftProjectionCacheManager = shiftProjectionCacheManager;
			_scheduleDayChangeCallback = scheduleDayChangeCallback;
		}

		public void Optimize(DateOnlyPeriod period, IEnumerable<IPerson> persons, IScheduleDictionary schedules, IScenario scenario, IOptimizationPreferences optimizationPreferences)
		{
			var maxSeatData = _maxSeatSkillDataFactory.Create(period, persons, scenario);

			using (_resourceCalculationContextFactory.Create(schedules, maxSeatData.AllMaxSeatSkills()))
			{
				foreach (var date in period.DayCollection())
				{
					foreach (var skillDay in maxSeatData.SkillDaysForDate(date))
					{
						var siteForSkill = maxSeatData.SiteForSkill(skillDay.Skill);

						foreach (var skillStaffPeriod in skillDay.SkillStaffPeriodCollection)
						{
							//hitta gubbe random?
							foreach (var person in persons)
							{
								if (!person.Period(date).Team.Site.Equals(siteForSkill))
									continue;

								if (ResourceCalculationContext.Fetch().ActivityResourcesWhereSeatRequired(skillDay.Skill, skillStaffPeriod.Period) <= siteForSkill.MaxSeats.Value)
									continue;

								//BEST SHIFT STUFF
								IEditableShift shiftToUse = null;
								var contractTimeBefore =
									schedules[person].ScheduledDay(date) // don't do this every interval/skillstaffperiod
										.PersonAssignment(true)
										.ProjectionService()
										.CreateProjection()
										.ContractTime();

								foreach (var shift in _shiftProjectionCacheManager.ShiftProjectionCachesFromRuleSets(date, person.PermissionInformation.DefaultTimeZone(), person.Period(date).RuleSetBag, false, true))
								{
									var layerThisPeriod = shift.MainShiftProjection.SingleOrDefault(x => x.Period.Contains(skillStaffPeriod.Period)); 
									if ((layerThisPeriod == null || !((IActivity)layerThisPeriod.Payload).RequiresSeat) &&
										shift.MainShiftProjection.ContractTime() == contractTimeBefore)
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