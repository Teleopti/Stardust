using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using log4net;
using log4net.Config;

namespace Teleopti.Ccc.Rta.TestApplication
{
    internal class Program
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof (Program));

        private static void Main(string[] args)
        {
            XmlConfigurator.Configure();
			ServicePointManager.ServerCertificateValidationCallback = ignoreInvalidCertificate;
            var serviceUrl = ConfigurationManager.AppSettings["serviceUrl"];

            var connectionProperties = new Hashtable();
            if (!string.IsNullOrEmpty(serviceUrl)) connectionProperties.Add("serviceUrl", serviceUrl);

            var keepRunning = true;
            while (keepRunning)
            {
                var sendSettings = new SendSettings();
				var originalSendCount = sendSettings.RemainingCount;
                var clientHandler = new ClientHandler(connectionProperties);
                clientHandler.StartLogClient();
	            var stopwatch = new System.Diagnostics.Stopwatch();
				stopwatch.Start();

				if (sendSettings.UseMultiThread)
					useMultiThread(sendSettings, clientHandler);
				else
					useSingleThread(sendSettings, clientHandler);

				clientHandler.StopLogClient();
				stopwatch.Stop();
				Console.WriteLine("Done with sending {0} rows of RTA data. Elapsed time equals: {1}", originalSendCount, stopwatch.Elapsed);
				Console.WriteLine("Press Y to send again or any other key to exit.");
				var info = Console.ReadKey();
				keepRunning = (info.Key == ConsoleKey.Y);
			}
		}

		private static void useMultiThread(SendSettings sendSettings, ClientHandler clientHandler)
		{
			var numberOfThreads = sendSettings.NumberOfThreads;
			var fullStateList = getThreadStateLists(numberOfThreads, sendSettings);
			var threadStateList = new List<Thread>();

			for (var i = 0; i < numberOfThreads; i++)
			{
				var threadSendList = fullStateList[i];
				var t = new Thread(() => sendStates(sendSettings, clientHandler, threadSendList));
				t.Start();
				threadStateList.Add(t);
			}

			threadStateList.ForEach(t => t.Join());
		}

		private static List<List<AgentStateForTest>> getThreadStateLists(int numberOfThreads, SendSettings settings)
		{
			var superList = new List<List<AgentStateForTest>>();
			for (var i = 0; i < numberOfThreads; i++)
			{
				var tempThreadList = new List<AgentStateForTest>();
				for (var j = 0; j < settings.RemainingCount / numberOfThreads; j++)
					tempThreadList.AddRange(settings.Read());
				superList.Add(tempThreadList);
			}
			return superList;
		}

	    private static void useSingleThread(SendSettings sendSettings, ClientHandler clientHandler)
	    {
		    while (sendSettings.RemainingCount > 0)
			    sendStates(sendSettings, clientHandler, sendSettings.Read());

		    sendStates(sendSettings, clientHandler, sendSettings.EndSequence());
	    }

	    private static void sendStates(SendSettings sendSettings, ClientHandler clientHandler, IEnumerable<AgentStateForTest> statesForTest)
                {
                    foreach (var stateForTest in statesForTest)
                    {
                        try
                        {
                            clientHandler.SendRtaDataToServer(stateForTest.AuthenticationKey, stateForTest.LogOn, stateForTest.StateCode, TimeSpan.Zero,
					                                  DateTime.UtcNow, sendSettings.PlatformId, stateForTest.DataSourceId,
                                                              stateForTest.BatchIdentifier,
                                                              stateForTest.IsSnapshot);

					if (sendSettings.IntervalForScheduleUpdate != 0 &&
					    sendSettings.RemainingCount%sendSettings.IntervalForScheduleUpdate == 0)
						clientHandler.UpdateScheduleChange(stateForTest.PersonId.ToString(),
						                                   stateForTest.BusinessUnitId.ToString(), DateTime.UtcNow);
                        }
                        catch (Exception exception)
                        {
                            Logger.Error("An error occured while sending RTA data to server", exception);
                            break;
                        }
	                    Thread.Sleep(stateForTest.WaitTime);
                    }
                }

		private static bool ignoreInvalidCertificate(object sender, X509Certificate certificate, X509Chain chain,
                                                     SslPolicyErrors sslpolicyerrors)
        {
            return true;
        }
    }
}