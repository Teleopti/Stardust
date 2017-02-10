using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Config;
using Teleopti.Ccc.Domain.Logon.Aspects;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service
{
	public class ContextLoaderWithSpreadTransactionLockStrategy : ContextLoader
	{
		public ContextLoaderWithSpreadTransactionLockStrategy(ICurrentDataSource dataSource, IDatabaseLoader databaseLoader, INow now, StateMapper stateMapper, ExternalLogonMapper externalLogonMapper, ScheduleCache scheduleCache, IAgentStatePersister agentStatePersister, ProperAlarm appliedAlarm, IConfigReader config, DeadLockRetrier deadLockRetrier, IKeyValueStorePersister keyValues, RtaProcessor processor, IAgentStateReadModelUpdater agentStateReadModelUpdater) : base(dataSource, databaseLoader, now, stateMapper, externalLogonMapper, scheduleCache, agentStatePersister, appliedAlarm, config, deadLockRetrier, keyValues, processor, agentStateReadModelUpdater)
		{
		}

		protected override void ProcessTransactions(string tenant, IContextLoadingStrategy strategy, IEnumerable<Func<IEnumerable<AgentState>>> transactions, ConcurrentBag<Exception> exceptions)
		{
			var parentThreadName = Thread.CurrentThread.Name;

			var taskTransactionCount = Math.Max(2, transactions.Count()/strategy.ParallelTransactions);

			var tasks = transactions
				.Batch(taskTransactionCount)
				.Select(transactionBatch =>
				{
					return Task.Factory.StartNew(() =>
					{
						if (Thread.CurrentThread.Name == null)
							Thread.CurrentThread.Name = $"{parentThreadName} #{Thread.CurrentThread.ManagedThreadId}";
						transactionBatch.ForEach(transactionData =>
						{
							try
							{
								_deadLockRetrier.RetryOnDeadlock(() => Transaction(tenant, strategy, transactionData));
							}
							catch (Exception e)
							{
								exceptions.Add(e);
							}
						});
					});
				})
				.ToArray();

			Task.WaitAll(tasks);
		}

	}
	




	public class ContextLoader : IContextLoader
	{
		private readonly ICurrentDataSource _dataSource;
		private readonly IDatabaseLoader _databaseLoader;
		protected readonly INow _now;
		private readonly StateMapper _stateMapper;
		private readonly ExternalLogonMapper _externalLogonMapper;
		private readonly ScheduleCache _scheduleCache;
		protected readonly IAgentStatePersister _agentStatePersister;
		private readonly ProperAlarm _appliedAlarm;
		protected readonly IConfigReader _config;
		protected readonly DeadLockRetrier _deadLockRetrier;
		private readonly IKeyValueStorePersister _keyValues;
		private readonly RtaProcessor _processor;
		private readonly IAgentStateReadModelUpdater _agentStateReadModelUpdater;

		public ContextLoader(ICurrentDataSource dataSource, IDatabaseLoader databaseLoader, INow now, StateMapper stateMapper, ExternalLogonMapper externalLogonMapper, ScheduleCache scheduleCache, IAgentStatePersister agentStatePersister, ProperAlarm appliedAlarm, IConfigReader config, DeadLockRetrier deadLockRetrier, IKeyValueStorePersister keyValues, RtaProcessor processor, IAgentStateReadModelUpdater agentStateReadModelUpdater)
		{
			_dataSource = dataSource;
			_databaseLoader = databaseLoader;
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
			_agentStateReadModelUpdater = agentStateReadModelUpdater;
		}

		public void For(StateInputModel input)
		{
			Process(new BatchStrategy(
				new BatchInputModel
				{
					AuthenticationKey = input.AuthenticationKey,
					PlatformTypeId = input.PlatformTypeId,
					SnapshotId = input.SnapshotId,
					SourceId = input.SourceId,
					States = new[]
					{
						new BatchStateInputModel
						{
							StateCode = input.StateCode,
							StateDescription = input.StateDescription,
							UserCode = input.UserCode
						}
					}
				},
				_now.UtcDateTime(),
				_config,
				_agentStatePersister,
				_databaseLoader,
				_externalLogonMapper
			));
		}

		public void ForBatch(BatchInputModel batch)
		{
			Process(new BatchStrategy(batch, _now.UtcDateTime(), _config, _agentStatePersister, _databaseLoader, _externalLogonMapper));
		}

		public void ForClosingSnapshot(DateTime snapshotId, string sourceId)
		{
			Process(new ClosingSnapshotStrategy(snapshotId, sourceId, _now.UtcDateTime(), _config, _agentStatePersister, _stateMapper, _databaseLoader));
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
				AddException = exceptions.Add
			};
			var items = strategy.PersonIds(strategyContext);
			var itemsCount = items.Count();
			if (itemsCount > 0)
			{
				var transactionSize = calculateTransactionSize(strategy.MaxTransactionSize, strategy.ParallelTransactions, itemsCount);

				var transactions = items
					.Batch(transactionSize)
					.Select(some => new Func<IEnumerable<AgentState>>(() =>
					{
						var result = _agentStatePersister.LockNLoad(some, strategy.DeadLockVictim);
						_stateMapper.RefreshMappingCache(result.MappingVersion);
						_scheduleCache.Refresh(result.ScheduleVersion);
						return result.AgentStates;
					}))
					.ToArray();

				ProcessTransactions(_dataSource.CurrentName(), strategy, transactions, exceptions);
			}

			if (exceptions.Count == 1)
			{
				var e = exceptions.First();
				PreserveStack.For(e);
				throw e;
			}
			if (exceptions.Any())
				throw new System.AggregateException(exceptions);
		}

		private static int calculateTransactionSize(int maxTransactionSize, int parallelTransactions, int thingsCount)
		{
			if (thingsCount <= maxTransactionSize* parallelTransactions)
				maxTransactionSize = (int) Math.Ceiling(thingsCount/(double)parallelTransactions);
			return maxTransactionSize;
		}

		protected virtual void ProcessTransactions(string tenant, IContextLoadingStrategy strategy, IEnumerable<Func<IEnumerable<AgentState>>> transactions, ConcurrentBag<Exception> exceptions)
		{
			var parentThreadName = Thread.CurrentThread.Name;
			Parallel.ForEach(
				transactions,
				new ParallelOptions { MaxDegreeOfParallelism = strategy.ParallelTransactions },
				data =>
				{
					try
					{
						if (Thread.CurrentThread.Name == null)
							Thread.CurrentThread.Name = $"{parentThreadName} #{Thread.CurrentThread.ManagedThreadId}";
						_deadLockRetrier.RetryOnDeadlock(() => Transaction(tenant, strategy, data));
					}
					catch (Exception e)
					{
						exceptions.Add(e);
					}
				}
			);
		}

		[TenantScope]
		[LogInfo]
		protected virtual void Transaction(
			string tenant,
			IContextLoadingStrategy strategy,
			Func<IEnumerable<AgentState>> agentStates
			)
		{
			WithUnitOfWork(() =>
			{
				agentStates.Invoke().ForEach(state =>
				{
					var context = new Context(
						strategy.CurrentTime,
						strategy.DeadLockVictim,
						strategy.GetInputFor(state),
						state,
						() => _scheduleCache.Read(state.PersonId),
						c => _agentStatePersister.Update(c.MakeAgentState()),
						_stateMapper,
						_appliedAlarm
					);

					var events = _processor.Process(context);

					if (context.ShouldProcessState())
						_agentStateReadModelUpdater.Update(context, events, context.DeadLockVictim);

				});
			});
		}

		[AllBusinessUnitsUnitOfWork]
		[ReadModelUnitOfWork]
		[AnalyticsUnitOfWork]
		protected virtual void WithUnitOfWork(Action action)
		{
			action.Invoke();
		}

		[AllBusinessUnitsUnitOfWork]
		[ReadModelUnitOfWork]
		[AnalyticsUnitOfWork]
		protected virtual T WithUnitOfWork<T>(Func<T> func)
		{
			return func.Invoke();
		}

		protected int ValidateSourceId(string sourceId)
		{
			return ValidateSourceId(_databaseLoader, sourceId);
		}

		public static int ValidateSourceId(IDatabaseLoader databaseLoader, string sourceId)
		{
			if (string.IsNullOrEmpty(sourceId))
				throw new InvalidSourceException("Source id is required");
			int dataSourceId;
			if (!databaseLoader.Datasources().TryGetValue(sourceId, out dataSourceId))
				throw new InvalidSourceException($"Source id \"{sourceId}\" not found");
			return dataSourceId;
		}
	}

}