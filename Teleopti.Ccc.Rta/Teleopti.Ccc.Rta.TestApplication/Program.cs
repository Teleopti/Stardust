using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlTypes;
using System.Globalization;
using System.Threading;
using log4net;
using log4net.Config;
using Teleopti.Ccc.Rta.LogClient;

namespace Teleopti.Ccc.Rta.TestApplication
{
    class Program
    {
        private static ILog _logger = LogManager.GetLogger(typeof (Program));

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters", MessageId = "System.Console.WriteLine(System.String,System.Object)"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters", MessageId = "System.Console.WriteLine(System.String)")]
        static void Main(string[] args)
        {
            XmlConfigurator.Configure();

            string serviceUrl = ConfigurationManager.AppSettings["serviceUrl"];

            var connectionProperties = new Hashtable();
            if (!string.IsNullOrEmpty(serviceUrl)) connectionProperties.Add("serviceUrl", serviceUrl);

            bool keepRunning = true;

            while (keepRunning)
            {
                var sendSettings = new SendSettings();
                int sendCount = sendSettings.RemainingCount;

                var clientHandler = new ClientHandler(connectionProperties);
                clientHandler.StartLogClient();
                while (sendSettings.RemainingCount > 0)
                {
                    IEnumerable<AgentStateForTest> statesForTest = sendSettings.Read();
                    foreach (AgentStateForTest stateForTest in statesForTest)
                    {
                        try
                        {
                            clientHandler.SendRtaDataToServer(stateForTest.LogOn, stateForTest.StateCode, TimeSpan.Zero,
                                                              DateTime.UtcNow, Guid.Empty, stateForTest.DataSourceId, stateForTest.BatchIdentifier,
                                                              stateForTest.IsSnapshot);
                        }
                        catch (Exception exception)
                        {
                            _logger.Error("An error occured while sending RTA data to server", exception);
                            break;
                        }
                        Thread.Sleep(stateForTest.WaitTime);
                    }
                }
                //Run the end of sequence (making sure LogOff signal is sent)
                foreach (AgentStateForTest stateForTest in sendSettings.EndSequence())
                {
                    try
                    {
                        clientHandler.SendRtaDataToServer(stateForTest.LogOn, stateForTest.StateCode, TimeSpan.Zero,
                                                          DateTime.UtcNow, Guid.Empty, 1, SqlDateTime.MinValue.Value,
                                                          false);
                    }
                    catch (Exception exception)
                    {
                        _logger.Error("An error occured while sending RTA data to server",exception);
                        break;
                    }
                }
                clientHandler.StopLogClient();

                Console.WriteLine("Done with sending {0} rows of RTA data.", sendCount);
                Console.WriteLine("Press Y to send again or any other key to exit.");
                ConsoleKeyInfo info = Console.ReadKey();
                keepRunning = (info.Key == ConsoleKey.Y);
            }
        }
    }
}