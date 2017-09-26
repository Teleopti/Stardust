using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.ExceptionServices;
using System.Threading;
using System.Threading.Tasks;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Config;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Logon.Aspects;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service
{
	public class ContextLoaderWithSpreadTransactionLockStrategy : ContextLoader
	{
		public ContextLoaderWithSpreadTransactionLockStrategy(ICurrentDataSource dataSource,
			DataSourceMapper dataSourceMapper, INow now, StateMapper stateMapper, ExternalLogonMapper externalLogonMapper,
			ScheduleCache scheduleCache, IAgentStatePersister agentStatePersister, ProperAlarm appliedAlarm,
			IConfigReader config, DeadLockRetrier deadLockRetrier, IKeyValueStorePersister keyValues,
			AgentStateProcessor processor, IEventPublisherScope eventPublisherScope, ICurrentEventPublisher eventPublisher,
			IRtaTracer tracer) : base(dataSource, dataSourceMapper, now, stateMapper, externalLogonMapper, scheduleCache,
			agentStatePersister, appliedAlarm, config, deadLockRetrier, keyValues, processor, eventPublisherScope,
			eventPublisher, tracer)

		{
		}

		protected override void ProcessTransactions(string tenant, IContextLoadingStrategy strategy,
			IEnumerable<Func<IEnumerable<AgentState>>> transactions, ConcurrentBag<Exception> exceptions)
		{
			var taskTransactionCount = Math.Max(2, transactions.Count() / strategy.ParallelTransactions);

			var tasks = transactions
				.Batch(taskTransactionCount)
				.Select(transactionBatch =>
				{
					return Task.Factory.StartNew(() =>
					{
						transactionBatch.ForEach(transaction => { ProcessTransaction(tenant, strategy, transaction, exceptions); });
					});
				})
				.ToArray();

			Task.WaitAll(tasks);
		}
	}


	public class ContextLoader : IContextLoader
	{
		private readonly ICurrentDataSource _dataSource;
		private readonly DataSourceMapper _dataSourceMapper;
		protected readonly INow _now;
		private readonly StateMapper _stateMapper;
		private readonly ExternalLogonMapper _externalLogonMapper;
		private readonly ScheduleCache _scheduleCache;
		protected readonly IAgentStatePersister _agentStatePersister;
		private readonly ProperAlarm _appliedAlarm;
		protected readonly IConfigReader _config;
		protected readonly DeadLockRetrier _deadLockRetrier;
		private readonly IKeyValueStorePersister _keyValues;
		private readonly AgentStateProcessor _processor;
		private readonly IEventPublisherScope _eventPublisherScope;
		private readonly ICurrentEventPublisher _eventPublisher;
		private readonly IRtaTracer _tracer;

		public ContextLoader(ICurrentDataSource dataSource, DataSourceMapper dataSourceMapper, INow now,
			StateMapper stateMapper, ExternalLogonMapper externalLogonMapper, ScheduleCache scheduleCache,
			IAgentStatePersister agentStatePersister, ProperAlarm appliedAlarm, IConfigReader config,
			DeadLockRetrier deadLockRetrier, IKeyValueStorePersister keyValues, AgentStateProcessor processor,
			IEventPublisherScope eventPublisherScope, ICurrentEventPublisher eventPublisher, IRtaTracer tracer)
		{
			_dataSource = dataSource;
			_dataSourceMapper = dataSourceMapper;
			_now = now;
			_stateMapper = stateMapper;
			_externalLogonMapper = externalLogonMapper;
			_scheduleCache = scheduleCache;
			_agentStatePersister = agentStatePersister;
			_appliedAlarm = appliedAlarm;
			_config = config;
			_deadLockRetrier = deadLockRetrier;
			_keyValues = keyValues;
			_processor = processor;
			_eventPublisherScope = eventPublisherScope;
			_eventPublisher = eventPublisher;
			_tracer = tracer;
		}

		public void ForBatch(BatchInputModel batch)
		{
			Process(new BatchStrategy(batch, _now.UtcDateTime(), _config, _agentStatePersister, _dataSourceMapper,
				_externalLogonMapper, _tracer));
		}

		public void ForClosingSnapshot(DateTime snapshotId, string sourceId)
		{
			Process(new ClosingSnapshotStrategy(snapshotId, sourceId, _now.UtcDateTime(), _config, _agentStatePersister,
				_stateMapper, _dataSourceMapper));
		}

		public void ForActivityChanges()
		{
			Process(new ActivityChangesStrategy(_now.UtcDateTime(), _config, _agentStatePersister, _keyValues, _scheduleCache));
		}

		protected void Process(IContextLoadingStrategy strategy)
		{
			var exceptions = new ConcurrentBag<Exception>();

			var strategyContext = new StrategyContext
			{
				WithUnitOfWork = WithUnitOfWork,
				WithReadModelUnitOfWork = WithReadModelUnitOfWork,
				AddException = exceptions.Add
			};
			var items = strategy.PersonIds(strategyContext);
			var itemsCount = items.Count();
			if (itemsCount > 0)
			{
				var transactionSize = calculateTransactionSize(strategy.MaxTransactionSize, strategy.ParallelTransactions,
					itemsCount);

				var transactions = items
					.Batch(transactionSize)
					.Select(some => new Func<IEnumerable<AgentState>>(() =>
					{
						var result = _agentStatePersister.LockNLoad(some, strategy.DeadLockVictim);
						refreshCaches(strategy, strategyContext, result.ScheduleVersion, result.MappingVersion);
						return result.AgentStates;
					}))
					.ToArray();

				ProcessTransactions(_dataSource.CurrentName(), strategy, transactions, exceptions);
			}
			else
			{
				CurrentScheduleReadModelVersion scheduleVersion = null;
				string mappingVersion = null;
				strategyContext.WithReadModelUnitOfWork(() =>
				{
					scheduleVersion = _keyValues.Get("CurrentScheduleReadModelVersion", () => null);
					mappingVersion = _keyValues.Get("RuleMappingsVersion");
				});
				refreshCaches(strategy, strategyContext, scheduleVersion, mappingVersion);
			}

			if (exceptions.Count == 1)
			{
				var e = exceptions.First();
				ExceptionDispatchInfo.Capture(e).Throw();
			}
			if (exceptions.Any())
				throw new System.AggregateException(exceptions);
		}

		private void refreshCaches(IContextLoadingStrategy strategy, StrategyContext strategyContext,
			CurrentScheduleReadModelVersion scheduleVersion, string mappingVersion)
		{
			_scheduleCache.Refresh(scheduleVersion);
			_stateMapper.Refresh(mappingVersion);
			strategy.VerifyConfiguration(strategyContext);
		}

		private static int calculateTransactionSize(int maxTransactionSize, int parallelTransactions, int thingsCount)
		{
			if (thingsCount <= maxTransactionSize * parallelTransactions)
				maxTransactionSize = (int) Math.Ceiling(thingsCount / (double) parallelTransactions);
			return maxTransactionSize;
		}

		protected virtual void ProcessTransactions(string tenant, IContextLoadingStrategy strategy,
			IEnumerable<Func<IEnumerable<AgentState>>> transactions, ConcurrentBag<Exception> exceptions)
		{
			Parallel.ForEach(
				transactions,
				new ParallelOptions {MaxDegreeOfParallelism = strategy.ParallelTransactions},
				data => { ProcessTransaction(tenant, strategy, data, exceptions); }
			);
		}

		[TenantScope]
		[LogInfo]
		protected virtual void ProcessTransaction(
			string tenant,
			IContextLoadingStrategy strategy,
			Func<IEnumerable<AgentState>> transaction,
			ConcurrentBag<Exception> exceptions)
		{
			try
			{
				if (Thread.CurrentThread.Name == null)
					Thread.CurrentThread.Name = $"{strategy.ParentThreadName} #{Thread.CurrentThread.ManagedThreadId}";

				_deadLockRetrier.RetryOnDeadlock(() => Transaction(tenant, strategy, transaction));
			}
			catch (Exception e)
			{
				exceptions.Add(e);
			}
		}

		protected virtual void Transaction(
			string tenant,
			IContextLoadingStrategy strategy,
			Func<IEnumerable<AgentState>> agentStates
		)
		{
			WithUnitOfWork(() =>
			{
				var eventCollector = new EventCollector(_eventPublisher);
				using (_eventPublisherScope.OnThisThreadPublishTo(eventCollector))
				{
					agentStates
						.Invoke()
						.Select(state =>
							new ProcessInput(
								strategy.CurrentTime,
								strategy.DeadLockVictim,
								strategy.GetInputFor(state),
								state,
								_scheduleCache.Read(state.PersonId),
								_stateMapper,
								_appliedAlarm
							)
						)
						.Select(x => _processor.Process(x))
						.Where(x => x.Processed)
						.ForEach(x => _agentStatePersister.Update(x.State));
				}
				// have to publish events inside the transaction with the person lock
				// because AgentStateReadModelUpdater, which is run in memory, does not handle the concurrency
				eventCollector.Publish();
			});
		}

		[ReadModelUnitOfWork]
		protected virtual void WithReadModelUnitOfWork(Action action)
		{
			action.Invoke();
		}

		[AllBusinessUnitsUnitOfWork]
		protected virtual void WithUnitOfWork(Action action)
		{
			action.Invoke();
		}

		[AllBusinessUnitsUnitOfWork]
		protected virtual T WithUnitOfWork<T>(Func<T> action)
		{
			return action.Invoke();
		}
	}
}