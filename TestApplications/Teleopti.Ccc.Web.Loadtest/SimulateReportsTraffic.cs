using System;
using System.Diagnostics;
using System.Net.Http;
using Teleopti.Ccc.Web.TestApplicationsCommon;

namespace Teleopti.Ccc.Web.Loadtest
{
	public class SimulateMyTimeScheduleTraffic : TrafficSimulatorBase
	{
		public TimeSpan GoToMyTimeScheduleController()
		{
			var stopwatch = new Stopwatch();
			stopwatch.Start();
			var message = new HttpRequestMessage(HttpMethod.Get, "mytime#Schedule/Week");
			var response = HttpClient.SendAsync(message).Result;
			if (!response.IsSuccessStatusCode)
			{
				throw new Exception("Unable to go to MyTime Schedule");
			}
			var content = response.Content.ReadAsStringAsync().Result;
			if (!content.Contains("Authentication/SignOut"))
				throw new Exception("Unable to go to MyTime Schedule");
			stopwatch.Stop();
			return stopwatch.Elapsed;
		}
	}

	public class SimulateReportsTraffic : TrafficSimulatorBase
	{
		public TimeSpan GoToReportsController()
		{
			var stopwatch = new Stopwatch();
			stopwatch.Start();
			GetReportsPage(new Guid("C232D751-AEC5-4FD7-A274-7C56B99E8DEC"));
			stopwatch.Stop();
			return stopwatch.Elapsed;
		}

		private void GetReportsPage(Guid reportId)
		{
			var message = new HttpRequestMessage(HttpMethod.Get, string.Format("Reporting/Report/{0}#{0}", reportId));
			var response = HttpClient.SendAsync(message).Result;
			if (!response.IsSuccessStatusCode)
			{
				throw new Exception("Unable to go to Reports");
			}
			var content = response.Content.ReadAsStringAsync().Result;
			if (!content.Contains("<iframe"))
				throw new Exception("Unable to go to Reports");
		}	
	}
}