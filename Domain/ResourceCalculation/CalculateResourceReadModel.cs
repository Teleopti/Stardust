using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Common.TimeLogger;
using Teleopti.Ccc.Domain.Intraday;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Domain.ResourceCalculation
{
	public class CalculateResourceReadModel
	{
		private readonly IScheduleForecastSkillReadModelRepository _scheduleForecastSkillReadModelRepository;
		private readonly IExtractSkillStaffDataForResourceCalcualtion _extractSkillStaffDataForResourceCalcualtion;
		private readonly INow _now;
		private readonly IStardustJobFeedback _feedback;

		public CalculateResourceReadModel( IScheduleForecastSkillReadModelRepository scheduleForecastSkillReadModelRepository, 
			INow now, IExtractSkillStaffDataForResourceCalcualtion extractSkillStaffDataForResourceCalcualtion, IStardustJobFeedback feedback)
		{
			_scheduleForecastSkillReadModelRepository = scheduleForecastSkillReadModelRepository;
		    _now = now;
			_extractSkillStaffDataForResourceCalcualtion = extractSkillStaffDataForResourceCalcualtion;
			_feedback = feedback;
		}

		[LogTime]
		public virtual void ResourceCalculatePeriod(DateTimePeriod period)
		{
			var periodDateOnly = new DateOnlyPeriod(new DateOnly(period.StartDateTime), new DateOnly(period.EndDateTime));
			_feedback.SendProgress?.Invoke($"Starting Read Model update for period {period}");
			var timeWhenResourceCalcDataLoaded = _now.UtcDateTime();
			var skillStaffPeriodDictionary =
				_extractSkillStaffDataForResourceCalcualtion.ExtractSkillStaffPeriodDictionary(periodDateOnly);
			var models = CreateReadModel(skillStaffPeriodDictionary, period);
			_scheduleForecastSkillReadModelRepository.Persist(models, timeWhenResourceCalcDataLoaded);
		}
		
		
		[LogTime]
		public virtual IEnumerable<ResourcesDataModel> CreateReadModel(ISkillSkillStaffPeriodExtendedDictionary skillSkillStaffPeriodExtendedDictionary, DateTimePeriod period)
		{
			var items  = new List<ResourcesDataModel>(); 
			_feedback.SendProgress?.Invoke($"Will update {skillSkillStaffPeriodExtendedDictionary.Keys.Count} skills.");
			if (skillSkillStaffPeriodExtendedDictionary.Keys.Count > 0)
			{
                foreach (var skill in skillSkillStaffPeriodExtendedDictionary.Keys)
				{
					
					var ret = new ResourcesDataModel();
					ret.Id = skill.Id.GetValueOrDefault();
					ret.Intervals = new List<SkillStaffingInterval>();
					foreach (var skillStaffPeriod in skillSkillStaffPeriodExtendedDictionary[skill].Values)
					{
						if ( !period.Contains(skillStaffPeriod.Period.StartDateTime) )
							continue;
						ret.Intervals.Add(new SkillStaffingInterval
						{
							StartDateTime = skillStaffPeriod.Period.StartDateTime,
							EndDateTime = skillStaffPeriod.Period.EndDateTime,
							Forecast = skillStaffPeriod.FStaff,
							StaffingLevel = skillStaffPeriod.CalculatedResource,
                            ForecastWithShrinkage = skillStaffPeriod.ForecastedDistributedDemandWithShrinkage
                        });
					}
					_feedback.SendProgress?.Invoke($"Updated {skill}.");
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
	    public double ForecastWithShrinkage { get; set; }
	}

    public class StaffingIntervalChange :  IEquatable<StaffingIntervalChange> 
	{
        public Guid SkillId { get; set; }
        public DateTime StartDateTime { get; set; }
        public DateTime EndDateTime { get; set; }
        public double StaffingLevel { get; set; }


		public bool Equals(StaffingIntervalChange other)
		{
			if (other == null) return false;
			return SkillId == other.SkillId && StartDateTime == other.StartDateTime && EndDateTime == other.EndDateTime && Math.Abs(StaffingLevel - other.StaffingLevel) < 0.001;
		}
    }
}