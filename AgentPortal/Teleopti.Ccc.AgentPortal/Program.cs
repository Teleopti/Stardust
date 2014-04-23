using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Teleopti.Ccc.AgentPortal.Helper;
using Teleopti.Ccc.AgentPortal.Main;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;

namespace Teleopti.Ccc.AgentPortal
{
    internal static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters", MessageId = "Teleopti.Ccc.AgentPortal.Helper.MessageBoxHelper.ShowErrorMessage(System.String,System.String)"), STAThread]
        private static void Main(string[] args)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            var settings = AppConfigHelper.LoadSettings();
            var logOnDetails = new LogOnDetails();
            try
            {
                var token = string.Empty;
                if (args.Length == 1)
                {
                    token = args[0];
                }
            	var activationArguments = AppDomain.CurrentDomain.SetupInformation.ActivationArguments;
                if (string.IsNullOrEmpty(token) &&
					activationArguments !=null &&
                    activationArguments.ActivationData != null &&
                    activationArguments.ActivationData.Length > 0)
                {
					if (activationArguments.ActivationData[0].IndexOf('?')!=-1)
						token = activationArguments.ActivationData[0];
                }
            	
                if (!string.IsNullOrEmpty(token))
                {
                    if (token.IndexOf('?') != -1)
                        token = token.Substring(token.IndexOf('?') + 1);
                    var clearToken = Encryption.DecryptStringFromBytes(Convert.FromBase64String(token),
                                                                       Convert.FromBase64String(settings["SharedKey"]),
                                                                       Convert.FromBase64String(settings["SharedIV"]));
                    var clearTokenProperties = clearToken.Split(',');
                    var tokenDictionary = new Dictionary<string, string>();
                    foreach (var clearTokenProperty in clearTokenProperties)
                    {
                        tokenDictionary.Add(clearTokenProperty.Substring(0, clearTokenProperty.IndexOf('=')),
                                            clearTokenProperty.Substring(clearTokenProperty.IndexOf('=') + 1));
                    }

                    logOnDetails.UserName = tokenDictionary["usr"];
                    logOnDetails.Password = tokenDictionary["pwd"];
                    logOnDetails.DataSource = new DataSourceDto
                                                  {
                                                      Name = tokenDictionary["datasource"],
                                                      AuthenticationTypeOptionDto =
                                                          AuthenticationTypeOptionDto.Application
                                                  };

                    var timestamp = tokenDictionary["time"];
                    double time;
                    if (!double.TryParse(timestamp, out time)) return;

                    var tokenTimestamp = TimeSpan.FromMilliseconds(time);
                    if (
                        DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).Subtract(
                            tokenTimestamp).TotalSeconds > 30)
                    {
                        logOnDetails = new LogOnDetails();
                    }
                }
            }
            catch (Exception e)
            {
                MessageBoxHelper.ShowErrorMessage(e.Message, string.Concat(UserTexts.Resources.LogOn, " - ", UserTexts.Resources.IllegalInput));
            }
            var logOnScreen = new LogOnScreen(logOnDetails,settings);
            if (logOnScreen.ShowDialog() == DialogResult.OK)
            {
                Application.Run(new MainScreen());
            }
        }
    }
}