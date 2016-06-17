using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.Common.TimeLogger;
using Teleopti.Ccc.Domain.Intraday;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Ccc.Domain.Scheduling.WebLegacy;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ResourceCalculation
{
	public class CalculateForReadModel
	{
		private readonly FillSchedulerStateHolderForResourceCalculation _fillSchedulerStateHolderForResourceCalculation;
		private readonly Func<ISchedulerStateHolder> _schedulerStateHolder;
		private readonly IResourceOptimizationHelper _resourceOptimizationHelper;
		private readonly IScheduleForecastSkillReadModelPersister _scheduleForecastSkillReadModelPersister;

		public CalculateForReadModel(
			FillSchedulerStateHolderForResourceCalculation fillSchedulerStateHolderForResourceCalculation,
			Func<ISchedulerStateHolder> schedulerStateHolder,
			IResourceOptimizationHelper resourceOptimizationHelper, IScheduleForecastSkillReadModelPersister scheduleForecastSkillReadModelPersister)
		{
			_fillSchedulerStateHolderForResourceCalculation = fillSchedulerStateHolderForResourceCalculation;
			_schedulerStateHolder = schedulerStateHolder;
			_resourceOptimizationHelper = resourceOptimizationHelper;
			_scheduleForecastSkillReadModelPersister = scheduleForecastSkillReadModelPersister;
		}

		[LogTime]
		public virtual IEnumerable<ResourcesDataModel> ResourceCalculatePeriod(DateOnlyPeriod publishedPeriod)
		{
			var stateHolder = _schedulerStateHolder();

			_fillSchedulerStateHolderForResourceCalculation.PreFillInformation(stateHolder, publishedPeriod);

			foreach (var day in publishedPeriod.DayCollection())
			{
				var period = new DateOnlyPeriod(day, day.AddDays(1));
				_fillSchedulerStateHolderForResourceCalculation.Fill(stateHolder, period);

				DoCalculation(period);

				var skillStaffPeriodDictionary =
					stateHolder.SchedulingResultState.SkillStaffPeriodHolder.SkillSkillStaffPeriodDictionary;
				var model = CreateReadModel(skillStaffPeriodDictionary, day.Date);
				_scheduleForecastSkillReadModelPersister.Persist(model,day);
			}
			
			return null;
		}

	   [LogTime]
		public virtual void DoCalculation(DateOnlyPeriod period)
		{
			foreach (var dateOnly in period.DayCollection())
			{
				_resourceOptimizationHelper.ResourceCalculateDate(dateOnly, true, true);
			}
		}

		
		[LogTime]
		public virtual IEnumerable<ResourcesDataModel> CreateReadModel(ISkillSkillStaffPeriodExtendedDictionary skillSkillStaffPeriodExtendedDictionary, DateTime date)
		{
			var items  = new List<ResourcesDataModel>(); 

			//just return first skill for now
			if (skillSkillStaffPeriodExtendedDictionary.Keys.Count > 0)
			{
				foreach (var skill in skillSkillStaffPeriodExtendedDictionary.Keys)
				{
					var ret = new ResourcesDataModel();
					ret.Id = skill.Id.GetValueOrDefault();
					ret.Intervals = new List<SkillStaffingInterval>();
					foreach (var skillStaffPeriod in skillSkillStaffPeriodExtendedDictionary[skill].Values)
					{
						if (  skillStaffPeriod.Period.StartDateTime.Date != date )
							continue;
						ret.Intervals.Add(new SkillStaffingInterval
						{
							StartDateTime = skillStaffPeriod.Period.StartDateTime,
							EndDateTime = skillStaffPeriod.Period.EndDateTime,
							Forecast = skillStaffPeriod.ForecastedDistributedDemandWithShrinkage,
							StaffingLevel = skillStaffPeriod.CalculatedResource
						});
					}
					items.Add(ret);
				}
			}

			return items;
		}
	}


	
	public class ResourcesDataModel
	{
		public Guid Id { get; set; }
		public string Area { get; set; }
		public List<SkillStaffingInterval> Intervals { get; set; }
	}

	public class SkillStaffingInterval
	{
		public DateTime StartDateTime { get; set; }
		public DateTime EndDateTime { get; set; }
		public double Forecast { get; set; }
		public double StaffingLevel { get; set; }
	}
}