using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Config;
using Teleopti.Ccc.Domain.Logon.Aspects;
using Teleopti.Interfaces.Domain;
using AggregateException = System.AggregateException;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service
{
	public class ContextLoaderWithConnectionQueryOptimizeAllTheThings : ContextLoader
	{
		private readonly ICurrentDataSource _dataSource;
		private readonly IConfigReader _config;

		public ContextLoaderWithConnectionQueryOptimizeAllTheThings(ICurrentDataSource dataSource, IDatabaseLoader databaseLoader, INow now, StateMapper stateMapper, IAgentStatePersister agentStatePersister, IMappingReader mappingReader, IDatabaseReader databaseReader, IScheduleProjectionReadOnlyReader scheduleProjectionReadOnlyReader, AppliedAdherence appliedAdherence, ProperAlarm appliedAlarm, IConfigReader config) : base(databaseLoader, now, stateMapper, agentStatePersister, mappingReader, databaseReader, scheduleProjectionReadOnlyReader, appliedAdherence, appliedAlarm)
		{
			_dataSource = dataSource;
			_config = config;
		}

		public override void For(StateInputModel input, Action<Context> action)
		{
			var found = false;

			WithUnitOfWork(() =>
			{
				var mappings = new MappingsState(() => _mappingReader.Read());
				var dataSourceId = ValidateSourceId(input);
				var userCode = input.UserCode;
				var now = _now.UtcDateTime();

				_agentStatePersister.Find(dataSourceId, userCode)
					.ForEach(state =>
					{
						found = true;

						action.Invoke(new Context(
							now,
							new InputInfo
							{
								PlatformTypeId = input.PlatformTypeId,
								SourceId = input.SourceId,
								SnapshotId = input.SnapshotId,
								StateCode = input.StateCode,
								StateDescription = input.StateDescription
							},
							state.PersonId,
							state.BusinessUnitId,
							state.TeamId.GetValueOrDefault(),
							state.SiteId.GetValueOrDefault(),
							() => state,
							() => makeScheduleState(state, now),
							s => mappings,
							c => _agentStatePersister.Update(c.MakeAgentState()),
							_stateMapper,
							_appliedAdherence,
							_appliedAlarm
							));
					});
			});

			if (!found)
				throw new InvalidUserCodeException($"No person found for SourceId {input.SourceId} and UserCode {input.UserCode}");
		}

		public override void ForBatch(BatchInputModel batch, Action<Context> action)
		{
			process(new batchStrategy(_config, _agentStatePersister, _databaseLoader, action, batch));
		}

		private class batchStrategy : baseStrategy<BatchStateInputModel>
		{
			private readonly IDatabaseLoader _databaseLoader;
			private readonly BatchInputModel _batch;

			public batchStrategy(IConfigReader config, IAgentStatePersister persister, IDatabaseLoader databaseLoader, Action<Context> action, BatchInputModel batch) : base(config, persister, action)
			{
				_databaseLoader = databaseLoader;
				_batch = batch;
				ParallelTransactions = _config.ReadValue("RtaBatchParallelTransactions", 7);
				MaxTransactionSize = _config.ReadValue("RtaBatchMaxTransactionSize", 100);
			}

			public override IEnumerable<BatchStateInputModel> AllThings()
			{
				return _batch.States;
			}

			public override IEnumerable<AgentState> GetStatesFor(IEnumerable<BatchStateInputModel> states, Action<Exception> addException)
			{
				var dataSourceId = ValidateSourceId(_databaseLoader, _batch);
				var userCodes = states.Select(x => x.UserCode);
				var agentStates = _persister.Find(dataSourceId, userCodes);

				userCodes
					.Where(x => agentStates.All(s => s.UserCode != x))
					.Select(x => new InvalidUserCodeException($"No person found for UserCode {x}, DataSourceId {dataSourceId}, SourceId {_batch.SourceId}"))
					.ForEach(addException);

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

		public override void ForActivityChanges(Action<Context> action)
		{
			var personIds = WithUnitOfWork(() => _agentStatePersister.GetAllPersonIds());
			process(new activityChangesStrategy(_config, _agentStatePersister, action, personIds));
		}

		private class activityChangesStrategy : baseStrategy<Guid>
		{
			private readonly IEnumerable<Guid> _things;

			public activityChangesStrategy(IConfigReader config, IAgentStatePersister persister, Action<Context> action, IEnumerable<Guid> things) : base(config, persister, action)
			{
				_things = things;
				ParallelTransactions = _config.ReadValue("RtaActivityChangesParallelTransactions", 7);
				MaxTransactionSize = _config.ReadValue("RtaActivityChangesMaxTransactionSize", 100);
			}

			public override IEnumerable<Guid> AllThings()
			{
				return _things;
			}

			public override IEnumerable<AgentState> GetStatesFor(IEnumerable<Guid> ids, Action<Exception> addException)
			{
				return _persister.Get(ids);
			}

			public override InputInfo GetInputFor(AgentState state)
			{
				return null;
			}
		}

		public override void ForClosingSnapshot(DateTime snapshotId, string sourceId, Action<Context> action)
		{
			var personIds = WithUnitOfWork(() => _agentStatePersister.GetPersonIdsForLogout(snapshotId, sourceId, Rta.LogOutBySnapshot));
			process(new closingSnapshotStrategy(_config, _agentStatePersister, action, snapshotId, personIds));
		}

		private class closingSnapshotStrategy : baseStrategy<Guid>
		{
			private readonly DateTime _snapshotId;
			private readonly IEnumerable<Guid> _things;

			public closingSnapshotStrategy(IConfigReader config, IAgentStatePersister persister, Action<Context> action, DateTime snapshotId, IEnumerable<Guid> things) : base(config, persister, action)
			{
				_snapshotId = snapshotId;
				_things = things;
				ParallelTransactions = _config.ReadValue("RtaCloseSnapshotParallelTransactions", 3);
				MaxTransactionSize = _config.ReadValue("RtaCloseSnapshotMaxTransactionSize", 1000);
			}

			public override IEnumerable<Guid> AllThings()
			{
				return _things;
			}

			public override IEnumerable<AgentState> GetStatesFor(IEnumerable<Guid> ids, Action<Exception> addException)
			{
				return _persister.Get(ids);
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

		public override void ForSynchronize(Action<Context> action)
		{
			var personIds = WithUnitOfWork(() =>
				_agentStatePersister.GetStates()
					.Where(x => x.StateCode != null)
					.Select(x => x.PersonId)
					.ToArray()
				);
			process(new synchronizeStrategy(_config, _agentStatePersister, action, personIds));
		}

		private class synchronizeStrategy : baseStrategy<Guid>
		{
			private readonly IEnumerable<Guid> _things;

			public synchronizeStrategy(IConfigReader config, IAgentStatePersister persister, Action<Context> action, IEnumerable<Guid> things) : base(config, persister, action)
			{
				_things = things;
				ParallelTransactions = _config.ReadValue("RtaSynchronizeParallelTransactions", 1);
				MaxTransactionSize = _config.ReadValue("RtaSynchronizeMaxTransactionSize", 1000);
				UpdateAgentState = null;
			}

			public override IEnumerable<Guid> AllThings()
			{
				return _things;
			}

			public override IEnumerable<AgentState> GetStatesFor(IEnumerable<Guid> ids, Action<Exception> addException)
			{
				return _persister.Get(ids);
			}

			public override InputInfo GetInputFor(AgentState state)
			{
				return new InputInfo
				{
					StateCode = state.StateCode,
					PlatformTypeId = state.PlatformTypeId.ToString()
				};
			}

			public override Func<AgentState> GetStored(AgentState state)
			{
				return null;
			}
		}

		private abstract class baseStrategy<T> : IStrategy<T>
		{
			protected readonly IConfigReader _config;
			protected readonly IAgentStatePersister _persister;

			protected baseStrategy(
				IConfigReader config,
				IAgentStatePersister persister,
				Action<Context> action
				)
			{
				_config = config;
				_persister = persister;
				Action = action;
				UpdateAgentState = c => _persister.Update(c.MakeAgentState());
			}

			public int ParallelTransactions { get; protected set; }
			public int MaxTransactionSize { get; protected set; }

			public abstract IEnumerable<T> AllThings();
			public abstract IEnumerable<AgentState> GetStatesFor(IEnumerable<T> things, Action<Exception> addException);
			public abstract InputInfo GetInputFor(AgentState state);
			public virtual Func<AgentState> GetStored(AgentState state)
			{
				return () => state;
			}

			public Action<Context> UpdateAgentState { get; protected set; }

			public Action<Context> Action { get; }
		}

		public class scheduleData : ScheduleState
		{
			public Guid PersonId;

			public scheduleData(IEnumerable<ScheduledActivity> schedules, bool cacheSchedules, Guid personId) : base(schedules, cacheSchedules)
			{
				PersonId = personId;
			}
		}

		public class sharedData
		{
			public DateTime now;
			public MappingsState mappings;
		}

		public class transactionData
		{
			public IEnumerable<scheduleData> schedules;
			public IEnumerable<AgentState> agentStates;
			public Lazy<sharedData> shared;
		}

		public interface IStrategy<T>
		{
			int ParallelTransactions { get; }
			int MaxTransactionSize { get; }

			IEnumerable<T> AllThings();
			IEnumerable<AgentState> GetStatesFor(IEnumerable<T> things, Action<Exception> addException);
			InputInfo GetInputFor(AgentState state);

			Action<Context> UpdateAgentState { get; }

			Action<Context> Action { get; }

			Func<AgentState> GetStored(AgentState state);
		}

		private void process<T>(IStrategy<T> strategy)
		{
			var exceptions = new ConcurrentBag<Exception>();

			var allThings = strategy.AllThings();
			var allThingsSize = allThings.Count();
			if (allThingsSize == 0)
				return;
			var maxTransactionSize = strategy.MaxTransactionSize;
			if (allThingsSize <= maxTransactionSize * strategy.ParallelTransactions)
				maxTransactionSize = (int) Math.Ceiling(allThingsSize / (double)strategy.ParallelTransactions);

			var shared = new Lazy<sharedData>(() =>
			{
				var mappings = new MappingsState(() => _mappingReader.Read());
				mappings.Use();
				return new sharedData
				{
					now = _now.UtcDateTime(),
					mappings = mappings
				};
			});

			var transactions = allThings
				.Batch(maxTransactionSize)
				.Select(someThings =>
				{
					return new Lazy<transactionData>(() =>
					{
						var agentStates = strategy.GetStatesFor(someThings, exceptions.Add);
						var now = shared.Value.now;

						var schedules = agentStates
							.GroupBy(x => x.PersonId, (_, states) => states.First())
							.Where(x => scheduleIsValid(x, now))
							.Select(x => new scheduleData(x.Schedule, false, x.PersonId));
						var loadSchedulesFor = agentStates
							.Where(x => !scheduleIsValid(x, now))
							.Select(x => x.PersonId)
							.ToArray();
						if (loadSchedulesFor.Any())
						{
							var loaded = _scheduleProjectionReadOnlyReader.GetCurrentSchedules(now, loadSchedulesFor);
							var loadedSchedules = loadSchedulesFor
								.Select(x => new scheduleData(loaded.Where(l => l.PersonId == x).ToArray(), true, x));
							schedules = schedules.Concat(loadedSchedules);
						}
						schedules = schedules.ToArray();

						return new transactionData
						{
							agentStates = agentStates,
							shared = shared,
							schedules = schedules
						};
					});
				});

			var tenant = _dataSource.CurrentName();
			Parallel.ForEach(
				transactions,
				new ParallelOptions {MaxDegreeOfParallelism = strategy.ParallelTransactions},
				sharedData =>
					Transaction(tenant, strategy, sharedData)
						.ForEach(exceptions.Add)
				);

			if (exceptions.Any())
				throw new AggregateException(exceptions);
		}

		[TenantScope]
		protected virtual IEnumerable<Exception> Transaction<T>(
			string tenant,
			IStrategy<T> strategy,
			Lazy<transactionData> transactionData)
		{
			var exceptions = new List<Exception>();

			WithUnitOfWork(() =>
			{
				var data = transactionData.Value;
				var shared = data.shared.Value;

				data.agentStates.ForEach(state =>
				{
					try
					{
						strategy.Action.Invoke(new Context(
							shared.now,
							strategy.GetInputFor(state),
							state.PersonId,
							state.BusinessUnitId,
							state.TeamId.GetValueOrDefault(),
							state.SiteId.GetValueOrDefault(),
							strategy.GetStored(state),
							() => data.schedules.Single(s => s.PersonId == state.PersonId),
							s => shared.mappings,
							strategy.UpdateAgentState,
							_stateMapper,
							_appliedAdherence,
							_appliedAlarm
							));
					}
					catch (Exception e)
					{
						exceptions.Add(e);
					}
				});
			});

			return exceptions;
		}

		private ScheduleState makeScheduleState(AgentState state, DateTime now)
		{
			var cached = scheduleIsValid(state, now);
			return new ScheduleState(
				cached
					? state.Schedule
					: _scheduleProjectionReadOnlyReader.GetCurrentSchedule(now, state.PersonId),
				!cached);
		}

		private static bool scheduleIsValid(AgentState state, DateTime now)
		{
			if (state.Schedule == null)
				return false;
			if (now.Date != state.ReceivedTime.GetValueOrDefault().Date)
				return false;
			return true;
		}
	}
}