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
                    Application.Run(new MainForm(url));
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
