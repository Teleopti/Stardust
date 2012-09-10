using System.Collections.Generic;

namespace Teleopti.Ccc.Sdk.Common.Contracts
{
	public interface INotificationMessage
	{
		string Subject { get; set; }
		IList<string> Messages { get; }
	}
}