using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Win.Budgeting
{
	public class ClipboardStatusEventModel
	{
		public ClipboardAction ClipboardAction { get; set; }

		public bool Enabled { get; set; }
	}
}