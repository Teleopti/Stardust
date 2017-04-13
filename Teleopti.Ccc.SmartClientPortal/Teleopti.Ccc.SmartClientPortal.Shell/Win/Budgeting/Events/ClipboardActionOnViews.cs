using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Win.Common;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Win.Budgeting.Events
{
	public class ClipboardActionOnViews
	{
		public BaseUserControl ViewType { get; set; }

		public ClipboardAction ClipboardAction { get; set; }
	}
}