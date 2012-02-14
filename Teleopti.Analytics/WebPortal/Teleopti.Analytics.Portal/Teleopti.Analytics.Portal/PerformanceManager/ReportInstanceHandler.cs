using System.Collections;
using System.Web;
using Teleopti.Analytics.Portal.AnalyzerProxy;
using Teleopti.Analytics.Portal.AnalyzerProxy.AnalyzerRef;

namespace Teleopti.Analytics.Portal.PerformanceManager
{
    public static class ReportInstanceHandler
    {
        public static void Add(ReportInstance reportInstance)
        {
            if (!InstanceCache.ContainsKey(reportInstance.Id))
            {
                // Add report instance to session cache
                InstanceCache.Add(reportInstance.Id, reportInstance);
            }
        }

        public static void Remove(string reportInstanceId)
        {
            if (InstanceCache.ContainsKey(reportInstanceId))
            {
                var reportInstance = (ReportInstance)InstanceCache[reportInstanceId];
                
                // Close Analyzer report to release resources from web server
                CloseReport(reportInstance);
                
                // Remove report instance from session cache
                InstanceCache.Remove(reportInstanceId);
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

        private static Hashtable InstanceCache
        {
            get
            {
                var ht = (Hashtable)HttpContext.Current.Session["InstanceCache"];
                if (ht == null)
                {
                    ht = new Hashtable();
                    HttpContext.Current.Session["InstanceCache"] = ht;
                }
                return ht;
            }
            set
            {
                HttpContext.Current.Session["InstanceCache"] = value;
            }
        }
    }
}
