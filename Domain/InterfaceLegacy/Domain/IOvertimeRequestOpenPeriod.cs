using Teleopti.Ccc.Domain.WorkflowControl;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.InterfaceLegacy.Domain
{
	public interface IOvertimeRequestOpenPeriod : IAggregateEntity, ICloneableEntity<IOvertimeRequestOpenPeriod>
	{
		DateOnlyPeriod GetPeriod(DateOnly viewpointDateOnly);
		OvertimeRequestAutoGrantType AutoGrantType { get; set; }
		int OrderIndex { get; }
		string DenyReason { get; set; }
		bool EnableWorkRuleValidation { get; set; }

		OvertimeWorkRuleValidationHandleType WorkRuleValidationHandleType { get; set; }
	}
}