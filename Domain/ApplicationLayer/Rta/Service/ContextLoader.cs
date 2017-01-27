using System;
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
			process(new singleStrategy(_config, _agentStatePersister, action, _databaseLoader, input, _now.UtcDateTime()));
		}

		public void ForBatch(BatchInputModel batch, Action<Context> action)
		{
			process(new batchStrategy(_config, _agentStatePersister, _databaseLoader, action, batch, _now.UtcDateTime()));
		}

		public void ForClosingSnapshot(DateTime snapshotId, string sourceId, Action<Context> action)
		{
			var logons = WithUnitOfWork(() => _agentStatePersister.FindForClosingSnapshot(snapshotId, sourceId, Rta.LogOutBySnapshot));
			process(new closingSnapshotStrategy(_config, _agentStatePersister, action, snapshotId, logons, _now.UtcDateTime()));
		}

		public void ForActivityChanges(Action<Context> action)
		{
			process(new activityChangesStrategy(_config, _agentStatePersister, _scheduleReader, action, _now.UtcDateTime()));
		}

		protected class activityChangesStrategy : baseStrategy<ExternalLogon>
		{
			private readonly IScheduleReader _scheduleReader;

			public activityChangesStrategy(IConfigReader config, IAgentStatePersister persister, IScheduleReader scheduleReader, Action<Context> action, DateTime time) : base(config, persister, action, time)
			{
				_scheduleReader = scheduleReader;
				ParallelTransactions = _config.ReadValue("RtaActivityChangesParallelTransactions", 7);
				MaxTransactionSize = _config.ReadValue("RtaActivityChangesMaxTransactionSize", 100);
				DeadLockVictim = DeadLockVictim.Yes;
			}

			public override IEnumerable<ExternalLogon> AllItems(strategyContext context)
			{
				IEnumerable<ExternalLogonForCheck> externalLogons = null;
				IEnumerable<ScheduledActivity> schedule = null;
				context.withUnitOfWork(() =>
				{
					externalLogons = _persister.FindForCheck();
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
				return _persister.Get(ids.Select(x => x.PersonId).ToArray(), DeadLockVictim);
			}

			public override InputInfo GetInputFor(AgentState state)
			{
				return null;
			}
		}

		private class singleStrategy : baseStrategy<StateInputModel>
		{
			private readonly IDatabaseLoader _databaseLoader;
			private readonly StateInputModel _model;

			public singleStrategy(IConfigReader config, IAgentStatePersister persister, Action<Context> action, IDatabaseLoader databaseLoader, StateInputModel model, DateTime time) : base(config, persister, action, time)
			{
				_databaseLoader = databaseLoader;
				_model = model;
				ParallelTransactions = 1;
				MaxTransactionSize = 1;
			}

			public override IEnumerable<StateInputModel> AllItems(strategyContext context)
			{
				return new[] {_model};
			}

			public override IEnumerable<AgentState> LockNLoad(IEnumerable<StateInputModel> things, strategyContext context)
			{
				try
				{
					var dataSourceId = ValidateSourceId(_databaseLoader, _model);

					var userCode = _model.UserCode;
					var agentStates = _persister.Find(new ExternalLogon {DataSourceId = dataSourceId, UserCode = userCode}, DeadLockVictim);
					if (agentStates.IsEmpty())
						throw new InvalidUserCodeException($"No person found for SourceId {_model.SourceId} and UserCode {_model.UserCode}");

					return agentStates;
				}
				catch (Exception e)
				{
					context.addException(e);
				}

				return Enumerable.Empty<AgentState>();
			}

			public override InputInfo GetInputFor(AgentState state)
			{
				return new InputInfo
				{
					PlatformTypeId = _model.PlatformTypeId,
					SourceId = _model.SourceId,
					SnapshotId = _model.SnapshotId,
					StateCode = _model.StateCode,
					StateDescription = _model.StateDescription
				};
			}
		}

		private class batchStrategy : baseStrategy<BatchStateInputModel>
		{
			private readonly IDatabaseLoader _databaseLoader;
			private readonly BatchInputModel _batch;

			public batchStrategy(IConfigReader config, IAgentStatePersister persister, IDatabaseLoader databaseLoader, Action<Context> action, BatchInputModel batch, DateTime time) : base(config, persister, action, time)
			{
				_databaseLoader = databaseLoader;
				_batch = batch;
				ParallelTransactions = _config.ReadValue("RtaBatchParallelTransactions", 7);
				MaxTransactionSize = _config.ReadValue("RtaBatchMaxTransactionSize", 100);

				var realUpdater = UpdateAgentState;
				UpdateAgentState = context =>
				{
					var stopwatch = new Stopwatch();
					stopwatch.Start();
					realUpdater(context);
					stopwatch.Stop();
					_logger.Debug($"Update agentstate completed, agent: {context.PersonId}, time: {stopwatch.ElapsedMilliseconds}");
				};
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
				var agentStates = _persister.Find(userCodes, DeadLockVictim);
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
			private readonly IEnumerable<ExternalLogon> _things;

			public closingSnapshotStrategy(IConfigReader config, IAgentStatePersister persister, Action<Context> action, DateTime snapshotId, IEnumerable<ExternalLogon> things, DateTime time) : base(config, persister, action, time)
			{
				_snapshotId = snapshotId;
				_things = things;
				ParallelTransactions = _config.ReadValue("RtaCloseSnapshotParallelTransactions", 3);
				MaxTransactionSize = _config.ReadValue("RtaCloseSnapshotMaxTransactionSize", 1000);
			}

			public override IEnumerable<ExternalLogon> AllItems(strategyContext context)
			{
				return _things.OrderBy(x => x.NormalizedString()).ToArray();
			}

			public override IEnumerable<AgentState> LockNLoad(IEnumerable<ExternalLogon> ids, strategyContext context)
			{
				return _persister.Find(ids, DeadLockVictim);
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
			protected readonly IConfigReader _config;
			protected readonly IAgentStatePersister _persister;

			protected baseStrategy(
				IConfigReader config,
				IAgentStatePersister persister,
				Action<Context> action,
				DateTime time
			)
			{
				_config = config;
				_persister = persister;
				Action = action;
				CurrentTime = time;
				DeadLockVictim = DeadLockVictim.No;
				UpdateAgentState = c => _persister.Update(c.MakeAgentState());
			}

			public DateTime CurrentTime { get; }
			public DeadLockVictim DeadLockVictim { get; protected set; }
			public int ParallelTransactions { get; protected set; }
			public int MaxTransactionSize { get; protected set; }

			public abstract IEnumerable<T> AllItems(strategyContext context);

			public abstract IEnumerable<AgentState> LockNLoad(IEnumerable<T> things, strategyContext context);

			public abstract InputInfo GetInputFor(AgentState state);

			public virtual Func<AgentState> GetStored(AgentState state)
			{
				return () => state;
			}

			public Action<Context> UpdateAgentState { get; protected set; }

			public Action<Context> Action { get; }
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

			Action<Context> UpdateAgentState { get; }

			Action<Context> Action { get; }

			Func<AgentState> GetStored(AgentState state);
		}

		public class strategyContext
		{
			public Action<Action> withUnitOfWork;
			public Action<Exception> addException;
		}

		public class transactionData
		{
			public IEnumerable<AgentState> agentStates;
			public IEnumerable<Mapping> mappings;
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
			return new transactionData
			{
				agentStates = agentStates,
				mappings = _mappingReader.Read(),
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
						state.PersonId,
						state.BusinessUnitId,
						state.TeamId.GetValueOrDefault(),
						state.SiteId.GetValueOrDefault(),
						strategy.GetStored(state),
						() => data.schedules.Where(s => s.PersonId == state.PersonId).ToArray(),
						data.mappings,
						strategy.UpdateAgentState,
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