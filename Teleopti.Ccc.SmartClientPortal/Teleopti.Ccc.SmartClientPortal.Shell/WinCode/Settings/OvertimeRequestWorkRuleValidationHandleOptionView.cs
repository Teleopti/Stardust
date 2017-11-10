using Teleopti.Ccc.Domain.WorkflowControl;

namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Settings
{
	public class OvertimeRequestWorkRuleValidationHandleOptionView
	{
		public string Description { get; }

		public OvertimeWorkRuleValidationHandleType WorkRuleValidationHandleType { get; }

		public OvertimeRequestWorkRuleValidationHandleOptionView(OvertimeWorkRuleValidationHandleType workRuleValidationHandleType, string description)
		{
			WorkRuleValidationHandleType = workRuleValidationHandleType;
			Description = description;
		}
	}
}