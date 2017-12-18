using Teleopti.Ccc.Domain.WorkflowControl;

namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Settings
{
	public class OvertimeRequestWorkRuleValidationHandleOptionView
	{
		public string Description { get; }

		public OvertimeValidationHandleType WorkRuleValidationHandleType { get; }

		public OvertimeRequestWorkRuleValidationHandleOptionView(OvertimeValidationHandleType workRuleValidationHandleType, string description)
		{
			WorkRuleValidationHandleType = workRuleValidationHandleType;
			Description = description;
		}
	}
}