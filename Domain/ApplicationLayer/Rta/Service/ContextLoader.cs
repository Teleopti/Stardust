using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
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
		public ContextLoaderWithSpreadTransactionLockStrategy(ICurrentDataSource dataSource, IDatabaseLoader databaseLoader, INow now, StateMapper stateMapper, ExternalLogonMapper externalLogonMapper, ScheduleCache scheduleCache, IAgentStatePersister agentStatePersister, ProperAlarm appliedAlarm, IConfigReader config, DeadLockRetrier deadLockRetrier, IKeyValueStorePersister keyValues) : base(dataSource, databaseLoader, now, stateMapper, externalLogonMapper, scheduleCache, agentStatePersister, appliedAlarm, config, deadLockRetrier, keyValues)
		{
		}

		protected override void ProcessTransactions(string tenant, IStrategy strategy, IEnumerable<Func<IEnumerable<AgentState>>> transactions, ConcurrentBag<Exception> exceptions)
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

		public ContextLoader(
			ICurrentDataSource dataSource,
			IDatabaseLoader databaseLoader,
			INow now,
			StateMapper stateMapper,
			ExternalLogonMapper externalLogonMapper,
			ScheduleCache scheduleCache,
			IAgentStatePersister agentStatePersister,
			ProperAlarm appliedAlarm,
			IConfigReader config,
			DeadLockRetrier deadLockRetrier,
			IKeyValueStorePersister keyValues
			)
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
		}

		public void For(StateInputModel input, Action<Context> action)
		{
			process(new batchStrategy(
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
				action,
				_now.UtcDateTime(),
				_config,
				_agentStatePersister,
				_databaseLoader,
				_externalLogonMapper
			));
		}

		public void ForBatch(BatchInputModel batch, Action<Context> action)
		{
			process(new batchStrategy(batch, action, _now.UtcDateTime(), _config, _agentStatePersister, _databaseLoader, _externalLogonMapper));
		}

		public void ForClosingSnapshot(DateTime snapshotId, string sourceId, Action<Context> action)
		{
			process(new closingSnapshotStrategy(snapshotId, sourceId, action, _now.UtcDateTime(), _config, _agentStatePersister, _databaseLoader));
		}

		public void ForActivityChanges(Action<Context> action)
		{
			process(new activityChangesStrategy(action, _now.UtcDateTime(), _config, _agentStatePersister, _keyValues, _scheduleCache));
		}

		protected class activityChangesStrategy : baseStrategy
		{
			private readonly IKeyValueStorePersister _keyValues;
			private readonly ScheduleCache _scheduleCache;

			public activityChangesStrategy(Action<Context> action, DateTime time, IConfigReader config, IAgentStatePersister persister, IKeyValueStorePersister keyValues, ScheduleCache scheduleCache) : base(config, persister, action, time)
			{
				_keyValues = keyValues;
				_scheduleCache = scheduleCache;
				ParallelTransactions = Config.ReadValue("RtaActivityChangesParallelTransactions", 7);
				MaxTransactionSize = Config.ReadValue("RtaActivityChangesMaxTransactionSize", 100);
				DeadLockVictim = DeadLockVictim.Yes;
			}

			public override IEnumerable<Guid> PersonIds(strategyContext context)
			{
				IEnumerable<PersonForCheck> persons = null;
				context.withUnitOfWork(() =>
				{
					_scheduleCache.Refresh(_keyValues.Get("CurrentScheduleReadModelVersion"));
					persons = Persister.FindForCheck();
				});
				return persons
					.Where(x =>
					{
						var activities = _scheduleCache.Read(x.PersonId);
						var nextCheck = ScheduleInfo.NextCheck(activities, x.LastTimeWindowCheckSum, x.LastCheck);
						return nextCheck == null || nextCheck <= CurrentTime;
					})
					.Select(x => x.PersonId)
					.ToArray();
			}

			public override InputInfo GetInputFor(AgentState state)
			{
				return null;
			}
		}
		
		private class batchStrategy : baseStrategy
		{
			private readonly IDatabaseLoader _databaseLoader;
			private readonly ExternalLogonMapper _externalLogonMapper;
			private readonly BatchInputModel _batch;
			private int _dataSourceId;
			private IDictionary<Guid, BatchStateInputModel> _matches;

			public batchStrategy(BatchInputModel batch, Action<Context> action, DateTime time, IConfigReader config, IAgentStatePersister persister, IDatabaseLoader databaseLoader, ExternalLogonMapper externalLogonMapper) : base(config, persister, action, time)
			{
				_databaseLoader = databaseLoader;
				_externalLogonMapper = externalLogonMapper;
				_batch = batch;
				ParallelTransactions = Config.ReadValue("RtaBatchParallelTransactions", 7);
				MaxTransactionSize = Config.ReadValue("RtaBatchMaxTransactionSize", 100);
			}

			public override IEnumerable<Guid> PersonIds(strategyContext context)
			{
				context.withUnitOfWork(() =>
				{
					_dataSourceId = ValidateSourceId(_databaseLoader, _batch.SourceId);
					_externalLogonMapper.Refresh();
				});
				_matches = _batch.States
					.SelectMany(state =>
					{
						var personIds = _externalLogonMapper.PersonIdsFor(_dataSourceId, state.UserCode);
						if (personIds.IsEmpty())
							context.addException(new InvalidUserCodeException($"No person found for UserCode {state.UserCode}, DataSourceId {_dataSourceId}, SourceId {_batch.SourceId}"));
						return personIds.Select(id =>
							new
							{
								state,
								id
							});
					})
					.GroupBy(x => x.id, x => x.state)
					.ToDictionary(x => x.Key, x => x.First());
				return _matches.Keys;
			}
			
			public override InputInfo GetInputFor(AgentState state)
			{
				var input = _matches[state.PersonId];
				return new InputInfo
				{
					PlatformTypeId = _batch.PlatformTypeId,
					SourceId = _batch.SourceId,
					UserCode = input.UserCode,
					StateCode = input.StateCode,
					StateDescription = input.StateDescription,
					SnapshotId = _batch.SnapshotId,
					SnapshotDataSourceId = _dataSourceId
				};
			}
		}

		private class closingSnapshotStrategy : baseStrategy
		{
			private readonly DateTime _snapshotId;
			private readonly string _sourceId;
			private readonly IDatabaseLoader _databaseLoader;
			private int _dataSourceId;

			public closingSnapshotStrategy(DateTime snapshotId, string sourceId, Action<Context> action, DateTime time, IConfigReader config, IAgentStatePersister persister, IDatabaseLoader databaseLoader) : base(config, persister, action, time)
			{
				_snapshotId = snapshotId;
				_sourceId = sourceId;
				_databaseLoader = databaseLoader;
				ParallelTransactions = Config.ReadValue("RtaCloseSnapshotParallelTransactions", 3);
				MaxTransactionSize = Config.ReadValue("RtaCloseSnapshotMaxTransactionSize", 1000);
			}

			public override IEnumerable<Guid> PersonIds(strategyContext context)
			{
				IEnumerable<Guid> personIds = null;
				context.withUnitOfWork(() =>
				{
					_dataSourceId = ValidateSourceId(_databaseLoader, _sourceId);
					personIds = Persister.FindForClosingSnapshot(_snapshotId, _dataSourceId, Rta.LogOutBySnapshot);
				});
				return personIds;
			}
			
			public override InputInfo GetInputFor(AgentState state)
			{
				return new InputInfo
				{
					StateCode = Rta.LogOutBySnapshot,
					PlatformTypeId = Guid.Empty.ToString(),
					SnapshotId = _snapshotId,
					SnapshotDataSourceId = _dataSourceId
				};
			}
		}

		protected abstract class baseStrategy : IStrategy
		{
			protected baseStrategy(
				IConfigReader config,
				IAgentStatePersister persister,
				Action<Context> action,
				DateTime time
			)
			{
				Config = config;
				Persister = persister;
				Action = action;
				CurrentTime = time;
				DeadLockVictim = DeadLockVictim.No;
			}

			public DateTime CurrentTime { get; }
			protected IConfigReader Config { get; }
			protected IAgentStatePersister Persister { get; }
			public DeadLockVictim DeadLockVictim { get; protected set; }
			public int ParallelTransactions { get; protected set; }
			public int MaxTransactionSize { get; protected set; }
			public Action<Context> Action { get; }

			public abstract IEnumerable<Guid> PersonIds(strategyContext context);
			public abstract InputInfo GetInputFor(AgentState state);
		}

		public interface IStrategy
		{
			DateTime CurrentTime { get; }
			DeadLockVictim DeadLockVictim { get; }
			int ParallelTransactions { get; }
			int MaxTransactionSize { get; }
			Action<Context> Action { get; }

			IEnumerable<Guid> PersonIds(strategyContext context);
			InputInfo GetInputFor(AgentState state);
		}

		public class strategyContext
		{
			public Action<Action> withUnitOfWork;
			public Action<Exception> addException;
		}

		protected void process(IStrategy strategy)
		{
			var exceptions = new ConcurrentBag<Exception>();

			var strategyContext = new strategyContext
			{
				withUnitOfWork = WithUnitOfWork,
				addException = exceptions.Add
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

		protected virtual void ProcessTransactions(string tenant, IStrategy strategy, IEnumerable<Func<IEnumerable<AgentState>>> transactions, ConcurrentBag<Exception> exceptions)
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
			IStrategy strategy,
			Func<IEnumerable<AgentState>> agentStates
			)
		{
			WithUnitOfWork(() =>
			{
				agentStates.Invoke().ForEach(state =>
				{
					strategy.Action.Invoke(new Context(
						strategy.CurrentTime,
						strategy.DeadLockVictim,
						strategy.GetInputFor(state),
						state,
						() => _scheduleCache.Read(state.PersonId),
						c => _agentStatePersister.Update(c.MakeAgentState()),
						_stateMapper,
						_appliedAlarm
					));
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