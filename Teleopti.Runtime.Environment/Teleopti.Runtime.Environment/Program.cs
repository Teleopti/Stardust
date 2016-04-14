using System;
using System.Configuration;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Windows.Forms;
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
	            rsa.ImportParameters(new RSAParameters
	            {
		            Modulus =
			            Convert.FromBase64String(
				            "tcQWMgdpQeCd8+gzB3rYQAehHXF5mBGdyFMkJMEmcQmTlkpg22xLNz/kNYXZ7j2Cuhls+PBORzZkfBsNoL1vErT+N9Es4EEWOt6ntNe7wujqQqktUT/QOWEMJ8zJQM3bn7Oj9H5StBr7DWSRzgEjOc7knDcb4KCQL3ceXqmqwSonPfP1hp+bE8rZuxDISYiZVEkm417YzUHBk3ppV30Q9zvfL9IZX0q/ebCTRnLFockl7yOVucomvo8j4ssFPCAYgASoNvzWq+s5UTzYELl1I7F3hQnFwx0bIpQFmGbZ5BbNczc6rVYtCX5KDMsVaJSUcXBAnqGd20hq/ICkBR658w=="),
		            Exponent = Convert.FromBase64String("AQAB")
	            });
				
                var isSuc = rsa.VerifyData(Encoding.UTF8.GetBytes(url), CryptoConfig.MapNameToOID("SHA1"), Convert.FromBase64String(signature));
                if (isSuc)
                {
					EO.WebBrowser.Runtime.AddLicense(
	"ewQU7qe0psLhrWmZpMDpjEOXpLHLu2jY8P0a9neEjrHLn1mz8wMP5KvA8vca" +
	"n53Y+PbooW+mt8Ddr2qrprEe9Ju8/AEU8Z7qxQXooW+mt8Ddr2qtprEh5Kvq" +
	"7QAZvFupprHavUaBpLHLn3Xq7fgZ4K3s9vbpz5DF/Qnas4LR0dP36WzPyNQQ" +
	"127o+87ou2jq7fgZ4K3s9vbpjEOzs/0U4p7l9/bpjEN14+30EO2s3MKetZ9Z" +
	"l6TNF+ic3PIEEMidtbrD3bRtr7jK4LR1pvD6DuSn6unaD71GgaSxy5914+30" +
	"EO2s3OnP566l4Of2GfKe3MKetZ9Zl6TNDOul5vvPuIlZl6Sxy59Zl8DyD+NZ" +
	"6/0BELxbvNO/7uer5vH2zZ+v3PY=");
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
