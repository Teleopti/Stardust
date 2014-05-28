using System;
using System.Configuration;
using System.Net;
using System.Windows.Forms;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Teleopti.Runtime.Environment
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            var client = new WebClient();
            var starturl = ConfigurationManager.AppSettings["url"];
            var response = client.DownloadString(starturl);
            var url = JObject.Parse(response)["Url"].Value<string>();
            Application.Run(new MainForm(url));
        }
    }
}
