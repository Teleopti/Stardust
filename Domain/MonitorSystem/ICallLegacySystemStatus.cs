using System.Net;

namespace Teleopti.Ccc.Domain.MonitorSystem
{
	public interface ICallLegacySystemStatus
	{
		HttpStatusCode Execute();
	}
}