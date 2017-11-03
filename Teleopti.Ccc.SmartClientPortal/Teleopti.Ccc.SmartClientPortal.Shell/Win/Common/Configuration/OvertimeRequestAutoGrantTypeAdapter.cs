using Teleopti.Ccc.Domain.WorkflowControl;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common;

namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.Common.Configuration
{
	public class OvertimeRequestAutoGrantTypeAdapter
	{
		public string DisplayName { get; set; }

		public OvertimeRequestAutoGrantType AutoGrantType { get; set; }

		public OvertimeRequestAutoGrantTypeAdapter(OvertimeRequestAutoGrantType autoGrantType)
		{
			DisplayName = UserTexts.Resources.ResourceManager.GetString(autoGrantType.ToString());
			AutoGrantType = autoGrantType;
		}
	}
}
