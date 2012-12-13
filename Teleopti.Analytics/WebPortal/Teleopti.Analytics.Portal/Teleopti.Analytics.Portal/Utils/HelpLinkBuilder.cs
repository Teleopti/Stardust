using System.Configuration;
using System.Threading;
using System.Globalization;

namespace Teleopti.Analytics.Portal.Utils
{
    public static class HelpLinkBuilder
    {
        public static string GetStandardReportHelpLink(string helpKey)
        {
			return string.Format(CultureInfo.InvariantCulture, "{0}/{1}", ConfigurationManager.AppSettings["HelpUrl"], helpKey);
        }

        public static string GetPerformanceManagerHelpLink()
        {
			return string.Format(CultureInfo.InvariantCulture, "{0}/Performance_Manager_Module", ConfigurationManager.AppSettings["HelpUrl"]);
        }
    }
}