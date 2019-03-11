using System;
using System.Configuration;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Windows.Forms;
using EO.WebEngine;
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


			try
			{
				ServicePointManager.SecurityProtocol = (SecurityProtocolType) 3072;
			}
			catch (Exception)
			{
				//system doesn't support tls1.2
			}
			
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
		                "wJ7qxQXooW+mt8Ddr2qvprEh5Kvq7QAZvFupprHavUaBpLHLn3Xq7fgZ4K3s" +
		                "9vbp732v+AvxsbGn3dUE9K2i0d8O9oeo+87ou2jq7fgZ4K3s9vbpjEOzs/0U" +
		                "4p7l9/bpjEN14+30EO2s3MKetZ9Zl6TNF+ic3PIEEMidtbrD3bRtr7jK4LR1" +
		                "pvD6DuSn6unaD71GgaSxy5914+30EO2s3OnP566l4Of2GfKe3MKetZ9Zl6TN" +
		                "DOul5vvPuIlZl6Sxy59Zl8DyD+NZ6/0BELxbvNO/7uer5vH2zZ+v3PYEFO6n" +
		                "tKbC4q1pmaTA6YxDl6Sxy7to2PD9GvZ3hI6xy59Zs/MDD+SrwPL3Gp+d2Pj2" +
		                "6KFvprfA3a9qq6axHvSbvPwBFPE=");
	                EO.Base.Runtime.EnableEOWP = true;
	                EO.Base.Runtime.InitWorkerProcessExecutable(System.IO.Path.Combine(Application.StartupPath, "eowp.exe"));
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
