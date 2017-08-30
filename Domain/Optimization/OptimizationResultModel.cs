using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization
{
	[Serializable]
	public class OptimizationResultModel
	{
		private readonly IList<OptimizationResultSkill> _skillResultList = new List<OptimizationResultSkill>();
		public int ScheduledAgentsCount { get; set; }
		public IEnumerable<BusinessRulesValidationResult> BusinessRulesValidationResults { get; set; }

		public IEnumerable<OptimizationResultSkill> SkillResultList
		{
			get { return _skillResultList; }
		}

		public void Map(IDictionary<ISkill, IEnumerable<ISkillDay>> skillDays, DateOnlyPeriod period)
		{
			foreach (var keyValuePair in skillDays)
			{
				var skill = keyValuePair.Key;
				var forecastSource = skill.SkillType.ForecastSource;
				if(forecastSource == ForecastSource.MaxSeatSkill || forecastSource == ForecastSource.OutboundTelephony)
					continue;

				var item = new OptimizationResultSkill {SkillName = skill.Name};
				_skillResultList.Add(item);
				var skillDaysDic = keyValuePair.Value.ToDictionary(k => k.CurrentDate);
				foreach (var dateOnly in period.DayCollection())
				{
					ISkillDay skillDay;
					var found = skillDaysDic.TryGetValue(dateOnly, out skillDay);
					double relativeDifference = !found
						? 0
						: SkillStaffPeriodHelper.RelativeDifference(skillDay.SkillStaffPeriodCollection).GetValueOrDefault(0);
					var detail = new OptimizationResultSkillDetail
					{
						Date = dateOnly,
						RelativeDifference = relativeDifference
					};

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