using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.ReadModelUpdaters;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.UnitOfWork;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeRepositories.Rta;
using Teleopti.Ccc.TestCommon.IoC;

namespace Teleopti.Ccc.InfrastructureTest.Rta
{
	[TestFixture]
	[MultiDatabaseTest]
	[Toggle(Toggles.RTA_SpreadTransactionLocksStrategy_41823)]
	[Explicit]
	[Category("LongRunning")]
	public class DeadlockTest : ISetup
	{
		public Database Database;
		public AnalyticsDatabase Analytics;
		public Domain.ApplicationLayer.Rta.Service.Rta Rta;
		public ConcurrencyRunner Run;
		public ICurrentDataSource DataSource;
		public FakeEventPublisher Publisher;
		public IKeyValueStorePersister KeyValues;
		public WithReadModelUnitOfWork WithReadModelUnitOfWork;
		public WithUnitOfWork UnitOfWork;
		public RetryingQueueSimulator QueueSimulator;
		public IPersonRepository Persons;
		public INow Now;

		public void Setup(ISystem system, IIocConfiguration configuration)
		{
			system.AddService<RetryingQueueSimulator>();
			system.UseTestDouble<FakeEventPublisher>().For<IEventPublisher>();
		}

		private void setup()
		{
			Publisher.AddHandler<PersonAssociationChangedEventPublisher>();
			Publisher.AddHandler<AgentStateMaintainer>();
			Publisher.AddHandler<MappingReadModelUpdater>();
			Publisher.AddHandler<ExternalLogonReadModelUpdater>();
			Publisher.AddHandler<ScheduleChangeProcessor>();

			Analytics.WithDataSource(9, "sourceId");
		}

		private IEnumerable<string> createStateCodes()
		{
			var stateCodes = Enumerable.Range(0, 10).Select(x => $"statecode{x}").ToArray();
			stateCodes.ForEach(x =>
			{
				Database
					.WithStateGroup(x)
					.WithStateCode(x);
			});
			Database.PublishRecurringEvents();
			return stateCodes;
		}

		private IEnumerable<string> createUsers(int count)
		{
			var uniqueUsers = Enumerable.Range(0, count).Select(x => $"user{x}").ToArray();
			uniqueUsers.ForEach(x => Database.WithAgent(x));
			Database.PublishRecurringEvents();
			return uniqueUsers;
		}

		private void createCrossMappedUsers(int count)
		{
			var sharingUsers = Enumerable.Range(0, count).Select(x => $"user{x}sharing").ToArray();
			sharingUsers.ForEach(x =>
			{
				Database
					.WithAgent(x)
					.WithExternalLogon(sharingUsers.RandomizeBetter().First())
					;
			});
			Database.PublishRecurringEvents();
		}

		private void triggerExternalLogonAndAgentStatePrepareByPersonAssociation()
		{
			WithReadModelUnitOfWork.Do(() =>
			{
				KeyValues.Update("PersonAssociationChangedPublishTrigger", true);
			});
		}

		private void triggerScheduleChanges()
		{
			var personids = UnitOfWork.Get(() => Persons.LoadAll().Select(x => x.Id.Value).ToArray());
			personids.ForEach(x => Publisher.Publish(new ScheduleChangedEvent
			{
				PersonId = x,
				StartDateTime = Now.UtcDateTime().Date.AddDays(-2),
				EndDateTime = Now.UtcDateTime().Date.AddDays(2)
			}));
		}

		private void runRecurringJobsIncludingActivityChecker()
		{
			Database.PublishRecurringEvents();
		}

		private void sendBatches(int states, int size, IEnumerable<string> users, IEnumerable<string> stateCodes)
		{
			var batches = Enumerable.Range(0, states)
				.Batch(size)
				.Select(_ => new BatchForTest
				{
					SourceId = "sourceId",
					States = users
						.Randomize()
						.Take(size)
						.Select(y => new BatchStateForTest
						{
							UserCode = y,
							StateCode = stateCodes.Randomize().First()
						}).ToArray()
				}).ToArray();
			batches.ForEach(Rta.SaveStateBatch);
		}

		private void sendSingles(int states, IEnumerable<string> users, IEnumerable<string> stateCodes)
		{
			var singles = Enumerable.Range(0, states)
				.Select(_ => new StateForTest
				{
					SourceId = "sourceId",
					UserCode = users.Randomize().First(),
					StateCode = stateCodes.Randomize().First()
				}).ToArray();
			singles.ForEach(Rta.SaveState);
		}

		[Test]
		public void ShouldNotDeadlockBetweenBatchTransactions()
		{
			setup();
			var stateCodes = createStateCodes();
			var users = createUsers(500);

			Run.InParallel(() =>
			{
				sendBatches(10000, 500, users, stateCodes);
			});

			Run.Wait();
		}

		[Test]
		public void ShouldNotDeadlockBetweenBatchTransactionsWithCrossMappedExternalLogons()
		{
			setup();
			var stateCodes = createStateCodes();
			var users = createUsers(500);
			createCrossMappedUsers(500);

			Run.InParallel(() =>
			{
				sendBatches(10000, 500, users, stateCodes);
			});

			Run.Wait();
		}

		[Test]
		public void ShouldNotDeadlockBetweenBatchesAndActivityChecker()
		{
			setup();
			var stateCodes = createStateCodes();
			var users = createUsers(500);

			var done = false;
			Run.InParallel(() =>
			{
				while (!done)
					triggerScheduleChanges();
			});
			Run.InParallel(() =>
			{
				while (!done)
					runRecurringJobsIncludingActivityChecker();
			});
			Run.InParallel(() =>
			{
				try
				{
					sendBatches(10000, 500, users, stateCodes);
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
			setup();
			var stateCodes = createStateCodes();
			var userCodes = createUsers(10);

			var done = false;
			Run.InParallel(() =>
			{
				while (!done)
					triggerExternalLogonAndAgentStatePrepareByPersonAssociation();
			});
			Run.InParallel(() =>
			{
				while (!done)
					runRecurringJobsIncludingActivityChecker();
			});
			Run.InParallel(() =>
			{
				try
				{
					sendSingles(100, userCodes, stateCodes);
				}
				finally
				{
					done = true;
				}
			});

			Run.Wait();
		}

		[Test]
		public void ShouldNotDeadlockThisMess([Range(1, 5)] int times)
		{
			setup();
			var stateCodes = createStateCodes();
			var users = createUsers(500);
			createCrossMappedUsers(500);

			var done = false;
			Run.InParallel(() =>
			{
				while (!done)
					triggerExternalLogonAndAgentStatePrepareByPersonAssociation();
			});
			Run.InParallel(() =>
			{
				while (!done)
					triggerScheduleChanges();
			});
			Run.InParallel(() =>
			{
				while (!done)
					runRecurringJobsIncludingActivityChecker();
			});
			Run.InParallel(() =>
			{
				try
				{
					sendBatches(10000, 500, users, stateCodes);
				}
				finally
				{
					done = true;
				}
			});

			Run.Wait();
		}
		
	}
}