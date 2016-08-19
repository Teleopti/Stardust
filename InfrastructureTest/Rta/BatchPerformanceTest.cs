using System.Diagnostics;
using System.Linq;
using NUnit.Framework;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.ReadModelUpdaters;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeRepositories.Rta;
using Teleopti.Ccc.TestCommon.IoC;

namespace Teleopti.Ccc.InfrastructureTest.Rta
{
	[TestFixture]
	[MultiDatabaseTest]
	[Toggle(Toggles.RTA_Optimize_39667)]
	[Toggle(Toggles.RTA_RuleMappingOptimization_39812)]
	[Toggle(Toggles.RTA_BatchConnectionOptimization_40116)]
	[Toggle(Toggles.RTA_BatchQueryOptimization_40169)]
	[Explicit]
	public class BatchPerformanceTest
	{
		public Database Database;
		public AnalyticsDatabase Analytics;
		public Domain.ApplicationLayer.Rta.Service.Rta Rta;
		public MappingReadModelUpdater MappingUpdater;
		public FakeConfigReader Config;

		[Test]
		public void Measure([Values(1,2,3,4,5,6,7,8,9,10)] int transactions, [Values("A", "B", "C")] string version)
		{
			Config.FakeSetting("RtaBatchTransactions", transactions.ToString());

			Analytics.WithDataSource(9, "sourceId");
			Database
				.WithStateGroup("phone")
				.WithStateCode("phone");
			Enumerable.Range(0, 100).ForEach(x => Database.WithStateGroup($"code{x}").WithStateCode($"code{x}"));
			Enumerable.Range(0, 10).ForEach(x => Database.WithActivity($"activity{x}"));
			var userCodes = Enumerable.Range(0, 1000).Select(x => $"user{x}").ToArray();
			userCodes.ForEach(x => Database.WithAgent(x));
			MappingUpdater.Handle(new TenantMinuteTickEvent());

			var batches = Enumerable.Range(0, 100)
				.Select(x =>
					new BatchForTest
					{
						States = userCodes
							.Randomize()
							.Take(50)
							.Select(y => new BatchStateForTest
							{
								UserCode = y,
								StateCode = "phone"
							})
							.ToArray()
					}
				);

			var timer = new Stopwatch();
			timer.Start();

			batches.ForEach(Rta.SaveStateBatch);

			timer.Stop();
			Debug.WriteLine(timer.Elapsed);

		}

	}
}