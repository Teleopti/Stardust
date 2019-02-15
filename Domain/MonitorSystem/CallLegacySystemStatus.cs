using System.Configuration;
using System.Net.Http;

namespace Teleopti.Ccc.Domain.MonitorSystem
{
	public class CallLegacySystemStatus : ICallLegacySystemStatus
	{
		private static readonly HttpClient client = new HttpClient();
		private static readonly string url = ConfigurationManager.AppSettings["SystemStatus"];
		
		public bool Execute()
		{
			return client.GetAsync(url).Result.IsSuccessStatusCode;
		}
	}
}