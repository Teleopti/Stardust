using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Linq;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Config;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service
{
	public interface IContextLoadingStrategy
	{
		DateTime CurrentTime { get; }
		DeadLockVictim DeadLockVictim { get; }
		int ParallelTransactions { get; }
		int MaxTransactionSize { get; }

		IEnumerable<Guid> PersonIds(StrategyContext context);
		InputInfo GetInputFor(AgentState state);
	}

	public class StrategyContext
	{
		public Action<Action> WithUnitOfWork;
		public Action<Exception> AddException;
	}

	public abstract class ContextLoadingStrategy : IContextLoadingStrategy
	{
		protected ContextLoadingStrategy(
			IConfigReader config,
			IAgentStatePersister persister,
			DateTime time
		)
		{
			Config = config;
			Persister = persister;
			CurrentTime = time;
			DeadLockVictim = DeadLockVictim.No;
		}

		public DateTime CurrentTime { get; }
		protected IConfigReader Config { get; }
		protected IAgentStatePersister Persister { get; }
		public DeadLockVictim DeadLockVictim { get; protected set; }
		public int ParallelTransactions { get; protected set; }
		public int MaxTransactionSize { get; protected set; }

		public abstract IEnumerable<Guid> PersonIds(StrategyContext context);
		public abstract InputInfo GetInputFor(AgentState state);
	}

	public class BatchStrategy : ContextLoadingStrategy
	{
		private readonly IDatabaseLoader _databaseLoader;
		private readonly ExternalLogonMapper _externalLogonMapper;
		private readonly BatchInputModel _batch;
		private int _dataSourceId;
		private IDictionary<Guid, BatchStateInputModel> _matches;

		public BatchStrategy(BatchInputModel batch, DateTime time, IConfigReader config, IAgentStatePersister persister, IDatabaseLoader databaseLoader, ExternalLogonMapper externalLogonMapper) : base(config, persister, time)
		{
			_databaseLoader = databaseLoader;
			_externalLogonMapper = externalLogonMapper;
			_batch = batch;
			ParallelTransactions = Config.ReadValue("RtaBatchParallelTransactions", 7);
			MaxTransactionSize = Config.ReadValue("RtaBatchMaxTransactionSize", 100);
		}

		public override IEnumerable<Guid> PersonIds(StrategyContext context)
		{
			context.WithUnitOfWork(() =>
			{
				_dataSourceId = ContextLoader.ValidateSourceId(_databaseLoader, _batch.SourceId);
				_externalLogonMapper.Refresh();
			});
			_matches = _batch.States
				.SelectMany(state =>
				{
					var personIds = _externalLogonMapper.PersonIdsFor(_dataSourceId, state.UserCode);
					if (personIds.IsEmpty())
						context.AddException(new InvalidUserCodeException($"No person found for UserCode {state.UserCode}, DataSourceId {_dataSourceId}, SourceId {_batch.SourceId}"));
					return personIds.Select(id =>
						new
						{
							state,
							id
						});
				})
				.GroupBy(x => x.id, x => x.state)
				.ToDictionary(x => x.Key, x => x.First());
			return _matches.Keys
				.OrderBy(x => new SqlGuid(x))
				.ToArray();
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

	public class ClosingSnapshotStrategy : ContextLoadingStrategy
	{
		private readonly DateTime _snapshotId;
		private readonly string _sourceId;
		private readonly StateMapper _stateMapper;
		private readonly IDatabaseLoader _databaseLoader;
		private int _dataSourceId;

		public ClosingSnapshotStrategy(DateTime snapshotId, string sourceId, DateTime time, IConfigReader config, IAgentStatePersister persister, StateMapper stateMapper, IDatabaseLoader databaseLoader) : base(config, persister, time)
		{
			_snapshotId = snapshotId;
			_sourceId = sourceId;
			_stateMapper = stateMapper;
			_databaseLoader = databaseLoader;
			ParallelTransactions = Config.ReadValue("RtaCloseSnapshotParallelTransactions", 3);
			MaxTransactionSize = Config.ReadValue("RtaCloseSnapshotMaxTransactionSize", 1000);
		}

		public override IEnumerable<Guid> PersonIds(StrategyContext context)
		{
			IEnumerable<Guid> personIds = null;
			context.WithUnitOfWork(() =>
			{
				_dataSourceId = ContextLoader.ValidateSourceId(_databaseLoader, _sourceId);
				personIds = Persister.FindForClosingSnapshot(_snapshotId, _dataSourceId, _stateMapper.LoggedOutStateGroupIds());
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

	public class ActivityChangesStrategy : ContextLoadingStrategy
	{
		private readonly IKeyValueStorePersister _keyValues;
		private readonly ScheduleCache _scheduleCache;

		public ActivityChangesStrategy(DateTime time, IConfigReader config, IAgentStatePersister persister, IKeyValueStorePersister keyValues, ScheduleCache scheduleCache) : base(config, persister, time)
		{
			_keyValues = keyValues;
			_scheduleCache = scheduleCache;
			ParallelTransactions = Config.ReadValue("RtaActivityChangesParallelTransactions", 7);
			MaxTransactionSize = Config.ReadValue("RtaActivityChangesMaxTransactionSize", 100);
			DeadLockVictim = DeadLockVictim.Yes;
		}

		public override IEnumerable<Guid> PersonIds(StrategyContext context)
		{
			IEnumerable<PersonForCheck> persons = null;
			context.WithUnitOfWork(() =>
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
}