using System;
using System.Collections.Generic;
using Teleopti.Analytics.Etl.Common.Interfaces.Transformer;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Analytics.Etl.Common.Transformer
{
	public class ScheduleForecastSkillResourceCalculation : IScheduleForecastSkillResourceCalculation
	{
		private readonly IDictionary<ISkill, IList<ISkillDay>> _skillDaysDictionary;
		private readonly ISchedulingResultService _schedulingResultService;
		private readonly IList<ISkillStaffPeriod> _skillStaffPeriodCollection;
		private Dictionary<IScheduleForecastSkillKey, IScheduleForecastSkill> _scheduleForecastSkillDictionary;
		private readonly int _intervalsPerDay;
		private readonly DateTimePeriod _period;

		private ScheduleForecastSkillResourceCalculation()
		{

		}

		public ScheduleForecastSkillResourceCalculation(IDictionary<ISkill, IList<ISkillDay>> skillDaysDictionary, ISchedulingResultService schedulingResultService, IList<ISkillStaffPeriod> skillStaffPeriodCollection, int intervalsPerDay, DateTimePeriod period)
			: this()
		{
			_skillDaysDictionary = skillDaysDictionary;
			_intervalsPerDay = intervalsPerDay;
			_period = period;
			_schedulingResultService = schedulingResultService;
			_skillStaffPeriodCollection = skillStaffPeriodCollection;
		}

		public Dictionary<IScheduleForecastSkillKey, IScheduleForecastSkill> GetResourceDataExcludingShrinkage(DateTime insertDateTime)
		{
			setResourceData(false, insertDateTime);
			return _scheduleForecastSkillDictionary;
		}

		public Dictionary<IScheduleForecastSkillKey, IScheduleForecastSkill> GetResourceDataIncludingShrinkage(DateTime insertDateTime)
		{
			setResourceData(true, insertDateTime);
			return _scheduleForecastSkillDictionary;
		}

		private void setResourceData(bool useShrinkage, DateTime insertDateTime)
		{
			initializeScheduleForecastSkillDictionary();

			foreach (ISkillStaffPeriod skillStaffPeriod in _skillStaffPeriodCollection)
			{
				skillStaffPeriod.Payload.UseShrinkage = useShrinkage;
			}

			_schedulingResultService.SchedulingResult(_period);

			collectResourceData(_skillDaysDictionary, useShrinkage, insertDateTime);
		}

		private void initializeScheduleForecastSkillDictionary()
		{
			if (_scheduleForecastSkillDictionary == null)
			{
				_scheduleForecastSkillDictionary = new Dictionary<IScheduleForecastSkillKey, IScheduleForecastSkill>(new ScheduleForecastSkillEqualComparer());
			}
		}

		private void collectResourceData(IEnumerable<KeyValuePair<ISkill, IList<ISkillDay>>> skillDaysDictionary, bool useShrinkage, DateTime insertDateTime)
		{
			int minutesPerInterval = 1440 / _intervalsPerDay;

			foreach (KeyValuePair<ISkill, IList<ISkillDay>> skill in skillDaysDictionary)
			{
				foreach (ISkillDay skillDay in skill.Value)
				{
					foreach (ISkillStaffPeriodView skillStaffPeriodView in skillDay.SkillStaffPeriodViewCollection(new TimeSpan(0, minutesPerInterval, 0)))
					{
						IScheduleForecastSkillKey scheduleForecastSkillKey =
							 new ScheduleForecastSkillKey(skillStaffPeriodView.Period.StartDateTime,
																new Interval(skillStaffPeriodView.Period.StartDateTime,
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
															 skillDay.Skill.BusinessUnit, insertDateTime, useShrinkage);

							_scheduleForecastSkillDictionary.Add(scheduleForecastSkillKey, scheduleForecastSkill);
						}
						else
						{
							// Update existing records in dictionary
							setScheduleForecastSkill(scheduleForecastSkill, skillStaffPeriodView,
															 skillDay.Skill.BusinessUnit, insertDateTime, useShrinkage);
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
			}
			else
			{
				scheduleForecastSkill.ForecastedResourcesMinutes = skillStaffPeriodView.FStaff * minutesPerInterval;
				scheduleForecastSkill.ForecastedResources = skillStaffPeriodView.FStaff;
				scheduleForecastSkill.ScheduledResourcesMinutes = skillStaffPeriodView.CalculatedResource * minutesPerInterval;
				scheduleForecastSkill.ScheduledResources = skillStaffPeriodView.CalculatedResource;

				scheduleForecastSkill.BusinessUnitCode = (Guid)businessUnit.Id;
				scheduleForecastSkill.BusinessUnitName = businessUnit.Name;
				scheduleForecastSkill.DataSourceId = 1;
				scheduleForecastSkill.InsertDate = insertDateTime;
				scheduleForecastSkill.UpdateDate = insertDateTime;
			}
		}

	}
}
