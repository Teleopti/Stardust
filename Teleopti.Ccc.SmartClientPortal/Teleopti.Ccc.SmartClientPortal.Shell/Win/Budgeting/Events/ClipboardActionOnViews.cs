using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.Common;

namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.Budgeting.Events
{
	public class ClipboardActionOnViews
	{
		public BaseUserControl ViewType { get; set; }

		public ClipboardAction ClipboardAction { get; set; }
	}
}