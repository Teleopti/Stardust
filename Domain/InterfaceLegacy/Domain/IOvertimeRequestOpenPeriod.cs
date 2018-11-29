using System.Collections.Generic;
using Teleopti.Ccc.Domain.WorkflowControl;

namespace Teleopti.Ccc.Domain.InterfaceLegacy.Domain
{
	public interface IOvertimeRequestOpenPeriod : IAggregateEntity, ICloneableEntity<IOvertimeRequestOpenPeriod>
	{
		DateOnlyPeriod GetPeriod(DateOnly viewpointDateOnly);
		OvertimeRequestAutoGrantType AutoGrantType { get; set; }
		int OrderIndex { get; }
		string DenyReason { get; set; }
		bool EnableWorkRuleValidation { get; set; }

		OvertimeValidationHandleType? WorkRuleValidationHandleType { get; set; }

		IReadOnlyCollection<IOvertimeRequestOpenPeriodSkillType> PeriodSkillTypes { get; }

		void AddPeriodSkillType(IOvertimeRequestOpenPeriodSkillType periodSkillType);

		void ClearPeriodSkillType();
	}
} 