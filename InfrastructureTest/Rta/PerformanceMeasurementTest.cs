using System.Diagnostics;
using System.Linq;
using NUnit.Framework;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.ReadModelUpdaters;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.IocCommon;
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
	[Toggle(Toggles.RTA_PersonOrganizationQueryOptimization_40261)]
	[Toggle(Toggles.RTA_ScheduleQueryOptimization_40260)]
	[Toggle(Toggles.RTA_ConnectionQueryOptimizeAllTheThings_40262)]
	[Explicit]
	public class PerformanceMeasurementTest : ISetup
	{
		public Database Database;
		public AnalyticsDatabase Analytics;
		public Domain.ApplicationLayer.Rta.Service.Rta Rta;
		public FakeConfigReader Config;
		public ConfigurableSyncEventPublisher Publisher;

		public void Setup(ISystem system, IIocConfiguration configuration)
		{
			system.UseTestDouble<ConfigurableSyncEventPublisher>().For<IEventPublisher>();
		}

		[Test]
		public void MeasureBatch([Values(1,2,3,4,5,6,7,8,9,10)] int transactions, [Values("A", "B", "C")] string version)
		{
			Config.FakeSetting("RtaBatchTransactions", transactions.ToString());

			Publisher.AddHandler<MappingReadModelUpdater>();
			Publisher.AddHandler<PersonAssociationChangedEventPublisher>();
			Publisher.AddHandler<AgentStateMaintainer>();
			Analytics.WithDataSource(9, "sourceId");
			Database
				.WithStateGroup("phone")
				.WithStateCode("phone");
			Enumerable.Range(0, 100).ForEach(x => Database.WithStateGroup($"code{x}").WithStateCode($"code{x}"));
			Enumerable.Range(0, 10).ForEach(x => Database.WithActivity($"activity{x}"));
			var userCodes = Enumerable.Range(0, 1000).Select(x => $"user{x}").ToArray();
			userCodes.ForEach(x => Database.WithAgent(x));
			Publisher.Publish(new TenantMinuteTickEvent());

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

		[Test]
		public void MeasureActivityChecker()
		{
			Publisher.AddHandler<MappingReadModelUpdater>();
			Publisher.AddHandler<PersonAssociationChangedEventPublisher>();
			Publisher.AddHandler<AgentStateMaintainer>();
			Analytics.WithDataSource(9, "sourceId");
			Database
				.WithStateGroup("phone")
				.WithStateCode("phone");
			Enumerable.Range(0, 100).ForEach(x => Database.WithStateGroup($"code{x}").WithStateCode($"code{x}"));
			Enumerable.Range(0, 10).ForEach(x => Database.WithActivity($"activity{x}"));
			var userCodes = Enumerable.Range(0, 12000).Select(x => $"user{x}").ToArray();
			userCodes.ForEach(x => Database.WithAgent(x));
			Publisher.Publish(new TenantMinuteTickEvent());
			userCodes
				.Batch(1000)
				.Select(x => new BatchForTest
				{
					States = x.Select(y => new BatchStateForTest
					{
						UserCode = y,
						StateCode = "phone"
					}).ToArray()
				}).ForEach(Rta.SaveStateBatch);
			
			var results = (
				from transactions in new[] {6, 7, 8, 10}
				from size in new[] {50, 90, 100, 110}
				from variation in new[] {"A", "B", "C"}
				select new {transactions, size, variation}).Select(x =>
				{
					Config.FakeSetting("RtaAllParallelTransactions", x.transactions.ToString());
					Config.FakeSetting("RtaAllBatchSize", x.size.ToString());

					var timer = new Stopwatch();
					timer.Start();

					Rta.CheckForActivityChanges(DataSourceHelper.TestTenantName);

					timer.Stop();
					return new
					{
						timer.Elapsed,
						x.transactions,
						x.size,
						x.variation
					};
				});

			results
				.OrderBy(x => x.Elapsed)
				.ForEach(x => Debug.WriteLine($"{x.Elapsed} - {x.size} {x.transactions} {x.variation}"));

		}
	}
}