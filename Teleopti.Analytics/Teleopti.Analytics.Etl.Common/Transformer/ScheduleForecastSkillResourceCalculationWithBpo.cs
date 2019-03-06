using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Analytics.Etl.Common.Interfaces.Transformer;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.ResourceCalculation;


namespace Teleopti.Analytics.Etl.Common.Transformer
{
	public class ScheduleForecastSkillResourceCalculationWithBpo : IScheduleForecastSkillResourceCalculation
	{
		private Dictionary<IScheduleForecastSkillKey, IScheduleForecastSkill> _scheduleForecastSkillDictionary;
		private readonly int _intervalsPerDay;
		private readonly DateTimePeriod _period;
		private readonly CascadingResourceCalculationContextFactory _cascadingResourceCalculationContextFactory;
		private readonly IEnumerable<ExternalStaff> _externalStaff;
		private readonly IResourceCalculation _resourceCalculation;

		public ScheduleForecastSkillResourceCalculationWithBpo(int intervalsPerDay,
			DateTimePeriod period,
			CascadingResourceCalculationContextFactory cascadingResourceCalculationContextFactory,
			IEnumerable<ExternalStaff> externalStaff,
			IResourceCalculation resourceCalculation)
		{
			_intervalsPerDay = intervalsPerDay;
			_period = period;
			_cascadingResourceCalculationContextFactory = cascadingResourceCalculationContextFactory;
			_externalStaff = externalStaff;
			_resourceCalculation = resourceCalculation;
		}

		public Dictionary<IScheduleForecastSkillKey, IScheduleForecastSkill> GetResourceDataExcludingShrinkage(DateTime insertDateTime, ISchedulingResultStateHolder schedulingResultStateHolder)
		{
			setResourceData(false, insertDateTime, schedulingResultStateHolder);
			return _scheduleForecastSkillDictionary;
		}

		public Dictionary<IScheduleForecastSkillKey, IScheduleForecastSkill> GetResourceDataIncludingShrinkage(DateTime insertDateTime, ISchedulingResultStateHolder schedulingResultStateHolder)
		{
			setResourceData(true, insertDateTime, schedulingResultStateHolder);
			return _scheduleForecastSkillDictionary;
		}

		private void setResourceData(bool useShrinkage, DateTime insertDateTime, ISchedulingResultStateHolder schedulingResultStateHolder)
		{
			initializeScheduleForecastSkillDictionary();

			var skillsWithSkillDays = schedulingResultStateHolder.Skills;
			foreach (var skillStaffPeriod in schedulingResultStateHolder.SkillStaffPeriodHolder.SkillStaffPeriodList(skillsWithSkillDays, schedulingResultStateHolder.Schedules.Period.VisiblePeriod))
			{
				skillStaffPeriod.Payload.UseShrinkage = useShrinkage;
			}

			var dateOnlyPeriodInUtc = _period.ToDateOnlyPeriod(TimeZoneInfo.Utc); //don't know if correct - copied from StageScheduleForecastSkillJobStep when getting skills
			var notPrimarySkillMode = !skillsWithSkillDays.Any(s => s.IsCascading());
			using (_cascadingResourceCalculationContextFactory.Create(schedulingResultStateHolder.Schedules, skillsWithSkillDays, _externalStaff, notPrimarySkillMode, dateOnlyPeriodInUtc))
			{
				_resourceCalculation.ResourceCalculate(dateOnlyPeriodInUtc, schedulingResultStateHolder.ToResourceOptimizationData(true,true));
			}
	
			collectResourceData(schedulingResultStateHolder.SkillDays, useShrinkage, insertDateTime);
		}

		private void initializeScheduleForecastSkillDictionary()
		{
			if (_scheduleForecastSkillDictionary == null)
			{
				_scheduleForecastSkillDictionary = new Dictionary<IScheduleForecastSkillKey, IScheduleForecastSkill>(new ScheduleForecastSkillEqualComparer());
			}
		}

		private void collectResourceData(IEnumerable<KeyValuePair<ISkill, IEnumerable<ISkillDay>>> skillDaysDictionary, bool useShrinkage, DateTime insertDateTime)
		{
			int minutesPerInterval = 1440 / _intervalsPerDay;

			foreach (KeyValuePair<ISkill, IEnumerable<ISkillDay>> skill in skillDaysDictionary)
			{
				foreach (ISkillDay skillDay in skill.Value)
				{
					foreach (ISkillStaffPeriodView skillStaffPeriodView in skillDay.SkillStaffPeriodViewCollection(new TimeSpan(0, minutesPerInterval, 0), useShrinkage))
					{
						IScheduleForecastSkillKey scheduleForecastSkillKey =
							 new ScheduleForecastSkillKey(skillStaffPeriodView.Period.StartDateTime,
																new IntervalBase(skillStaffPeriodView.Period.StartDateTime,
																				 _intervalsPerDay).Id, skillDay.Skill.Id.GetValueOrDefault(),
																skillDay.Scenario.Id.GetValueOrDefault());

						_scheduleForecastSkillDictionary.TryGetValue(scheduleForecastSkillKey, out var scheduleForecastSkill);
						if (scheduleForecastSkill == null)
						{
							// Add new records to dictionary
							scheduleForecastSkill =
							new ScheduleForecastSkill(scheduleForecastSkillKey.StartDateTime, scheduleForecastSkillKey.IntervalId,
															  scheduleForecastSkillKey.SkillCode,
															  scheduleForecastSkillKey.ScenarioCode);

							setScheduleForecastSkill(scheduleForecastSkill, skillStaffPeriodView,
															 skillDay.Skill.GetOrFillWithBusinessUnit_DONTUSE(), insertDateTime, useShrinkage);

							_scheduleForecastSkillDictionary.Add(scheduleForecastSkillKey, scheduleForecastSkill);
						}
						else
						{
							// Update existing records in dictionary
							setScheduleForecastSkill(scheduleForecastSkill, skillStaffPeriodView,
															 skillDay.Skill.GetOrFillWithBusinessUnit_DONTUSE(), insertDateTime, useShrinkage);
						}
					}
				}
			}
		}

		private static void setScheduleForecastSkill(IScheduleForecastSkill scheduleForecastSkill, ISkillStaffPeriodView skillStaffPeriodView, IBusinessUnit businessUnit, DateTime insertDateTime, bool useShrinkage)
		{
			double minutesPerInterval = skillStaffPeriodView.Period.ElapsedTime().TotalMinutes;
			if (useShrinkage)
			{
				scheduleForecastSkill.ForecastedResourcesIncludingShrinkageMinutes = skillStaffPeriodView.FStaff * minutesPerInterval;
				scheduleForecastSkill.ForecastedResourcesIncludingShrinkage = skillStaffPeriodView.FStaff;
				scheduleForecastSkill.ScheduledResourcesIncludingShrinkageMinutes = skillStaffPeriodView.CalculatedResource * minutesPerInterval;
				scheduleForecastSkill.ScheduledResourcesIncludingShrinkage = skillStaffPeriodView.CalculatedResource;
				scheduleForecastSkill.EstimatedTasksAnsweredWithinSLIncludingShrinkage = skillStaffPeriodView.EstimatedServiceLevelShrinkage.Value * skillStaffPeriodView.ForecastedTasks;
				scheduleForecastSkill.ForecastedTasksIncludingShrinkage = skillStaffPeriodView.ForecastedTasks;
			}
			else
			{
				scheduleForecastSkill.ForecastedResourcesMinutes = skillStaffPeriodView.FStaff * minutesPerInterval;
				scheduleForecastSkill.ForecastedResources = skillStaffPeriodView.FStaff;
				scheduleForecastSkill.ScheduledResourcesMinutes = skillStaffPeriodView.CalculatedResource * minutesPerInterval;
				scheduleForecastSkill.ScheduledResources = skillStaffPeriodView.CalculatedResource;
				scheduleForecastSkill.EstimatedTasksAnsweredWithinSL = skillStaffPeriodView.EstimatedServiceLevel.Value * skillStaffPeriodView.ForecastedTasks;
				scheduleForecastSkill.ForecastedTasks = skillStaffPeriodView.ForecastedTasks;

				scheduleForecastSkill.BusinessUnitCode = (Guid)businessUnit.Id;
				scheduleForecastSkill.BusinessUnitName = businessUnit.Name;
				scheduleForecastSkill.DataSourceId = 1;
				scheduleForecastSkill.InsertDate = insertDateTime;
				scheduleForecastSkill.UpdateDate = insertDateTime;
			}
		}
	}
}
