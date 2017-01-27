using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using log4net;
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
		public ContextLoaderWithSpreadTransactionLockStrategy(
			ICurrentDataSource dataSource, IDatabaseLoader databaseLoader, INow now, StateMapper stateMapper,
			IAgentStatePersister agentStatePersister, IMappingReader mappingReader, IScheduleReader scheduleReader,
			ProperAlarm appliedAlarm, IConfigReader config, DeadLockRetrier deadLockRetrier)
			: base(dataSource, databaseLoader, now, stateMapper, agentStatePersister,
				mappingReader, scheduleReader, appliedAlarm, config, deadLockRetrier)
		{
		}

		protected override void ProcessTransactions<T>(string tenant, IStrategy<T> strategy, IEnumerable<Func<transactionData>> transactions, ConcurrentBag<Exception> exceptions)
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
		private static readonly ILog _logger = LogManager.GetLogger("PerfLog.Rta");
		private readonly ICurrentDataSource _dataSource;
		private readonly IDatabaseLoader _databaseLoader;
		protected readonly INow _now;
		private readonly StateMapper _stateMapper;
		protected readonly IAgentStatePersister _agentStatePersister;
		private readonly IMappingReader _mappingReader;
		private readonly IScheduleReader _scheduleReader;
		private readonly ProperAlarm _appliedAlarm;
		protected readonly IConfigReader _config;
		protected readonly DeadLockRetrier _deadLockRetrier;

		public ContextLoader(
			ICurrentDataSource dataSource,
			IDatabaseLoader databaseLoader,
			INow now,
			StateMapper stateMapper,
			IAgentStatePersister agentStatePersister,
			IMappingReader mappingReader,
			IScheduleReader scheduleReader,
			ProperAlarm appliedAlarm,
			IConfigReader config,
			DeadLockRetrier deadLockRetrier)
		{
			_dataSource = dataSource;
			_databaseLoader = databaseLoader;
			_now = now;
			_stateMapper = stateMapper;
			_agentStatePersister = agentStatePersister;
			_mappingReader = mappingReader;
			_scheduleReader = scheduleReader;
			_appliedAlarm = appliedAlarm;
			_config = config;
			_deadLockRetrier = deadLockRetrier;
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
				_databaseLoader
			));
		}

		public void ForBatch(BatchInputModel batch, Action<Context> action)
		{
			process(new batchStrategy(batch, action, _now.UtcDateTime(), _config, _agentStatePersister, _databaseLoader));
		}

		public void ForClosingSnapshot(DateTime snapshotId, string sourceId, Action<Context> action)
		{
			process(new closingSnapshotStrategy(snapshotId, sourceId, action, _now.UtcDateTime(), _config, _agentStatePersister));
		}

		public void ForActivityChanges(Action<Context> action)
		{
			process(new activityChangesStrategy(action, _now.UtcDateTime(), _config, _agentStatePersister, _scheduleReader));
		}

		protected class activityChangesStrategy : baseStrategy<ExternalLogon>
		{
			private readonly IScheduleReader _scheduleReader;

			public activityChangesStrategy(Action<Context> action, DateTime time, IConfigReader config, IAgentStatePersister persister, IScheduleReader scheduleReader) : base(config, persister, action, time)
			{
				_scheduleReader = scheduleReader;
				ParallelTransactions = Config.ReadValue("RtaActivityChangesParallelTransactions", 7);
				MaxTransactionSize = Config.ReadValue("RtaActivityChangesMaxTransactionSize", 100);
				DeadLockVictim = DeadLockVictim.Yes;
			}

			public override IEnumerable<ExternalLogon> AllItems(strategyContext context)
			{
				IEnumerable<ExternalLogonForCheck> externalLogons = null;
				IEnumerable<ScheduledActivity> schedule = null;
				context.withUnitOfWork(() =>
				{
					externalLogons = Persister.FindForCheck();
					schedule = _scheduleReader.Read();
				});
				return externalLogons
					.Where(x =>
					{
						var activities = schedule.Where(a => a.PersonId == x.PersonId).ToArray();
						var nextCheck = ScheduleInfo.NextCheck(activities, x.LastTimeWindowCheckSum, x.LastCheck);
						return nextCheck == null || nextCheck <= CurrentTime;
					})
					.ToArray();
			}

			public override IEnumerable<AgentState> LockNLoad(IEnumerable<ExternalLogon> ids, strategyContext context)
			{
				return Persister.Get(ids.Select(x => x.PersonId).ToArray(), DeadLockVictim);
			}

			public override InputInfo GetInputFor(AgentState state)
			{
				return null;
			}
		}
		
		private class batchStrategy : baseStrategy<BatchStateInputModel>
		{
			private readonly IDatabaseLoader _databaseLoader;
			private readonly BatchInputModel _batch;

			public batchStrategy(BatchInputModel batch, Action<Context> action, DateTime time, IConfigReader config, IAgentStatePersister persister, IDatabaseLoader databaseLoader) : base(config, persister, action, time)
			{
				_databaseLoader = databaseLoader;
				_batch = batch;
				ParallelTransactions = Config.ReadValue("RtaBatchParallelTransactions", 7);
				MaxTransactionSize = Config.ReadValue("RtaBatchMaxTransactionSize", 100);
			}

			public override IEnumerable<BatchStateInputModel> AllItems(strategyContext context)
			{
				return _batch.States.OrderBy(x => x.UserCode).ToArray();
			}

			public override IEnumerable<AgentState> LockNLoad(IEnumerable<BatchStateInputModel> states, strategyContext context)
			{
				var dataSourceId = ValidateSourceId(_databaseLoader, _batch);
				var userCodes = states
					.Select(x => new ExternalLogon
					{
						DataSourceId = dataSourceId,
						UserCode = x.UserCode
					})
					.ToArray();
				var stopwatch = new Stopwatch();
				stopwatch.Start();
				var agentStates = Persister.Find(userCodes, DeadLockVictim);
				stopwatch.Stop();
				_logger.Debug($"Load agents completed, time: {stopwatch.ElapsedMilliseconds}");

				userCodes
					.Where(x => agentStates.All(s => s.UserCode != x.UserCode))
					.Select(x => new InvalidUserCodeException($"No person found for UserCode {x}, DataSourceId {dataSourceId}, SourceId {_batch.SourceId}"))
					.ForEach(context.addException);

				return agentStates;
			}

			public override InputInfo GetInputFor(AgentState state)
			{
				var s = state as AgentStateFound;
				var input = _batch.States.Single(x => x.UserCode == s.UserCode);
				return new InputInfo
				{
					PlatformTypeId = _batch.PlatformTypeId,
					SnapshotId = _batch.SnapshotId,
					SourceId = _batch.SourceId,
					StateCode = input.StateCode,
					StateDescription = input.StateDescription,
				};
			}
		}

		private class closingSnapshotStrategy : baseStrategy<ExternalLogon>
		{
			private readonly DateTime _snapshotId;
			private readonly string _sourceId;

			public closingSnapshotStrategy(DateTime snapshotId, string sourceId, Action<Context> action, DateTime time, IConfigReader config, IAgentStatePersister persister) : base(config, persister, action, time)
			{
				_snapshotId = snapshotId;
				_sourceId = sourceId;
				ParallelTransactions = Config.ReadValue("RtaCloseSnapshotParallelTransactions", 3);
				MaxTransactionSize = Config.ReadValue("RtaCloseSnapshotMaxTransactionSize", 1000);
			}

			public override IEnumerable<ExternalLogon> AllItems(strategyContext context)
			{
				IEnumerable<ExternalLogon> logons = null;
				context.withUnitOfWork(() =>
				{
					logons = Persister.FindForClosingSnapshot(_snapshotId, _sourceId, Rta.LogOutBySnapshot);
				});
				return logons.OrderBy(x => x.NormalizedString()).ToArray();
			}

			public override IEnumerable<AgentState> LockNLoad(IEnumerable<ExternalLogon> ids, strategyContext context)
			{
				return Persister.Find(ids, DeadLockVictim);
			}

			public override InputInfo GetInputFor(AgentState state)
			{
				return new InputInfo
				{
					StateCode = Rta.LogOutBySnapshot,
					PlatformTypeId = Guid.Empty.ToString(),
					SnapshotId = _snapshotId
				};
			}
		}

		protected abstract class baseStrategy<T> : IStrategy<T>
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

			public abstract IEnumerable<T> AllItems(strategyContext context);
			public abstract IEnumerable<AgentState> LockNLoad(IEnumerable<T> things, strategyContext context);
			public abstract InputInfo GetInputFor(AgentState state);
			
		}
		
		public interface IStrategy<T>
		{
			DateTime CurrentTime { get; }
			DeadLockVictim DeadLockVictim { get; }

			int ParallelTransactions { get; }
			int MaxTransactionSize { get; }

			IEnumerable<T> AllItems(strategyContext context);

			IEnumerable<AgentState> LockNLoad(IEnumerable<T> things, strategyContext context);

			InputInfo GetInputFor(AgentState state);

			Action<Context> Action { get; }
		}

		public class strategyContext
		{
			public Action<Action> withUnitOfWork;
			public Action<Exception> addException;
		}

		public class transactionData
		{
			public IEnumerable<AgentState> agentStates;
			public IEnumerable<ScheduledActivity> schedules;
		}

		protected void process<T>(IStrategy<T> strategy)
		{
			var exceptions = new ConcurrentBag<Exception>();

			var strategyContext = new strategyContext
			{
				withUnitOfWork = WithUnitOfWork,
				addException = exceptions.Add
			};
			var items = strategy.AllItems(strategyContext);
			var itemsCount = items.Count();
			if (itemsCount == 0)
				return;

			var transactionSize = calculateTransactionSize(strategy.MaxTransactionSize, strategy.ParallelTransactions, itemsCount);

			var transactions = items
				.Batch(transactionSize)
				.Select(some => new Func<transactionData>(() => loadTransactionData(strategy, strategyContext, some)))
				.ToArray();

			ProcessTransactions(_dataSource.CurrentName(), strategy, transactions, exceptions);

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

		private transactionData loadTransactionData<T>(IStrategy<T> strategy, strategyContext context, IEnumerable<T> items)
		{
			var agentStates = strategy.LockNLoad(items, context);
			_stateMapper.RefreshMappingCache();
			return new transactionData
			{
				agentStates = agentStates,
				schedules = _scheduleReader.Read()
			};
		}

		protected virtual void ProcessTransactions<T>(string tenant, IStrategy<T> strategy, IEnumerable<Func<transactionData>> transactions, ConcurrentBag<Exception> exceptions)
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
		protected virtual void Transaction<T>(
			string tenant,
			IStrategy<T> strategy,
			Func<transactionData> transactionData
			)
		{
			WithUnitOfWork(() =>
			{
				var data = transactionData.Invoke();

				data.agentStates.ForEach(state =>
				{
					strategy.Action.Invoke(new Context(
						strategy.CurrentTime,
						strategy.DeadLockVictim,
						strategy.GetInputFor(state),
						state,
						() => data.schedules.Where(s => s.PersonId == state.PersonId).ToArray(),
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

		protected int ValidateSourceId(IValidatable input)
		{
			return ValidateSourceId(_databaseLoader, input);
		}

		public static int ValidateSourceId(IDatabaseLoader databaseLoader, IValidatable input)
		{
			if (string.IsNullOrEmpty(input.SourceId))
				throw new InvalidSourceException("Source id is required");
			int dataSourceId;
			if (!databaseLoader.Datasources().TryGetValue(input.SourceId, out dataSourceId))
				throw new InvalidSourceException($"Source id \"{input.SourceId}\" not found");
			return dataSourceId;
		}
	}
}