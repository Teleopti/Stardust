﻿using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.WorkflowControl;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.OvertimeRequests
{
	public class OvertimeRequestSkillTypeFlatOpenPeriod
	{
		public OvertimeRequestAutoGrantType AutoGrantType { get; set; }
		public int OrderIndex { get; set; }
		public bool EnableWorkRuleValidation { get; set; }
		public OvertimeValidationHandleType? WorkRuleValidationHandleType { get; set; }
		public ISkillType SkillType { get; set; }
		public string DenyReason { get; set; }
		public DateOnlyPeriod Period { get; set; }
		public IOvertimeRequestOpenPeriod OriginPeriod { get; set; }
	}
}