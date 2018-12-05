using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Linq;
using System.Threading;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Config;
using Teleopti.Wfm.Adherence.Tracer;

namespace Teleopti.Wfm.Adherence.States
{
	public interface IContextLoadingStrategy
	{
		string ParentThreadName { get; }
		DateTime CurrentTime { get; }
		DeadLockVictim DeadLockVictim { get; }
		int ParallelTransactions { get; }
		int MaxTransactionSize { get; }

		void VerifyConfiguration(StrategyContext strategyContext);

		IEnumerable<Guid> PersonIds(StrategyContext context);
		InputInfo GetInputFor(AgentState state);
		StateTraceLog GetTraceFor(AgentState state);
	}

	public class StrategyContext
	{
		public Action<Action> WithUnitOfWork;
		public Action<Action> WithReadModelUnitOfWork;
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
			ParentThreadName = Thread.CurrentThread.Name;
			Config = config;
			Persister = persister;
			CurrentTime = time;
			DeadLockVictim = DeadLockVictim.No;
		}

		public string ParentThreadName { get; }
		public DateTime CurrentTime { get; }
		protected IConfigReader Config { get; }
		protected IAgentStatePersister Persister { get; }
		public DeadLockVictim DeadLockVictim { get; protected set; }
		public int ParallelTransactions { get; protected set; }
		public int MaxTransactionSize { get; protected set; }

		public virtual void VerifyConfiguration(StrategyContext strategyContext)
		{
		}

		public abstract IEnumerable<Guid> PersonIds(StrategyContext context);
		public abstract InputInfo GetInputFor(AgentState state);
		public abstract StateTraceLog GetTraceFor(AgentState state);
	}

	public class BatchStrategy : ContextLoadingStrategy
	{
		private readonly ExternalLogonMapper _externalLogonMapper;
		private readonly IRtaTracer _tracer;
		private readonly BatchInputModel _batch;
		private readonly int _dataSourceId;
		private IDictionary<Guid, BatchStateInputModel> _matches;

		public BatchStrategy(
			BatchInputModel batch,
			DateTime time,
			IConfigReader config,
			IAgentStatePersister persister,
			DataSourceMapper dataSourceMapper,
			ExternalLogonMapper externalLogonMapper,
			IRtaTracer tracer) :
			base(config, persister, time)
		{
			_externalLogonMapper = externalLogonMapper;
			_tracer = tracer;
			_batch = batch;
			_dataSourceId = dataSourceMapper.ValidateSourceId(_batch.SourceId, batch.States.EmptyIfNull().Select(x => x.TraceLog));
			ParallelTransactions = Config.ReadValue("RtaBatchParallelTransactions", 7);
			MaxTransactionSize = Config.ReadValue("RtaBatchMaxTransactionSize", 100);
		}

		public override IEnumerable<Guid> PersonIds(StrategyContext context)
		{
			_externalLogonMapper.Refresh();
			_matches = _batch.States
				.EmptyIfNull()
				.SelectMany(state =>
				{
					var personIds = _externalLogonMapper.PersonIdsFor(_dataSourceId, state.UserCode);
					if (personIds.IsEmpty())
					{
						_tracer.InvalidUserCode(state.TraceLog);
						context.AddException(new InvalidUserCodeException(
							$"No person found for UserCode {state.UserCode}, DataSourceId {_dataSourceId}, SourceId {_batch.SourceId}"));
					}

					return personIds.Select(id =>
						new
						{
							state,
							id
						});
				})
				.GroupBy(x => x.id, x => x.state)
				.ToDictionary(x => x.Key, x => x.Last());
			return _matches.Keys
				.OrderBy(x => new SqlGuid(x))
				.ToArray();
		}

		public override InputInfo GetInputFor(AgentState state)
		{
			var input = _matches[state.PersonId];
			return new InputInfo
			{
				SourceId = _batch.SourceId,
				UserCode = input.UserCode,
				StateCode = input.StateCode,
				StateDescription = input.StateDescription,
				SnapshotId = _batch.SnapshotId,
				SnapshotDataSourceId = _dataSourceId
			};
		}

		public override StateTraceLog GetTraceFor(AgentState state) => _matches[state.PersonId].TraceLog;
	}

	public class ClosingSnapshotStrategy : ContextLoadingStrategy
	{
		private readonly DateTime _snapshotId;
		private readonly StateMapper _stateMapper;
		private readonly IRtaTracer _tracer;
		private readonly int _dataSourceId;

		public ClosingSnapshotStrategy(
			DateTime snapshotId,
			string sourceId,
			DateTime time,
			IConfigReader config,
			IAgentStatePersister persister,
			StateMapper stateMapper,
			DataSourceMapper dataSourceMapper,
			IRtaTracer tracer) : base(config, persister, time)
		{
			_snapshotId = snapshotId;
			_stateMapper = stateMapper;
			_tracer = tracer;
			_dataSourceId = dataSourceMapper.ValidateSourceId(sourceId, null);
			ParallelTransactions = Config.ReadValue("RtaCloseSnapshotParallelTransactions", 3);
			MaxTransactionSize = Config.ReadValue("RtaCloseSnapshotMaxTransactionSize", 1000);
		}

		public override IEnumerable<Guid> PersonIds(StrategyContext context)
		{
			IEnumerable<Guid> personIds = null;
			context.WithUnitOfWork(() => { personIds = Persister.FindForClosingSnapshot(_snapshotId, _dataSourceId, _stateMapper.LoggedOutStateGroupIds()); });
			return personIds;
		}

		public override InputInfo GetInputFor(AgentState state)
		{
			return new InputInfo
			{
				StateCode = Rta.LogOutBySnapshot,
				SnapshotId = _snapshotId,
				SnapshotDataSourceId = _dataSourceId
			};
		}

		public override StateTraceLog GetTraceFor(AgentState state)
		{
			return _tracer.SnapshotLogout(state.PersonId, Rta.LogOutBySnapshot);
		}

		public override void VerifyConfiguration(StrategyContext strategyContext)
		{
			if (_stateMapper.LoggedOutStateGroupIds().IsEmpty())
				strategyContext.AddException(new NoLoggedOutStateGroupsException("No Logged out StateGroup found"));
		}
	}

	public class ActivityChangesStrategy : ContextLoadingStrategy
	{
		private readonly IKeyValueStorePersister _keyValues;
		private readonly ScheduleCache _scheduleCache;
		private readonly ExternalLogonMapper _externalLogonMapper;
		private readonly IRtaTracer _tracer;

		public ActivityChangesStrategy(
			DateTime time,
			IConfigReader config,
			IAgentStatePersister persister,
			IKeyValueStorePersister keyValues,
			ScheduleCache scheduleCache,
			ExternalLogonMapper externalLogonMapper,
			IRtaTracer tracer) : base(config, persister, time)
		{
			_keyValues = keyValues;
			_scheduleCache = scheduleCache;
			_externalLogonMapper = externalLogonMapper;
			_tracer = tracer;
			ParallelTransactions = Config.ReadValue("RtaActivityChangesParallelTransactions", 7);
			MaxTransactionSize = Config.ReadValue("RtaActivityChangesMaxTransactionSize", 100);
			DeadLockVictim = DeadLockVictim.Yes;
		}

		public override IEnumerable<Guid> PersonIds(StrategyContext context)
		{
			_externalLogonMapper.Refresh();
			IEnumerable<PersonForCheck> persons = null;
			CurrentScheduleReadModelVersion version = null;
			context.WithReadModelUnitOfWork(() => { version = _keyValues.Get("CurrentScheduleReadModelVersion", null); });
			_scheduleCache.Refresh(version);
			context.WithUnitOfWork(() => { persons = Persister.FindForCheck(); });
			return persons
				.Where(x =>
				{
					var activities = _scheduleCache.Read(x.PersonId);
					var nextCheck = ScheduleInfo.NextCheck(activities, x.LastTimeWindowCheckSum, x.LastCheck, CurrentTime);
					return nextCheck <= CurrentTime;
				})
				.Select(x => x.PersonId)
				.ToArray();
		}

		public override InputInfo GetInputFor(AgentState state)
		{
			return null;
		}

		public override StateTraceLog GetTraceFor(AgentState state)
		{
			return _tracer.ActivityCheck(state.PersonId);
		}
	}
}