using System.Net;

namespace Teleopti.Ccc.Domain.MonitorSystem
{
	public class CheckLegacySystemStatus
	{
		private readonly ICallLegacySystemStatus _callLegacySystemStatus;

		public CheckLegacySystemStatus(ICallLegacySystemStatus callLegacySystemStatus)
		{
			_callLegacySystemStatus = callLegacySystemStatus;
		}
		
		public HttpStatusCode Execute()
		{
			return _callLegacySystemStatus.Execute();
		}
	}
}