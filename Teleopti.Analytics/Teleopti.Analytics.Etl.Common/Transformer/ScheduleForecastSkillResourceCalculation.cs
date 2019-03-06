using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Analytics.Etl.Common.Interfaces.Transformer;
using Teleopti.Ccc.Domain.Cascading;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.ResourceCalculation;


namespace Teleopti.Analytics.Etl.Common.Transformer
{
	public class ScheduleForecastSkillResourceCalculation : IScheduleForecastSkillResourceCalculation
	{
		private readonly ShovelResources _shovelResources;
		private readonly IDictionary<ISkill, IEnumerable<ISkillDay>> _skillDaysDictionary;
		private readonly ISchedulingResultService _schedulingResultService;
		private readonly ISkillStaffPeriodHolder _skillStaffPeriodHolder;
		private readonly IScheduleDictionary _scheduleDictionary;
		private readonly IEnumerable<ISkill> _skillsWithSkillDays;
		private Dictionary<IScheduleForecastSkillKey, IScheduleForecastSkill> _scheduleForecastSkillDictionary;
		private readonly int _intervalsPerDay;
		private readonly DateTimePeriod _period;
		private readonly CascadingResourceCalculationContextFactory _cascadingResourceCalculationContextFactory;

		public ScheduleForecastSkillResourceCalculation(ShovelResources shovelResources,
									IDictionary<ISkill, IEnumerable<ISkillDay>> skillDaysDictionary, 
									ISchedulingResultService schedulingResultService, 
									ISkillStaffPeriodHolder skillStaffPeriodHolder,
									IScheduleDictionary scheduleDictionary,
									IEnumerable<ISkill> skillsWithSkillDays,
									int intervalsPerDay, 
									DateTimePeriod period,
									CascadingResourceCalculationContextFactory cascadingResourceCalculationContextFactory)
		{
			_shovelResources = shovelResources;
			_skillDaysDictionary = skillDaysDictionary;
			_intervalsPerDay = intervalsPerDay;
			_period = period;
			_cascadingResourceCalculationContextFactory = cascadingResourceCalculationContextFactory;
			_schedulingResultService = schedulingResultService;
			_skillStaffPeriodHolder = skillStaffPeriodHolder;
			_scheduleDictionary = scheduleDictionary;
			_skillsWithSkillDays = skillsWithSkillDays;
		}

		public Dictionary<IScheduleForecastSkillKey, IScheduleForecastSkill> GetResourceDataExcludingShrinkage(DateTime insertDateTime, ISchedulingResultStateHolder schedulingResultStateHolder)
		{
			setResourceData(false, insertDateTime);
			return _scheduleForecastSkillDictionary;
		}

		public Dictionary<IScheduleForecastSkillKey, IScheduleForecastSkill> GetResourceDataIncludingShrinkage(DateTime insertDateTime, ISchedulingResultStateHolder schedulingResultStateHolder)
		{
			setResourceData(true, insertDateTime);
			return _scheduleForecastSkillDictionary;
		}

		private void setResourceData(bool useShrinkage, DateTime insertDateTime)
		{
			initializeScheduleForecastSkillDictionary();

			
			foreach (ISkillStaffPeriod skillStaffPeriod in _skillStaffPeriodHolder.SkillStaffPeriodList(_skillsWithSkillDays, _scheduleDictionary.Period.VisiblePeriod))
			{
				skillStaffPeriod.Payload.UseShrinkage = useShrinkage;
			}

			var dateOnlyPeriodInUtc = _period.ToDateOnlyPeriod(TimeZoneInfo.Utc); //don't know if correct - copied from StageScheduleForecastSkillJobStep when getting skills
			using (_cascadingResourceCalculationContextFactory.Create(_scheduleDictionary, _skillsWithSkillDays, Enumerable.Empty<ExternalStaff>(), false, dateOnlyPeriodInUtc))
			{
				_schedulingResultService.SchedulingResult(_period);
				_shovelResources.Execute(new SkillResourceCalculationPeriodWrapper(_skillStaffPeriodHolder.SkillSkillStaffPeriodDictionary), _scheduleDictionary, _skillsWithSkillDays, dateOnlyPeriodInUtc,new NoShovelingCallback(), null);
			}
	
			collectResourceData(_skillDaysDictionary, useShrinkage, insertDateTime);
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
						IScheduleForecastSkill scheduleForecastSkill;

						_scheduleForecastSkillDictionary.TryGetValue(scheduleForecastSkillKey, out scheduleForecastSkill);
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
