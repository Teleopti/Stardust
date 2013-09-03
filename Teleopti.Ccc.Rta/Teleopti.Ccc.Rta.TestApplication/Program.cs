using System;
using System.Collections;
using System.Configuration;
using System.Data.SqlTypes;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using log4net;
using log4net.Config;
using Teleopti.Ccc.Rta.LogClient;

namespace Teleopti.Ccc.Rta.TestApplication
{
    internal class Program
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof (Program));

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization",
            "CA1303:Do not pass literals as localized parameters",
            MessageId = "System.Console.WriteLine(System.String,System.Object)"),
         System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization",
             "CA1303:Do not pass literals as localized parameters",
             MessageId = "System.Console.WriteLine(System.String)")]
        private static void Main(string[] args)
        {
            XmlConfigurator.Configure();
            ServicePointManager.ServerCertificateValidationCallback = IgnoreInvalidCertificate;
            var serviceUrl = ConfigurationManager.AppSettings["serviceUrl"];

            var connectionProperties = new Hashtable();
            if (!string.IsNullOrEmpty(serviceUrl)) connectionProperties.Add("serviceUrl", serviceUrl);

            var keepRunning = true;
            while (keepRunning)
            {
                var sendSettings = new SendSettings();
                var sendCount = sendSettings.RemainingCount;
                var platformTypeId = sendSettings.PlatformId;
                var clientHandler = new ClientHandler(connectionProperties);
                clientHandler.StartLogClient();
	            var stopwatch = new System.Diagnostics.Stopwatch();
				stopwatch.Start();
                while (sendSettings.RemainingCount > 0)
                {
                    var statesForTest = sendSettings.Read();
                    foreach (var stateForTest in statesForTest)
                    {
                        try
                        {
                            clientHandler.SendRtaDataToServer(stateForTest.LogOn, stateForTest.StateCode, TimeSpan.Zero,
                                                              DateTime.UtcNow, platformTypeId, stateForTest.DataSourceId,
                                                              stateForTest.BatchIdentifier,
                                                              stateForTest.IsSnapshot);

							if (sendSettings.IntervalForScheduleUpdate != 0 && sendSettings.RemainingCount % sendSettings.IntervalForScheduleUpdate == 0)
								clientHandler.UpdateScheduleChange(stateForTest.PersonId, stateForTest.BusinessUnitId.ToString(), DateTime.UtcNow);
                        }
                        catch (Exception exception)
                        {
                            Logger.Error("An error occured while sending RTA data to server", exception);
                            break;
                        }
                        Thread.Sleep(stateForTest.WaitTime);
                    }
                }
                //Run the end of sequence (making sure LogOff signal is sent)
                foreach (var stateForTest in sendSettings.EndSequence())
                {
                    try
                    {
                        clientHandler.SendRtaDataToServer(stateForTest.LogOn, stateForTest.StateCode, TimeSpan.Zero,
                                                          DateTime.UtcNow, platformTypeId, stateForTest.DataSourceId,
                                                          SqlDateTime.MinValue.Value,
                                                          false);
                    }
                    catch (Exception exception)
                    {
                        Logger.Error("An error occured while sending RTA data to server", exception);
                        break;
                    }
                }
                clientHandler.StopLogClient();
				stopwatch.Stop();
                Console.WriteLine("Done with sending {0} rows of RTA data. Elapsed time equals: {1}", sendCount, stopwatch.Elapsed);
                Console.WriteLine("Press Y to send again or any other key to exit.");
                var info = Console.ReadKey();
                keepRunning = (info.Key == ConsoleKey.Y);
            }
        }

        private static bool IgnoreInvalidCertificate(object sender, X509Certificate certificate, X509Chain chain,
                                                     SslPolicyErrors sslpolicyerrors)
        {
            return true;
        }
    }
}