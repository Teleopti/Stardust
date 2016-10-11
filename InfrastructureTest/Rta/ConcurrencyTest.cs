using System.Linq;
using NUnit.Framework;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.ReadModelUpdaters;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.UnitOfWork;
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
	[Toggle(Toggles.RTA_FasterUpdateOfScheduleChanges_40536)]
	[Explicit]
	[Category("LongRunning")]
	public class ConcurrencyTest : ISetup
	{
		public Database Database;
		public AnalyticsDatabase Analytics;
		public Domain.ApplicationLayer.Rta.Service.Rta Rta;
		public ConcurrencyRunner Run;
		public ICurrentDataSource DataSource;
		public ConfigurableSyncEventPublisher Publisher;
		public IKeyValueStorePersister KeyValues;
		public WithReadModelUnitOfWork WithReadModelUnitOfWork;
		public WithUnitOfWork UnitOfWork;
		public RetryingQueueSimulator QueueSimulator;

		public void Setup(ISystem system, IIocConfiguration configuration)
		{
			system.AddService<RetryingQueueSimulator>();
			system.UseTestDouble<ConfigurableSyncEventPublisher>().For<IEventPublisher>();
		}

		[Test]
		public void ShouldNotDeadlockBetweenProcesses()
		{
			Publisher.AddHandler(typeof(PersonAssociationChangedEventPublisher));
			Publisher.AddHandler(typeof(AgentStateMaintainer));
			Publisher.AddHandler(typeof(MappingReadModelUpdater));
			var tenant = DataSource.CurrentName();
			Analytics.WithDataSource(9, "sourceId");
			var stateCodes = Enumerable.Range(0, 10).Select(x => $"statecode{x}").ToArray();
			stateCodes.ForEach(x =>
			{
				Database
					.WithStateGroup(x)
					.WithStateCode(x);
			});
			var userCodes = Enumerable.Range(0, 2000).Select(x => $"user{x}").ToArray();
			userCodes.ForEach(x => Database.WithAgent(x));
			Database.PublishRecurringEvents();

			var done = false;
			// agent state maintainer
			Run.InParallel(() =>
			{
				while (!done)
				{
					WithReadModelUnitOfWork.Do(() =>
					{
						KeyValues.Update("PersonAssociationChangedPublishTrigger", true);
					});
					QueueSimulator.ProcessAsync(() =>
					{
						Database.PublishRecurringEvents();
					});
					QueueSimulator.WaitForAll();
				}
			});
			// activity checker
			Run.InParallel(() =>
			{
				while (!done)
				{
					Rta.CheckForActivityChanges(tenant);
				}
			});
			// batches
			Run.InParallel(() =>
			{
				var batches = Enumerable.Range(0, 10000)
					.Batch(500)
					.Select(_ => new BatchForTest
					{
						SourceId = "sourceId",
						States = userCodes
							.Randomize()
							.Take(500)
							.Select(y => new BatchStateForTest
							{
								UserCode = y,
								StateCode = stateCodes.Randomize().First()
							}).ToArray()
					}).ToArray();
				try
				{
					batches.ForEach(Rta.SaveStateBatch);
				}
				finally
				{
					done = true;
				}
			});

			Run.Wait();
		}

		[Test]
		public void ShouldNotDeadlockBetweenSaveStateAndPrepareState()
		{
			Publisher.AddHandler(typeof(PersonAssociationChangedEventPublisher));
			Publisher.AddHandler(typeof(AgentStateMaintainer));
			Publisher.AddHandler(typeof(MappingReadModelUpdater));
			Analytics.WithDataSource(9, "sourceId");
			var stateCodes = Enumerable.Range(0, 10).Select(x => $"statecode{x}").ToArray();
			stateCodes.ForEach(x =>
			{
				Database
					.WithStateGroup(x)
					.WithStateCode(x);
			});
			var userCodes = Enumerable.Range(0, 10).Select(x => $"user{x}").ToArray();
			userCodes.ForEach(x => Database.WithAgent(x));
			Database.PublishRecurringEvents();

			Run.InParallel(() =>
			{
				100.Times(() =>
				{
					WithReadModelUnitOfWork.Do(() =>
					{
						KeyValues.Update("PersonAssociationChangedPublishTrigger", true);
					});
					Database.PublishRecurringEvents();
				});
			});
			Run.InParallel(() =>
			{
				100.Times(() =>
				{
					Rta.SaveState(new StateForTest
					{
						SourceId = "sourceId",
						UserCode = userCodes.Randomize().First(),
						StateCode = stateCodes.Randomize().First()
					});
				});
			});

			Run.Wait();
		}
	}
}