using Teleopti.Analytics.Portal.AnalyzerProxy;
using Teleopti.Analytics.Portal.AnalyzerProxy.AnalyzerRef;
using Teleopti.Analytics.Portal.Utils;

namespace Teleopti.Analytics.Portal.PerformanceManager
{
	public static class ReportInstanceHandler
	{
		public static void Add(ReportInstance reportInstance)
		{
			if (!StateHolder.ReportInstanceCache.ContainsKey(reportInstance.Id))
			{
				// Add report instance to session cache
				StateHolder.ReportInstanceCache.Add(reportInstance.Id, reportInstance);
			}
		}

		public static void Remove(string reportInstanceId)
		{
			if (StateHolder.ReportInstanceCache.ContainsKey(reportInstanceId))
			{
				var reportInstance = (ReportInstance)StateHolder.ReportInstanceCache[reportInstanceId];

				// Close Analyzer report to release resources from web server
				CloseReport(reportInstance);

				// Remove report instance from session cache
				StateHolder.ReportInstanceCache.Remove(reportInstanceId);
			}
		}

		private static void CloseReport(ReportInstance reportInstance)
		{
			var olapInformation = new OlapInformation();

			using (var clientProxy = new ClientProxy(olapInformation.OlapServer, olapInformation.OlapDatabase))
			{
				clientProxy.CloseReport(reportInstance, false);
			}
		}
	}
}
