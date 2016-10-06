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
		private readonly IExtractSkillStaffDataForResourceCalculation _extractSkillStaffDataForResourceCalculation;
		private readonly INow _now;
		private readonly IStardustJobFeedback _feedback;

		public CalculateResourceReadModel(IScheduleForecastSkillReadModelRepository scheduleForecastSkillReadModelRepository,
			INow now, IExtractSkillStaffDataForResourceCalculation extractSkillStaffDataForResourceCalculation, IStardustJobFeedback feedback)
		{
			_scheduleForecastSkillReadModelRepository = scheduleForecastSkillReadModelRepository;
			_now = now;
			_extractSkillStaffDataForResourceCalculation = extractSkillStaffDataForResourceCalculation;
			_feedback = feedback;
		}

		[LogTime]
		public virtual void ResourceCalculatePeriod(DateTimePeriod period)
		{
			var periodDateOnly = new DateOnlyPeriod(new DateOnly(period.StartDateTime), new DateOnly(period.EndDateTime));
			_feedback.SendProgress?.Invoke($"Starting Read Model update for period {period}");
			var timeWhenResourceCalcDataLoaded = _now.UtcDateTime();
			var skillStaffPeriodDictionary =
				_extractSkillStaffDataForResourceCalculation.ExtractSkillStaffPeriodDictionary(periodDateOnly);
			var models = CreateReadModel(skillStaffPeriodDictionary, period);
			_scheduleForecastSkillReadModelRepository.Persist(models, timeWhenResourceCalcDataLoaded);
		}


		[LogTime]
		public virtual IEnumerable<SkillStaffingInterval> CreateReadModel(ISkillSkillStaffPeriodExtendedDictionary skillSkillStaffPeriodExtendedDictionary, DateTimePeriod period)
		{
			var ret = new List<SkillStaffingInterval>();
			_feedback.SendProgress?.Invoke($"Will update {skillSkillStaffPeriodExtendedDictionary.Keys.Count} skills.");
			if (skillSkillStaffPeriodExtendedDictionary.Keys.Count > 0)
			{
				foreach (var skill in skillSkillStaffPeriodExtendedDictionary.Keys)
				{
					foreach (var skillStaffPeriod in skillSkillStaffPeriodExtendedDictionary[skill].Values)
					{
						if (!period.Contains(skillStaffPeriod.Period.StartDateTime))
							continue;
						ret.Add(new SkillStaffingInterval
						{
							SkillId = skill.Id.GetValueOrDefault(),
							StartDateTime = skillStaffPeriod.Period.StartDateTime,
							EndDateTime = skillStaffPeriod.Period.EndDateTime,
							Forecast = skillStaffPeriod.FStaff,
							StaffingLevel = skillStaffPeriod.CalculatedResource,
							ForecastWithShrinkage = skillStaffPeriod.ForecastedDistributedDemandWithShrinkage
						});
					}
					_feedback.SendProgress?.Invoke($"Updated {skill}.");
				}
			}
			return ret;
		}
	}

	public class SkillStaffingInterval
	{
		public Guid SkillId { get; set; }
		public DateTime StartDateTime { get; set; }
		public DateTime EndDateTime { get; set; }
		public double Forecast { get; set; }
		public double StaffingLevel { get; set; }
		public double ForecastWithShrinkage { get; set; }

		public TimeSpan GetTimeSpan()
		{
			return EndDateTime.Subtract(StartDateTime);
		}

		public double divideBy(TimeSpan ts)
		{
			return (double)GetTimeSpan().Ticks/ts.Ticks;
		}
	}

	public class StaffingIntervalChange : IEquatable<StaffingIntervalChange>
	{
		public Guid SkillId { get; set; }
		public DateTime StartDateTime { get; set; }
		public DateTime EndDateTime { get; set; }
		public double StaffingLevel { get; set; }


		public bool Equals(StaffingIntervalChange other)
		{
			if (other == null) return false;
			return SkillId == other.SkillId
					&& StartDateTime == other.StartDateTime
					&& EndDateTime == other.EndDateTime
					&& Math.Abs(StaffingLevel - other.StaffingLevel) < 0.001;
		}
	}
}