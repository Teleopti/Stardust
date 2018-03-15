using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.ResourcePlanner.Hints;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization
{
	[Serializable]
	public class OptimizationResultModel
	{
		private readonly IList<OptimizationResultSkill> _skillResultList = new List<OptimizationResultSkill>();
		public int ScheduledAgentsCount { get; set; }
		public IEnumerable<SchedulingHintError> BusinessRulesValidationResults { get; set; }

		public IEnumerable<OptimizationResultSkill> SkillResultList
		{
			get { return _skillResultList; }
		}

		public void Map(IDictionary<ISkill, IEnumerable<ISkillDay>> skillDays, DateOnlyPeriod period)
		{
			var timezoneToSaveToDb = TimeZoneInfo.Utc; //this is actually wrong, but maybe good enough for now... (due to stored data per day and not per interval)
			foreach (var keyValuePair in skillDays)
			{
				var skill = keyValuePair.Key;
				var forecastSource = skill.SkillType.ForecastSource;
				if(forecastSource == ForecastSource.MaxSeatSkill || forecastSource == ForecastSource.OutboundTelephony)
					continue;

				var item = new OptimizationResultSkill {SkillName = skill.Name};
				_skillResultList.Add(item);
				foreach (var dateOnly in period.DayCollection())
				{
					var periodInAgentTimeZone = dateOnly.ToDateOnlyPeriod().ToDateTimePeriod(timezoneToSaveToDb);
					var skillStaffPeriods = new List<ISkillStaffPeriod>();

					foreach (var skillDay in skillDays[skill])
					{
						foreach (var skillStaffPeriod in skillDay.SkillStaffPeriodCollection)
						{
							if (periodInAgentTimeZone.Contains(skillStaffPeriod.Period))
							{
								skillStaffPeriods.Add(skillStaffPeriod);
							}
						}
					}

					var detail = new OptimizationResultSkillDetail
					{
						Date = dateOnly,
						RelativeDifference = SkillStaffPeriodHelper.RelativeDifference(skillStaffPeriods).GetValueOrDefault(0)
					};

					//this is wrong. will not check if skill is open correctly. 
					//add test + fix when needed
					var oneOfTheSkillDays = skillDays[skill].FirstOrDefault(); 
					detail.ColorId = oneOfTheSkillDays != null && oneOfTheSkillDays.OpenForWork.IsOpen ? mapColorId(detail.RelativeDifference, skill) : 4;
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

		public class OptimizationResultSkill
		{
			private readonly IList<OptimizationResultSkillDetail> _skillDetails = new List<OptimizationResultSkillDetail>();
			public string SkillName { get; set; }

			public IEnumerable<OptimizationResultSkillDetail> SkillDetails
			{
				get { return _skillDetails; }
			}

			public void AddDetail(OptimizationResultSkillDetail detail)
			{
				_skillDetails.Add(detail);
			}
		}

		public struct OptimizationResultSkillDetail
		{
			public DateOnly Date { get; set; }
			public double RelativeDifference { get; set; }
			public int ColorId { get; set; }
		}

	}
}