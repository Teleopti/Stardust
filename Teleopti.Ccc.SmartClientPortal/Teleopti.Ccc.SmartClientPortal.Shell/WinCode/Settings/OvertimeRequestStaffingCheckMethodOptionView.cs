using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Settings
{
	public class OvertimeRequestStaffingCheckMethodOptionView
	{
		public string Description { get; }

		public OvertimeRequestStaffingCheckMethod OvertimeRequestStaffingCheckMethod { get; }

		public OvertimeRequestStaffingCheckMethodOptionView(
			OvertimeRequestStaffingCheckMethod overtimeRequestStaffingCheckMethod, string description)
		{
			OvertimeRequestStaffingCheckMethod = overtimeRequestStaffingCheckMethod;
			Description = description;
		}
	}
}