using System.Configuration;
using System.Globalization;

namespace Teleopti.Analytics.Portal.Utils
{
    public static class HelpLinkBuilder
    {
        public static string GetPerformanceManagerHelpLink()
        {
			return string.Format(CultureInfo.InvariantCulture, "{0}/Performance_Manager_Module", ConfigurationManager.AppSettings["HelpUrl"]);
        }
    }
}