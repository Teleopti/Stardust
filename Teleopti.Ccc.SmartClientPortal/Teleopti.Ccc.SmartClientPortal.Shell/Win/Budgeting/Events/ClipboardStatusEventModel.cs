using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.Budgeting.Events
{
	public class ClipboardStatusEventModel
	{
		public ClipboardAction ClipboardAction { get; set; }

		public bool Enabled { get; set; }
	}
}