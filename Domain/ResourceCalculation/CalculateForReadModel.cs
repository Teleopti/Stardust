using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common.TimeLogger;
using Teleopti.Ccc.Domain.Logon;
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

		public CalculateForReadModel(
			FillSchedulerStateHolderForResourceCalculation fillSchedulerStateHolderForResourceCalculation,
			Func<ISchedulerStateHolder> schedulerStateHolder,
			IResourceOptimizationHelper resourceOptimizationHelper)
		{
			_fillSchedulerStateHolderForResourceCalculation = fillSchedulerStateHolderForResourceCalculation;
			_schedulerStateHolder = schedulerStateHolder;
			_resourceOptimizationHelper = resourceOptimizationHelper;
		}

		[LogTime]
		[UnitOfWork]
		public virtual ResourcesDataModel ResourceCalculatePeriod(DateOnlyPeriod period)
		{
			var stateHolder = _schedulerStateHolder();
			_fillSchedulerStateHolderForResourceCalculation.Fill(stateHolder, null, null, null, period);
			foreach (var dateOnly in period.DayCollection())
			{
				_resourceOptimizationHelper.ResourceCalculateDate(dateOnly,true,true);
			}

			return createReadModel(stateHolder.SchedulingResultState.SkillStaffPeriodHolder.SkillSkillStaffPeriodDictionary,period);
		}

		private ResourcesDataModel createReadModel(ISkillSkillStaffPeriodExtendedDictionary skillSkillStaffPeriodExtendedDictionary, DateOnlyPeriod period)
		{
			var ret = new ResourcesDataModel();
			//just return first skill for now
			if (skillSkillStaffPeriodExtendedDictionary.Keys.Count > 0)
			{
				var skill = skillSkillStaffPeriodExtendedDictionary.Keys.First();
				ret.Id = skill.Id.GetValueOrDefault();
				ret.Area = skill.Name;
				ret.Intervals = new List<SkillStaffingInterval>();
				foreach (var skillStaffPeriod in skillSkillStaffPeriodExtendedDictionary[skill].Values)
				{
					if(skillStaffPeriod.Period.StartDateTime < period.StartDate.Date || skillStaffPeriod.Period.StartDateTime > period.EndDate.Date)
						continue;
					ret.Intervals.Add(new SkillStaffingInterval
					{
						StartDateTime = skillStaffPeriod.Period.StartDateTime,
						EndDateTime = skillStaffPeriod.Period.EndDateTime,
						Forecast = skillStaffPeriod.ForecastedDistributedDemandWithShrinkage,
						StaffingLevel = skillStaffPeriod.CalculatedResource
					});
				}
			}
	
			return ret;
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