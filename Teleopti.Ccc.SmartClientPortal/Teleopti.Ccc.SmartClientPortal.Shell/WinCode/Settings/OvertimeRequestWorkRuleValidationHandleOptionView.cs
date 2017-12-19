using Teleopti.Ccc.Domain.WorkflowControl;

namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Settings
{
	public class OvertimeRequestValidationHandleOptionView
	{
		public string Description { get; }

		public OvertimeValidationHandleType WorkRuleValidationHandleType { get; }

		public OvertimeRequestValidationHandleOptionView(OvertimeValidationHandleType workRuleValidationHandleType, string description)
		{
			WorkRuleValidationHandleType = workRuleValidationHandleType;
			Description = description;
		}
	}
}