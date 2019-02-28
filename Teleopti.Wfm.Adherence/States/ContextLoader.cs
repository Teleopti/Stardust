using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.ExceptionServices;
using System.Threading;
using System.Threading.Tasks;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Config;
using Teleopti.Ccc.Domain.Infrastructure;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Logon.Aspects;
using Teleopti.Wfm.Adherence.Tracer;

namespace Teleopti.Wfm.Adherence.States
{
	public class ContextLoader : IContextLoader
	{
		private readonly ICurrentDataSource _dataSource;
		private readonly DataSourceMapper _dataSourceMapper;
		private readonly INow _now;
		private readonly StateMapper _stateMapper;
		private readonly ExternalLogonMapper _externalLogonMapper;
		private readonly BelongsToDateMapper _belongsToDateMapper;
		private readonly ScheduleCache _scheduleCache;
		private readonly IAgentStatePersister _agentStatePersister;
		private readonly ProperAlarm _appliedAlarm;
		private readonly IConfigReader _config;
		private readonly DeadLockRetrier _deadLockRetrier;
		private readonly IKeyValueStorePersister _keyValues;
		private readonly AgentStateProcessor _processor;
		private readonly ICurrentEventPublisher _eventPublisher;
		private readonly IRtaTracer _tracer;

		public ContextLoader(
			ICurrentDataSource dataSource, 
			DataSourceMapper dataSourceMapper, 
			INow now,
			StateMapper stateMapper, 
			ExternalLogonMapper externalLogonMapper, 
			BelongsToDateMapper belongsToDateMapper,
			ScheduleCache scheduleCache,
			IAgentStatePersister agentStatePersister, 
			ProperAlarm appliedAlarm, 
			IConfigReader config,
			DeadLockRetrier deadLockRetrier, 
			IKeyValueStorePersister keyValues, 
			AgentStateProcessor processor,
			ICurrentEventPublisher eventPublisher, 
			IRtaTracer tracer)
		{
			_dataSource = dataSource;
			_dataSourceMapper = dataSourceMapper;
			_now = now;
			_stateMapper = stateMapper;
			_externalLogonMapper = externalLogonMapper;
			_belongsToDateMapper = belongsToDateMapper;
			_scheduleCache = scheduleCache;
			_agentStatePersister = agentStatePersister;
			_appliedAlarm = appliedAlarm;
			_config = config;
			_deadLockRetrier = deadLockRetrier;
			_keyValues = keyValues;
			_processor = processor;
			_eventPublisher = eventPublisher;
			_tracer = tracer;
		}

		[LogInfo]
		public virtual void ForBatch(BatchInputModel batch) => Process(new BatchStrategy(batch, _now.UtcDateTime(), _config, _agentStatePersister, _dataSourceMapper, _externalLogonMapper, _tracer));

		[LogInfo]
		public virtual void ForClosingSnapshot(DateTime snapshotId, string sourceId) => Process(new ClosingSnapshotStrategy(snapshotId, sourceId, _now.UtcDateTime(), _config, _agentStatePersister, _stateMapper, _dataSourceMapper, _tracer));

		[LogInfo]
		public virtual void ForActivityChanges() => Process(new ActivityChangesStrategy(_now.UtcDateTime(), _config, _agentStatePersister, _keyValues, _scheduleCache, _externalLogonMapper, _tracer));

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
				var transactionSize = calculateTransactionSize(strategy.MaxTransactionSize, strategy.ParallelTransactions, itemsCount);

				var transactions = items
					.Batch(transactionSize)
					.Select(some => new Func<IEnumerable<AgentInTransaction>>(() =>
					{
						var result = _agentStatePersister.LockNLoad(some, strategy.DeadLockVictim);
						refreshCaches(strategy, strategyContext, result.ScheduleVersion, result.MappingVersion);
						return result.AgentStates
							.Select(x => new AgentInTransaction
							{
								State = x,
								Trace = strategy.GetTraceFor(x)
							});
					}))
					.ToArray();

				processTransactions(_dataSource.CurrentName(), strategy, transactions, exceptions);
			}
			else
			{
				CurrentScheduleReadModelVersion scheduleVersion = null;
				strategyContext.WithReadModelUnitOfWork(() => { scheduleVersion = _keyValues.Get("CurrentScheduleReadModelVersion", () => null); });
				refreshCaches(strategy, strategyContext, scheduleVersion, null);
			}

			if (exceptions.Count == 1)
			{
				var e = exceptions.First();
				ExceptionDispatchInfo.Capture(e).Throw();
			}

			if (exceptions.Any())
				throw new System.AggregateException(exceptions);
		}

		private void refreshCaches(IContextLoadingStrategy strategy, StrategyContext strategyContext, CurrentScheduleReadModelVersion scheduleVersion, string mappingVersion)
		{
			_scheduleCache.Refresh(scheduleVersion);
			if (mappingVersion != null)
				_stateMapper.Refresh(mappingVersion);
			else
				_stateMapper.Refresh();
			strategy.VerifyConfiguration(strategyContext);
		}

		private static int calculateTransactionSize(int maxTransactionSize, int parallelTransactions, int thingsCount)
		{
			if (thingsCount <= maxTransactionSize * parallelTransactions)
				maxTransactionSize = (int) Math.Ceiling(thingsCount / (double) parallelTransactions);
			return maxTransactionSize;
		}

		private void processTransactions(
			string tenant,
			IContextLoadingStrategy strategy,
			IEnumerable<Func<IEnumerable<AgentInTransaction>>> transactions,
			ConcurrentBag<Exception> exceptions)
		{
			// transaction spreading over sql clustered index strategy optimization...
			// minimizing chance for page lock dead locks ;)

			var taskTransactionCount = Math.Max(2, transactions.Count() / strategy.ParallelTransactions);

			var tasks = transactions
				.Batch(taskTransactionCount)
				.Select(transactionBatch =>
				{
					var task = Task.Run(() => { transactionBatch.ForEach(transaction => { ProcessTransaction(tenant, strategy, transaction, exceptions); }); });
					task.ConfigureAwait(false);
					return task;
				})
				.ToArray();

			Task.WaitAll(tasks);
		}

		[TenantScope]
		[LogInfo]
		protected virtual void ProcessTransaction(
			string tenant,
			IContextLoadingStrategy strategy,
			Func<IEnumerable<AgentInTransaction>> transaction,
			ConcurrentBag<Exception> exceptions)
		{
			try
			{
				if (Thread.CurrentThread.Name == null)
					Thread.CurrentThread.Name = $"{strategy.ParentThreadName} #{Thread.CurrentThread.ManagedThreadId}";

				_deadLockRetrier.RetryOnDeadlock(() => this.transaction(strategy, transaction));
			}
			catch (Exception e)
			{
				exceptions.Add(e);
			}
		}

		public class AgentInTransaction
		{
			public AgentState State;
			public StateTraceLog Trace;
		}

		private void transaction(
			IContextLoadingStrategy strategy,
			Func<IEnumerable<AgentInTransaction>> agentStates
		)
		{
			WithUnitOfWork(() =>
			{
				var events = agentStates
						.Invoke()
						.Select(x =>
							new ProcessInput(
								strategy.CurrentTime,
								strategy.DeadLockVictim,
								strategy.GetInputFor(x.State),
								x.State,
								_scheduleCache.Read(x.State.PersonId),
								_stateMapper,
								_externalLogonMapper,
								_belongsToDateMapper,
								_appliedAlarm,
								x.Trace
							)
						)
						.Select(x => _processor.Process(x))
						.SelectMany(x =>
						{
							if (x.Processed)
							{
								_agentStatePersister.Update(x.State);
								_tracer.StateProcessed(x.TraceLog, x.Events);
							}

							return x.Events;
						})
						.ToArray()
					;
				// have to publish events inside the transaction with the person lock
				// because AgentStateReadModelUpdater, which is run in memory, does not handle the concurrency
				_eventPublisher.Current().Publish(events);
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