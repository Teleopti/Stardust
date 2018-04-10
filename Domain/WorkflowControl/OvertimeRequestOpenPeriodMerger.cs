using System.Collections.Generic;
using Teleopti.Ccc.Domain.ApplicationLayer.OvertimeRequests;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.WorkflowControl
{
	public class OvertimeRequestOpenPeriodMerger
	{
		public OvertimeRequestSkillTypeFlatOpenPeriod Merge(IEnumerable<OvertimeRequestSkillTypeFlatOpenPeriod> overtimeRequestOpenPeriods)
		{
			if (overtimeRequestOpenPeriods.IsNullOrEmpty())
				return null;

			var mergedOvertimeRequestOpenPeriod = new OvertimeRequestSkillTypeFlatOpenPeriod
			{
				AutoGrantType = OvertimeRequestAutoGrantType.Yes
			};

			foreach (var overtimeRequestOpenPeriod in overtimeRequestOpenPeriods)
			{
				if (prioritizedAutoGrantTypes.IndexOf(overtimeRequestOpenPeriod.AutoGrantType) <=
					prioritizedAutoGrantTypes.IndexOf(mergedOvertimeRequestOpenPeriod.AutoGrantType))
				{
					mergedOvertimeRequestOpenPeriod.AutoGrantType = overtimeRequestOpenPeriod.AutoGrantType;
					mergedOvertimeRequestOpenPeriod.DenyReason = overtimeRequestOpenPeriod.DenyReason;
					mergedOvertimeRequestOpenPeriod.EnableWorkRuleValidation = overtimeRequestOpenPeriod.EnableWorkRuleValidation;
					mergedOvertimeRequestOpenPeriod.WorkRuleValidationHandleType = overtimeRequestOpenPeriod.WorkRuleValidationHandleType;
					mergedOvertimeRequestOpenPeriod.SkillType = overtimeRequestOpenPeriod.SkillType;
					mergedOvertimeRequestOpenPeriod.OriginPeriod = overtimeRequestOpenPeriod.OriginPeriod;
				}
			}
			return mergedOvertimeRequestOpenPeriod;
		}

		private static IList<OvertimeRequestAutoGrantType> prioritizedAutoGrantTypes =>
			new List<OvertimeRequestAutoGrantType> { OvertimeRequestAutoGrantType.Deny, OvertimeRequestAutoGrantType.No, OvertimeRequestAutoGrantType.Yes };
	}
}