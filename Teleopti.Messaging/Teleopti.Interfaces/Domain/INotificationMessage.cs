using System.Collections.Generic;

namespace Teleopti.Interfaces.Domain
{
	public interface INotificationMessage
	{
		string Subject { get; set; }
		IList<string> Messages { get; }
	}
}