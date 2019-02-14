using System.Configuration;
using System.Net;
using System.Net.Http;

namespace Teleopti.Ccc.Domain.MonitorSystem
{
	public class CallLegacySystemStatus : ICallLegacySystemStatus
	{
		private static readonly HttpClient client = new HttpClient();
		private static readonly string url = ConfigurationManager.AppSettings["SystemStatus"];
		
		public HttpStatusCode Execute()
		{
			return client.GetAsync(url).Result.StatusCode;
		}
	}
}