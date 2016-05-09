using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Teleopti.Ccc.Web.TestApplicationsCommon;

namespace Teleopti.Ccc.Web.Loadtest
{
	public class LoadMyTimeScheduleTest : LoadAbstractTest<MyTimeScheduleTimingData>
	{
		public LoadMyTimeScheduleTest(string baseUrl, string businessUnitName) : base(baseUrl, businessUnitName)
		{
		}

		protected override MyTimeScheduleTimingData RunTest(UserData user)
		{
			var timingData = new MyTimeScheduleTimingData();
			using (var trafficSimulator = new SimulateMyTimeScheduleTraffic())
			{
				try
				{
					trafficSimulator.Start(BaseUrl, BusinessUnitName, user.Username, user.Password);
					var loginTaken = trafficSimulator.LogOn();
					var timeTaken = trafficSimulator.GoToMyTimeScheduleController();
					lock (Lock)
					{
						timingData.Logon += loginTaken.TotalMilliseconds;
						timingData.Schedule += timeTaken.TotalMilliseconds;
						Console.Write(".");
					}
				}
				catch (Exception ex)
				{
					timingData.Errors++;
				}
				
			}
			return timingData;
		}
	}



	public abstract class LoadAbstractTest<T>
	{
		protected readonly string BaseUrl;
		protected readonly string BusinessUnitName;
		protected static readonly object Lock = new object();

		protected LoadAbstractTest(string baseUrl, string businessUnitName)
		{
			BaseUrl = baseUrl;
			BusinessUnitName = businessUnitName;
		}

		protected abstract T RunTest(UserData user);

		public async Task<List<T>> RunAsync(List<UserData> users, int maxDownloads)
		{
			var concurrentQueue = new ConcurrentQueue<T>();

			using (var semaphore = new SemaphoreSlim(maxDownloads))
			{
				var tasks = users.Select(user => Task.Factory.StartNew(() =>
				{
					semaphore.Wait();
					try
					{
						var runTest = RunTest(user);
						concurrentQueue.Enqueue(runTest);
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

	public class LoadReportsTest : LoadAbstractTest<ReportTimingData>
	{
		public LoadReportsTest(string baseUrl, string businessUnitName):base(baseUrl, businessUnitName)
		{
		}

		protected override ReportTimingData RunTest(UserData user)
		{
			var timingData = new ReportTimingData();
			using (var trafficSimulator = new SimulateReportsTraffic())
			{
				trafficSimulator.Start(BaseUrl, BusinessUnitName, user.Username, user.Password);
				var loginTaken = trafficSimulator.LogOn();
				var timeTaken = trafficSimulator.GoToReportsController();
				lock (Lock)
				{
					timingData.Logon += loginTaken.TotalMilliseconds;
					timingData.Reports += timeTaken.TotalMilliseconds;
				}
			}
			return timingData;
		}
	}

	public class ReportTimingData
	{
		public double Logon { get; set; }
		public double Reports { get; set; }
	}

	public class MyTimeScheduleTimingData
	{
		public double Logon { get; set; }
		public double Schedule { get; set; }
		public int Errors { get; set; }
	}
}