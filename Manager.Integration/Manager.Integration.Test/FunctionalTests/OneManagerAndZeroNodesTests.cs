using System;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Manager.Integration.Test.Data;
using Manager.Integration.Test.Helpers;
using Manager.Integration.Test.Initializers;
using Manager.Integration.Test.Models;
using Manager.Integration.Test.Timers;
using Manager.IntegrationTest.Console.Host.Helpers;
using Manager.IntegrationTest.Console.Host.Log4Net.Extensions;
using Newtonsoft.Json;
using NUnit.Framework;

namespace Manager.Integration.Test.FunctionalTests
{
	[TestFixture]
	public class OneManagerAndZeroNodesTests : InitialzeAndFinalizeOneManagerAndZeroNodes
	{
		[Test]
		public void JobsShouldJustBeQueuedIfNoNodesTest()
		{
			Thread.Sleep(TimeSpan.FromSeconds(15));

			var httpSender = new HttpSender();
			var mangerUriBuilder = new ManagerUriBuilder();
			var uri = mangerUriBuilder.GetAddToJobQueueUri();

			this.Log().DebugWithLineNumber("Start.");

			var testJobTimerParams =
				new TestJobTimerParams("Loop", TimeSpan.FromSeconds(1));

			var jobParamsToJson = JsonConvert.SerializeObject(testJobTimerParams);

			var jobQueueItem = new JobQueueItem
			{
				Name = "Job Name",
				Serialized = jobParamsToJson,
				Type = "NodeTest.JobHandlers.TestJobTimerParams",
				CreatedBy = "WPF Client"
			};

			var checkTablesInManagerDbTimer = 
				new CheckTablesInManagerDbTimer(ManagerDbConnectionString,100);

			var taskCheckData = new Task(() =>
			{
				checkTablesInManagerDbTimer.JobQueueTimer.Start();

				HttpResponseMessage response = httpSender.PostAsync(uri, 
																    jobQueueItem).Result;

				while (!response.IsSuccessStatusCode)
				{
					response = httpSender.PostAsync(uri, jobQueueItem).Result;

					Thread.Sleep(TimeSpan.FromSeconds(5));
				}
			});

			taskCheckData.Start();
			taskCheckData.Wait(TimeSpan.FromMinutes(5));

			Assert.IsTrue(checkTablesInManagerDbTimer.ManagerDbRepository.Jobs.Count() == 1);

			Assert.IsTrue(!checkTablesInManagerDbTimer.ManagerDbRepository.Jobs.Any());

			checkTablesInManagerDbTimer.Dispose();
		}
	}
}