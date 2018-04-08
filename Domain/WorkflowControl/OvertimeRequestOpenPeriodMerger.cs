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

			IOvertimeRequestOpenPeriod mergedOvertimeRequestOpenPeriod = new OvertimeRequestOpenDatePeriod
			{
				AutoGrantType = OvertimeRequestAutoGrantType.Yes
			};

			foreach (var overtimeRequestOpenPeriod in overtimeRequestOpenPeriods)
			{
				if (prioritizedAutoGrantTypes.IndexOf(overtimeRequestOpenPeriod.AutoGrantType) <=
					prioritizedAutoGrantTypes.IndexOf(mergedOvertimeRequestOpenPeriod.AutoGrantType))
				{
					if (overtimeRequestOpenPeriod is OvertimeRequestOpenDatePeriod period)
					{
						mergedOvertimeRequestOpenPeriod = new OvertimeRequestOpenDatePeriod
						{
							Period = period.Period
						};
					}
					else
					{
						mergedOvertimeRequestOpenPeriod = new OvertimeRequestOpenRollingPeriod
						{
							BetweenDays = ((OvertimeRequestOpenRollingPeriod)overtimeRequestOpenPeriod).BetweenDays
						};
					}
					mergedOvertimeRequestOpenPeriod.AutoGrantType = overtimeRequestOpenPeriod.AutoGrantType;
					mergedOvertimeRequestOpenPeriod.DenyReason = overtimeRequestOpenPeriod.DenyReason;
					mergedOvertimeRequestOpenPeriod.EnableWorkRuleValidation = overtimeRequestOpenPeriod.EnableWorkRuleValidation;
					mergedOvertimeRequestOpenPeriod.WorkRuleValidationHandleType = overtimeRequestOpenPeriod.WorkRuleValidationHandleType;
					mergedOvertimeRequestOpenPeriod.SkillType = overtimeRequestOpenPeriod.SkillType;
				}
			}
			return mergedOvertimeRequestOpenPeriod;
		}

		private static IList<OvertimeRequestAutoGrantType> prioritizedAutoGrantTypes =>
			new List<OvertimeRequestAutoGrantType> { OvertimeRequestAutoGrantType.Deny, OvertimeRequestAutoGrantType.No, OvertimeRequestAutoGrantType.Yes };
	}
}