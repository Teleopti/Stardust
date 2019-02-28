using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.ResourcePlanner.Hints;

namespace Teleopti.Ccc.Domain.Optimization
{
	[Serializable]
	public class FullSchedulingResultModel
	{
		private readonly IList<FullSchedulingResultSkill> _skillResultList = new List<FullSchedulingResultSkill>();
		public int ScheduledAgentsCount { get; set; }
		public IEnumerable<SchedulingHintError> BusinessRulesValidationResults { get; set; }

		public IEnumerable<FullSchedulingResultSkill> SkillResultList => _skillResultList;

		public void Map(IDictionary<ISkill, IEnumerable<ISkillDay>> skillDays, DateOnlyPeriod period)
		{
			foreach (var keyValuePair in skillDays)
			{
				var skill = keyValuePair.Key;
				var forecastSource = skill.SkillType.ForecastSource;
				if(forecastSource == ForecastSource.MaxSeatSkill || forecastSource == ForecastSource.OutboundTelephony)
					continue;

				var item = new FullSchedulingResultSkill {SkillName = skill.Name};
				_skillResultList.Add(item);
				var skillDaysDic = keyValuePair.Value.ToDictionary(k => k.CurrentDate);
				foreach (var dateOnly in period.DayCollection())
				{
					var found = skillDaysDic.TryGetValue(dateOnly, out var skillDay);
					double relativeDifference;
					if (!found){
						relativeDifference = 0;
					}
					else
					{
						foreach (var skillStaffPeriod in skillDay.SkillStaffPeriodCollection)
						{
							skillStaffPeriod.Payload.UseShrinkage = true;
						}
						relativeDifference = SkillStaffPeriodHelper
							.RelativeDifference(skillDay.SkillStaffPeriodCollection).GetValueOrDefault(0);
					}

					var detail = new FullSchedulingResultSkillDetail
					{
						Date = dateOnly,
						RelativeDifference = Math.Round(relativeDifference, 4)
					};

					if (found)
					{
						detail.IntervalDetails = skillDay.SkillStaffPeriodCollection.Select(x => new IntervalDetail
						{
							StartTime = TimeZoneHelper.ConvertFromUtc(x.Period.StartDateTime, skill.TimeZone).TimeOfDay,
							ScheduledAgents = Math.Round(x.CalculatedResource, 2),
							ForecastAgents = Math.Round(x.ForecastedDistributedDemand, 2)
						});
					}

					detail.ColorId = skillDay != null && skillDay.OpenForWork.IsOpen ? mapColorId(detail.RelativeDifference, skill) : 4;
					item.AddDetail(detail);
				}
			}
		}

		private static int mapColorId(double relativeDifference, ISkill skill)
		{
			if (relativeDifference > skill.StaffingThresholds.Overstaffing.Value)
				return 3;

			if (relativeDifference < skill.StaffingThresholds.SeriousUnderstaffing.Value)
				return 2;

			if (relativeDifference < skill.StaffingThresholds.Understaffing.Value)
				return 1;

			return 0;
		}

		public class FullSchedulingResultSkill
		{
			private readonly IList<FullSchedulingResultSkillDetail> _skillDetails = new List<FullSchedulingResultSkillDetail>();
			public string SkillName { get; set; }

			public IEnumerable<FullSchedulingResultSkillDetail> SkillDetails => _skillDetails;

			public void AddDetail(FullSchedulingResultSkillDetail detail)
			{
				_skillDetails.Add(detail);
			}
		}

		public struct FullSchedulingResultSkillDetail
		{
			public DateOnly Date { get; set; }
			public double RelativeDifference { get; set; }
			public int ColorId { get; set; }
			public IEnumerable<IntervalDetail> IntervalDetails { get; set; }
		}

	}

	public class IntervalDetail
	{
		[JsonProperty(PropertyName = "s")]
		public double ScheduledAgents { get; set; }
		
		[JsonProperty(PropertyName = "f")]
		public double ForecastAgents { get; set; }
		
		[JsonProperty(PropertyName = "x")]
		public TimeSpan StartTime { get; set; }
	}
}