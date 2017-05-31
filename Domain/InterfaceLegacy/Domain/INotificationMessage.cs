using System.Collections.Generic;

namespace Teleopti.Ccc.Domain.InterfaceLegacy.Domain
{
	public interface INotificationMessage
	{
		string Subject { get; set; }
		IList<string> Messages { get; }
		string CustomerName { get; set; }

		string Data { get; set; }
	}
}