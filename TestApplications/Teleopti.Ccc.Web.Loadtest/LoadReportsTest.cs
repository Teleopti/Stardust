using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Teleopti.Ccc.Web.TestApplicationsCommon;

namespace Teleopti.Ccc.Web.Loadtest
{
	public class LoadReportsTest
	{
		private readonly string _baseUrl;
		private readonly string _businessUnitName;
		private static readonly object Lock = new object();

		public LoadReportsTest(string baseUrl, string businessUnitName)
		{
			_baseUrl = baseUrl;
			_businessUnitName = businessUnitName;
		}

		public async Task<List<TimingData>> RunAsync(List<UserData> users, int maxDownloads)
		{
			var concurrentQueue = new ConcurrentQueue<TimingData>();

			using (var semaphore = new SemaphoreSlim(maxDownloads))
			{
				var tasks = users.Select(user => Task.Factory.StartNew(() =>
				{
					semaphore.Wait();
					try
					{
						var timingData = new TimingData();
						using (var trafficSimulator = new SimulateReportsTraffic())
						{
							trafficSimulator.Start(_baseUrl, _businessUnitName, user.Username, user.Password);
							var loginTaken = trafficSimulator.LogOn();
							var timeTaken = trafficSimulator.GoToReportsController();
							lock (Lock)
							{
								timingData.Logon += loginTaken.TotalMilliseconds;
								timingData.Reports += timeTaken.TotalMilliseconds;
							}
						}
						concurrentQueue.Enqueue(timingData);
					}
					finally
					{
						semaphore.Release();
					}
				}, TaskCreationOptions.LongRunning));


				await Task.WhenAll(tasks.ToArray());
			}
			return concurrentQueue.ToList();
		}
	}

	public class TimingData
	{
		public double Logon { get; set; }
		public double Reports { get; set; }
	}
}