using System.Collections.Generic;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.WorkflowControl
{
	public class OvertimeRequestOpenPeriodMerger
	{
		public IOvertimeRequestOpenPeriod Merge(IEnumerable<IOvertimeRequestOpenPeriod> overtimeRequestOpenPeriods)
		{
			if (overtimeRequestOpenPeriods.IsNullOrEmpty())
				return null;

			var mergedPeriod = new OvertimeRequestOpenDatePeriod
			{
				AutoGrantType = OvertimeRequestAutoGrantType.Yes
			};

			foreach (var overtimeRequestOpenPeriod in overtimeRequestOpenPeriods)
			{
				if (prioritizedAutoGrantTypes.IndexOf(overtimeRequestOpenPeriod.AutoGrantType) <=
					prioritizedAutoGrantTypes.IndexOf(mergedPeriod.AutoGrantType))
				{
					mergedPeriod.AutoGrantType = overtimeRequestOpenPeriod.AutoGrantType;
					mergedPeriod.DenyReason = overtimeRequestOpenPeriod.DenyReason;
					mergedPeriod.EnableWorkRuleValidation = overtimeRequestOpenPeriod.EnableWorkRuleValidation;
					mergedPeriod.WorkRuleValidationHandleType = overtimeRequestOpenPeriod.WorkRuleValidationHandleType;
					mergedPeriod.SkillType = overtimeRequestOpenPeriod.SkillType;
				}
			}
			return mergedPeriod;
		}

		private static IList<OvertimeRequestAutoGrantType> prioritizedAutoGrantTypes =>
			new List<OvertimeRequestAutoGrantType> { OvertimeRequestAutoGrantType.Deny, OvertimeRequestAutoGrantType.No, OvertimeRequestAutoGrantType.Yes };
	}
}