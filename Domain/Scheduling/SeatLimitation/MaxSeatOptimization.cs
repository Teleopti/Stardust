using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.SeatLimitation
{
	public class MaxSeatOptimization
	{
		private readonly MaxSeatSkillDataFactory _maxSeatSkillDataFactory;
		private readonly ResourceCalculationContextFactory _resourceCalculationContextFactory;
		private readonly IShiftProjectionCacheManager _shiftProjectionCacheManager;
		private readonly IScheduleDayChangeCallback _scheduleDayChangeCallback;
		private readonly IMainShiftOptimizeActivitySpecificationSetter _mainShiftOptimizeActivitySpecificationSetter;
		private readonly ISchedulingOptionsCreator _schedulingOptionsCreator;

		public MaxSeatOptimization(MaxSeatSkillDataFactory maxSeatSkillDataFactory, 
														CascadingResourceCalculationContextFactory resourceCalculationContextFactory,
														IShiftProjectionCacheManager shiftProjectionCacheManager,
														IScheduleDayChangeCallback scheduleDayChangeCallback,
														IMainShiftOptimizeActivitySpecificationSetter mainShiftOptimizeActivitySpecificationSetter,
														ISchedulingOptionsCreator schedulingOptionsCreator)
		{
			_maxSeatSkillDataFactory = maxSeatSkillDataFactory;
			_resourceCalculationContextFactory = resourceCalculationContextFactory;
			_shiftProjectionCacheManager = shiftProjectionCacheManager;
			_scheduleDayChangeCallback = scheduleDayChangeCallback;
			_mainShiftOptimizeActivitySpecificationSetter = mainShiftOptimizeActivitySpecificationSetter;
			_schedulingOptionsCreator = schedulingOptionsCreator;
		}

		public void Optimize(DateOnlyPeriod period, IEnumerable<IPerson> persons, IScheduleDictionary schedules, IScenario scenario, IOptimizationPreferences optimizationPreferences)
		{
			var maxSeatData = _maxSeatSkillDataFactory.Create(period, persons, scenario);
			var schedulingOptions = _schedulingOptionsCreator.CreateSchedulingOptions(optimizationPreferences);

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
								var scheduleDay = schedules[person].ScheduledDay(date);
								var contractTimeBefore = scheduleDay // don't do this every interval/skillstaffperiod
										.PersonAssignment(true)
										.ProjectionService()
										.CreateProjection()
										.ContractTime();

								_mainShiftOptimizeActivitySpecificationSetter.SetMainShiftOptimizeActivitySpecification(schedulingOptions, optimizationPreferences, scheduleDay.GetEditorShift(), date);
								
								foreach (var shift in _shiftProjectionCacheManager.ShiftProjectionCachesFromRuleSets(date, person.PermissionInformation.DefaultTimeZone(), person.Period(date).RuleSetBag, false, true))
								{
									var layerThisPeriod = shift.MainShiftProjection.SingleOrDefault(x => x.Period.Contains(skillStaffPeriod.Period)); 
									if ((layerThisPeriod == null || !((IActivity)layerThisPeriod.Payload).RequiresSeat) &&
										shift.MainShiftProjection.ContractTime() == contractTimeBefore &&
										isSatisfied(schedulingOptions.MainShiftOptimizeActivitySpecification, shift.MainShiftProjection)) //refactor when all shift tests are in place
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

		//remove when all shift tests are in place
		private bool isSatisfied(ISpecification<IEditableShift> specification, IVisualLayerCollection visualLayerCollection)
		{
			var mainShiftOptimizeActivitiesSpecification = specification as MainShiftOptimizeActivitiesSpecification;

			if (mainShiftOptimizeActivitiesSpecification == null)
				return true;

			if (!mainShiftOptimizeActivitiesSpecification.CorrectStart(visualLayerCollection))
				return false;

			if (!mainShiftOptimizeActivitiesSpecification.CorrectEnd(visualLayerCollection))
				return false;

			if (!mainShiftOptimizeActivitiesSpecification.CorrectAlteredBetween(visualLayerCollection))
				return false;

			return true;
		}
	}
}