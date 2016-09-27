using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.Common.TimeLogger;
using Teleopti.Ccc.Domain.Intraday;
using Teleopti.Ccc.Domain.Scheduling.WebLegacy;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ResourceCalculation
{
	public class CalculateForReadModel
	{
		private readonly LoaderForResourceCalculation _loaderForResourceCalculation;
		private readonly IResourceOptimizationHelper _resourceOptimizationHelper;
		private readonly IScheduleForecastSkillReadModelRepository _scheduleForecastSkillReadModelRepository;
	    private readonly INow _now;

		public CalculateForReadModel(
			LoaderForResourceCalculation loaderForResourceCalculation,
			IResourceOptimizationHelper resourceOptimizationHelper, IScheduleForecastSkillReadModelRepository scheduleForecastSkillReadModelRepository, INow now)
		{
			_loaderForResourceCalculation = loaderForResourceCalculation;
			_resourceOptimizationHelper = resourceOptimizationHelper;
			_scheduleForecastSkillReadModelRepository = scheduleForecastSkillReadModelRepository;
		    _now = now;
		}

		[LogTime]
		public virtual IEnumerable<ResourcesDataModel> ResourceCalculatePeriod(DateOnlyPeriod publishedPeriod)
		{

			_loaderForResourceCalculation.PreFillInformation( publishedPeriod);

			foreach (var day in publishedPeriod.DayCollection())
			{
				var period = new DateOnlyPeriod(day, day.AddDays(1));
				var resCalcData = _loaderForResourceCalculation.ResourceCalculationData(period);
				DoCalculation(period, resCalcData);

				var skillStaffPeriodDictionary = resCalcData.SkillStaffPeriodHolder.SkillSkillStaffPeriodDictionary;
				var model = CreateReadModel(skillStaffPeriodDictionary, day.Date);
				_scheduleForecastSkillReadModelRepository.Persist(model,day);
			}
			
			return null;
		}

	   [LogTime]
		public virtual void DoCalculation(DateOnlyPeriod period, IResourceCalculationData resCalcData)
		{
			foreach (var dateOnly in period.DayCollection())
			{
				_resourceOptimizationHelper.ResourceCalculate(dateOnly, resCalcData);
			}
		}

		
		[LogTime]
		public virtual IEnumerable<ResourcesDataModel> CreateReadModel(ISkillSkillStaffPeriodExtendedDictionary skillSkillStaffPeriodExtendedDictionary, DateTime date)
		{
			var items  = new List<ResourcesDataModel>(); 

			if (skillSkillStaffPeriodExtendedDictionary.Keys.Count > 0)
			{
			    var now = _now.UtcDateTime();
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
							Forecast = skillStaffPeriod.FStaff,
							StaffingLevel = skillStaffPeriod.CalculatedResource,
                            CalculatedOn = now,
                            ForecastWithShrinkage = skillStaffPeriod.ForecastedDistributedDemandWithShrinkage
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
        public DateTime CalculatedOn { get; set; }
	    public double ForecastWithShrinkage { get; set; }
	}
}