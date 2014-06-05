using System;
using System.Configuration;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;
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
	        client.Credentials = CredentialCache.DefaultNetworkCredentials;
            var starturl = ConfigurationManager.AppSettings["url"];
            try
            {
                var response = client.DownloadString(starturl);
                var result = JObject.Parse(response);
                var url = result["Url"].Value<string>();
                var signature = result["Signature"].Value<string>();

                var rsa = new RSACryptoServiceProvider();
                rsa.FromXmlString("<RSAKeyValue><Modulus>tcQWMgdpQeCd8+gzB3rYQAehHXF5mBGdyFMkJMEmcQmTlkpg22xLNz/kNYXZ7j2Cuhls+PBORzZkfBsNoL1vErT+N9Es4EEWOt6ntNe7wujqQqktUT/QOWEMJ8zJQM3bn7Oj9H5StBr7DWSRzgEjOc7knDcb4KCQL3ceXqmqwSonPfP1hp+bE8rZuxDISYiZVEkm417YzUHBk3ppV30Q9zvfL9IZX0q/ebCTRnLFockl7yOVucomvo8j4ssFPCAYgASoNvzWq+s5UTzYELl1I7F3hQnFwx0bIpQFmGbZ5BbNczc6rVYtCX5KDMsVaJSUcXBAnqGd20hq/ICkBR658w==</Modulus><Exponent>AQAB</Exponent></RSAKeyValue>");
                var isSuc = rsa.VerifyData(Encoding.UTF8.GetBytes(url), CryptoConfig.MapNameToOID("SHA1"), Convert.FromBase64String(signature));
                if (isSuc)
                {
                    EO.WebBrowser.Runtime.AddLicense(
                        "cqvm8fbNn6/c9gQU7qe0psPZr1uXs8+4iVmXpLHnrprj8AAivUaBpLHLn3Xm" +
                        "9vUQ8YLl6gDL45rr6c7NtWiqs8PbsG2ZpAcQ8azg8//ooWuZpMDpjEOXpLHL" +
                        "u6zg6/8M867p6c8j9ZDYtdLy1LDLs+DktYHGtPgUwZ/uwc7nrqzg6/8M867p" +
                        "6c+4iXWm8PoO5Kfq6c+4iXXj7fQQ7azcwp61n1mXpM0X6Jzc8gQQyJ21usPd" +
                        "tG2vuMrgtHWm8PoO5Kfq6doPvUaBpLHLn3Xj7fQQ7azc6c/nrqXg5/YZ8p7c" +
                        "wp61n1mXpM0M66Xm+8+4iVmXpLHLn1mXwPIP41nr/QEQvFu807/u5w==");
                    Application.Run(new MainForm(url));
                }
                else
                {
                    MessageBox.Show(@"Invalid Url in configuration.", @"Warning");
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message, @"Error");
            }
           
        }
    }
}
