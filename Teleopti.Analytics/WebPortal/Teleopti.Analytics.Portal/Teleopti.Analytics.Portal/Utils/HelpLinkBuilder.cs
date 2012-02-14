using System.Configuration;
using System.Threading;

namespace Teleopti.Analytics.Portal.Utils
{
    public static class HelpLinkBuilder
    {
        public static string GetStandardReportHelpLink(string helpKey)
        {
            return string.Format("{0}/{1}/{2}", ConfigurationManager.AppSettings["HelpUrl"], getLanguage(), helpKey);
        }

        public static string GetPerformanceManagerHelpLink()
        {
            return string.Format("{0}/{1}/Performance_Manager.html", ConfigurationManager.AppSettings["HelpUrl"], getLanguage());
        }

        private static string getLanguage()
        {
            //Get culture, two letter Style
            string helpLang = string.Empty;
            string helpCulture = Thread.CurrentThread.CurrentUICulture.Name;

            if (!string.IsNullOrEmpty(helpCulture)) helpLang = helpCulture.Substring(0, 2);

            switch (helpLang)
            {
                case "sv":
                    return helpLang;
                case "de":
                    return helpLang;
                /* 
                case "ru":
                    _helpLang = "ru";
                        break;
                case "zh":
                    _helpLang = "zh";
                    break;                  
                */
                default:
                    return "en";
            }
        }
    }
}