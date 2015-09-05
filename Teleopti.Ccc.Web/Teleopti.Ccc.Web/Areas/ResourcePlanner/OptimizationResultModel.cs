using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.ResourcePlanner
{
	[Serializable]
	public class OptimizationResultModel
	{
		private readonly IList<OptimizationResultSkill> _skillResultList = new List<OptimizationResultSkill>();

		public IEnumerable<OptimizationResultSkill> SkillResultList
		{
			get { return _skillResultList; }
		}

		public void Map(IDictionary<ISkill, IList<ISkillDay>> skillDays)
		{
			foreach (var keyValuePair in skillDays)
			{
				var skill = keyValuePair.Key;
				var forecastSource = skill.SkillType.ForecastSource;
				if(forecastSource == ForecastSource.MaxSeatSkill || forecastSource == ForecastSource.OutboundTelephony)
					continue;

				var item = new OptimizationResultSkill {SkillName = skill.Name};
				_skillResultList.Add(item);
				foreach (var skillDay in keyValuePair.Value)
				{
					var detail = new OptimizationResultSkillDetail
					{
						Date = skillDay.CurrentDate,
						RelativeDifference =
							SkillStaffPeriodHelper.RelativeDifference(skillDay.SkillStaffPeriodCollection).GetValueOrDefault(-1)
					};
					detail.ColorId = mapColorId(detail.RelativeDifference, skill);
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