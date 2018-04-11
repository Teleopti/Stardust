using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.OvertimeRequests
{
	public class SkillTypeFlatOvertimeOpenPeriodMapper
	{
		public IList<OvertimeRequestSkillTypeFlatOpenPeriod> Map(
			IEnumerable<IOvertimeRequestOpenPeriod> overtimeRequestOpenPeriods, ISkillType defaultSkillType)
		{
			var overtimeRequestSkillTypeFlatOpenPeriods = new List<OvertimeRequestSkillTypeFlatOpenPeriod>();
			foreach (var overtimeRequestOpenPeriod in overtimeRequestOpenPeriods)
			{
				if (overtimeRequestOpenPeriod.PeriodSkillTypes.Any())
					foreach (var overtimeRequestOpenPeriodSkillType in overtimeRequestOpenPeriod.PeriodSkillTypes)
					{
						overtimeRequestSkillTypeFlatOpenPeriods.Add(createSkillTypeFlatOpenPeriod(
							overtimeRequestOpenPeriodSkillType.SkillType,
							overtimeRequestOpenPeriod));
					}
				else
					overtimeRequestSkillTypeFlatOpenPeriods.Add(createSkillTypeFlatOpenPeriod(defaultSkillType,
						overtimeRequestOpenPeriod));
			}
			return overtimeRequestSkillTypeFlatOpenPeriods.OrderBy(x => x.OrderIndex).ToList();
		}

		private OvertimeRequestSkillTypeFlatOpenPeriod createSkillTypeFlatOpenPeriod(ISkillType skillType,
			IOvertimeRequestOpenPeriod overtimeRequestOpenPeriod)
		{
			return new OvertimeRequestSkillTypeFlatOpenPeriod
			{
				AutoGrantType = overtimeRequestOpenPeriod.AutoGrantType,
				OrderIndex = overtimeRequestOpenPeriod.OrderIndex,
				EnableWorkRuleValidation = overtimeRequestOpenPeriod.EnableWorkRuleValidation,
				WorkRuleValidationHandleType = overtimeRequestOpenPeriod.WorkRuleValidationHandleType,
				SkillType = skillType,
				OriginPeriod = overtimeRequestOpenPeriod,
			};
		}

	}
}