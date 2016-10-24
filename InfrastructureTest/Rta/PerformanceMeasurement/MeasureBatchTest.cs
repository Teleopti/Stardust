using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.ReadModelUpdaters;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeRepositories.Rta;
using Teleopti.Ccc.TestCommon.IoC;

namespace Teleopti.Ccc.InfrastructureTest.Rta.PerformanceMeasurement
{
	[TestFixture]
	[InfrastructureTest]
	[Toggle(Toggles.RTA_BatchConnectionOptimization_40116)]
	[Toggle(Toggles.RTA_BatchQueryOptimization_40169)]
	[Toggle(Toggles.RTA_PersonOrganizationQueryOptimization_40261)]
	[Toggle(Toggles.RTA_ScheduleQueryOptimization_40260)]
	[Toggle(Toggles.RTA_ConnectionQueryOptimizeAllTheThings_40262)]
	[Toggle(Toggles.RTA_FasterUpdateOfScheduleChanges_40536)]
	[Explicit]
	[Category("LongRunning")]
	public class MeasureBatchTest : PerformanceMeasurementTestBase, ISetup
	{
		public Database Database;
		public Domain.ApplicationLayer.Rta.Service.Rta Rta;
		public FakeConfigReader Config;
		public ConfigurableSyncEventPublisher Publisher;
		public AgentStateMaintainer Maintainer;
		public StateStreamSynchronizer Synchronizer;
		public MutableNow Now;

		public void Setup(ISystem system, IIocConfiguration configuration)
		{
			system.UseTestDouble<ConfigurableSyncEventPublisher>().For<IEventPublisher>();
		}

		public override void OneTimeSetUp()
		{
			Publisher.AddHandler<MappingReadModelUpdater>();
			Publisher.AddHandler<PersonAssociationChangedEventPublisher>();
			Publisher.AddHandler<AgentStateMaintainer>();
			Analytics.WithDataSource(9, "sourceId");
			Database.WithDefaultScenario("default");
			stateCodes.ForEach(x => Database.WithStateGroup(x).WithStateCode(x));
			Enumerable.Range(0, 10).ForEach(x => Database.WithActivity($"activity{x}"));

			MakeUsersFaster(userCodes);

			// trigger tick to populate mappings
			Publisher.Publish(new TenantMinuteTickEvent());

			// single state and init (touch will think its already done)
			Rta.SaveState(new StateForTest
			{
				UserCode = userCodes.First(),
				StateCode = "code1"
			});
			Synchronizer.Initialize();
		}

		private static IEnumerable<string> userCodes => Enumerable.Range(0, 1000).Select(x => $"user{x}").ToArray();
		private static IEnumerable<string> stateCodes => Enumerable.Range(0, 100).Select(x => $"code{x}").ToArray();
		
		[Test]
		public void Measure(
			[Values(5, 7, 9)] int parallelTransactions,
			[Values(80, 100, 150)] int transactionSize,
			[Values(50, 500, 1000)] int batchSize,
			[Values("A", "B", "C")] string variation
		)
		{
			Config.FakeSetting("RtaBatchParallelTransactions", parallelTransactions.ToString());
			Config.FakeSetting("RtaBatchMaxTransactionSize", transactionSize.ToString());

			var states = 5000;

			var batches = Enumerable.Range(0, states)
				.Batch(batchSize)
				.Select(s => new BatchForTest
				{
					States = userCodes
						.RandomizeBetter()
						.Take(batchSize)
						.Select(y => new BatchStateForTest
						{
							UserCode = y,
							StateCode = stateCodes.RandomizeBetter().First()
						})
						.ToArray()
				}).ToArray();

			batches.ForEach(Rta.SaveStateBatch);
		}

	}
}