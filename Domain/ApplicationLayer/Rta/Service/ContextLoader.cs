using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
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
	public class ContextLoaderWithFasterActivityCheck : ContextLoader
	{
		public ContextLoaderWithFasterActivityCheck(IScheduleCacheStrategy scheduleCacheStrategy, ICurrentDataSource dataSource, IDatabaseLoader databaseLoader, INow now, StateMapper stateMapper, IAgentStatePersister agentStatePersister, IMappingReader mappingReader, IScheduleReader scheduleReader, AppliedAdherence appliedAdherence, ProperAlarm appliedAlarm, IConfigReader config) : base(scheduleCacheStrategy, dataSource, databaseLoader, now, stateMapper, agentStatePersister, mappingReader, scheduleReader, appliedAdherence, appliedAlarm, config)
		{
		}

		public override void ForActivityChanges(Action<Context> action)
		{
			var time = _now.UtcDateTime();
			var logons = WithUnitOfWork(() => _agentStatePersister.FindForCheck())
				.Where(x => x.NextCheck == null || x.NextCheck <= time)
				.ToArray();
			process(new activityChangesStrategy(_config, _agentStatePersister, action, logons, time));
		}
	}

	public class ContextLoader : IContextLoader
	{
		private readonly IScheduleCacheStrategy _scheduleCacheStrategy;
		private readonly ICurrentDataSource _dataSource;
		private readonly IDatabaseLoader _databaseLoader;
		protected readonly INow _now;
		private readonly StateMapper _stateMapper;
		protected readonly IAgentStatePersister _agentStatePersister;
		private readonly IMappingReader _mappingReader;
		private readonly IScheduleReader _scheduleReader;
		private readonly AppliedAdherence _appliedAdherence;
		private readonly ProperAlarm _appliedAlarm;
		protected readonly IConfigReader _config;

		public ContextLoader(
			IScheduleCacheStrategy scheduleCacheStrategy, 
			ICurrentDataSource dataSource, 
			IDatabaseLoader databaseLoader, 
			INow now, 
			StateMapper stateMapper, 
			IAgentStatePersister agentStatePersister, 
			IMappingReader mappingReader, 
			IScheduleReader scheduleReader, 
			AppliedAdherence appliedAdherence, 
			ProperAlarm appliedAlarm, 
			IConfigReader config)
		{
			_scheduleCacheStrategy = scheduleCacheStrategy;
			_dataSource = dataSource;
			_databaseLoader = databaseLoader;
			_now = now;
			_stateMapper = stateMapper;
			_agentStatePersister = agentStatePersister;
			_mappingReader = mappingReader;
			_scheduleReader = scheduleReader;
			_appliedAdherence = appliedAdherence;
			_appliedAlarm = appliedAlarm;
			_config = config;
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

		public virtual void ForActivityChanges(Action<Context> action)
		{
			var logons = WithUnitOfWork(() => _agentStatePersister.FindAll());
			process(new activityChangesStrategy(_config, _agentStatePersister, action, logons, _now.UtcDateTime()));
		}

		public void ForSynchronize(Action<Context> action)
		{
			var personIds = WithUnitOfWork(() =>
				_agentStatePersister.GetStates()
					.Where(x => x.StateCode != null)
					.Select(x => x.PersonId)
					.ToArray()
				);
			process(new synchronizeStrategy(_config, _agentStatePersister, action, personIds, _now.UtcDateTime()));
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
				throw new InvalidSourceException($"Source id not found {input.SourceId}");
			return dataSourceId;
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

			public override IEnumerable<StateInputModel> AllThings()
			{
				return new[] { _model };
			}

			public override IEnumerable<AgentState> GetStatesFor(IEnumerable<StateInputModel> things, Action<Exception> addException)
			{
				try
				{
					var dataSourceId = ValidateSourceId(_databaseLoader, _model);

					var userCode = _model.UserCode;
					var agentStates = _persister.Find(new ExternalLogon {DataSourceId = dataSourceId, UserCode = userCode}, DeadLockVictim.No);
					if (agentStates.IsEmpty())
						throw new InvalidUserCodeException($"No person found for SourceId {_model.SourceId} and UserCode {_model.UserCode}");

					return agentStates;
				}
				catch (Exception e)
				{
					addException(e);
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
			}

			public override IEnumerable<BatchStateInputModel> AllThings()
			{
				return _batch.States.OrderBy(x => x.UserCode).ToArray();
			}

			public override IEnumerable<AgentState> GetStatesFor(IEnumerable<BatchStateInputModel> states, Action<Exception> addException)
			{
				var dataSourceId = ValidateSourceId(_databaseLoader, _batch);
				var userCodes = states.Select(x => new ExternalLogon
				{
					DataSourceId = dataSourceId,
					UserCode = x.UserCode
				});
				var agentStates = _persister.Find(userCodes, DeadLockVictim.No);

				userCodes
					.Where(x => agentStates.All(s => s.UserCode != x.UserCode))
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

		protected class activityChangesStrategy : baseStrategy<ExternalLogon>
		{
			private readonly IEnumerable<ExternalLogon> _things;

			public activityChangesStrategy(IConfigReader config, IAgentStatePersister persister, Action<Context> action, IEnumerable<ExternalLogon> things, DateTime time) : base(config, persister, action, time)
			{
				_things = things;
				ParallelTransactions = _config.ReadValue("RtaActivityChangesParallelTransactions", 7);
				MaxTransactionSize = _config.ReadValue("RtaActivityChangesMaxTransactionSize", 100);
			}

			public override IEnumerable<ExternalLogon> AllThings()
			{
				return _things.OrderBy(x => x.UserCode).ToArray();
			}

			public override IEnumerable<AgentState> GetStatesFor(IEnumerable<ExternalLogon> ids, Action<Exception> addException)
			{
				return _persister.Find(ids, DeadLockVictim.Yes);
			}

			public override InputInfo GetInputFor(AgentState state)
			{
				return null;
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

			public override IEnumerable<ExternalLogon> AllThings()
			{
				return _things.OrderBy(x => x.UserCode).ToArray();
			}

			public override IEnumerable<AgentState> GetStatesFor(IEnumerable<ExternalLogon> ids, Action<Exception> addException)
			{
				return _persister.Find(ids, DeadLockVictim.No);
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

		private class synchronizeStrategy : baseStrategy<Guid>
		{
			private readonly IEnumerable<Guid> _things;

			public synchronizeStrategy(IConfigReader config, IAgentStatePersister persister, Action<Context> action, IEnumerable<Guid> things, DateTime time) : base(config, persister, action, time)
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
				UpdateAgentState = c => _persister.Update(c.MakeAgentState(), c.CacheSchedules);
			}

			public DateTime CurrentTime { get; }
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

			public scheduleData(IEnumerable<ScheduledActivity> schedules, bool newSchedules, Guid personId) : base(schedules, newSchedules)
			{
				PersonId = personId;
			}
		}

		public class sharedData
		{
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
			DateTime CurrentTime { get; }

			int ParallelTransactions { get; }
			int MaxTransactionSize { get; }

			IEnumerable<T> AllThings();
			IEnumerable<AgentState> GetStatesFor(IEnumerable<T> things, Action<Exception> addException);
			InputInfo GetInputFor(AgentState state);

			Action<Context> UpdateAgentState { get; }

			Action<Context> Action { get; }

			Func<AgentState> GetStored(AgentState state);
		}

		protected void process<T>(IStrategy<T> strategy)
		{
			var exceptions = new ConcurrentBag<Exception>();

			var allThings = strategy.AllThings();
			var allThingsSize = allThings.Count();
			if (allThingsSize == 0)
				return;
			var maxTransactionSize = strategy.MaxTransactionSize;
			if (allThingsSize <= maxTransactionSize * strategy.ParallelTransactions)
				maxTransactionSize = (int) Math.Ceiling(allThingsSize / (double) strategy.ParallelTransactions);

			var shared = new Lazy<sharedData>(() =>
			{
				var mappings = new MappingsState(() => _mappingReader.Read());
				mappings.Use();
				return new sharedData
				{
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

						var validated = agentStates
							.GroupBy(x => x.PersonId, (_, states) => states.First())
							.Select(x => new
							{
								state = x,
								valid = _scheduleCacheStrategy.ValidateCached(x, strategy.CurrentTime)
							})
							.ToArray();
						var schedules = validated
							.Where(x => x.valid)
							.Select(x => new scheduleData(x.state.Schedule, false, x.state.PersonId));
						var loadSchedulesFor = validated
							.Where(x => !x.valid)
							.Select(x => new PersonBusinessUnit {BusinessUnitId = x.state.BusinessUnitId, PersonId = x.state.PersonId})
							.ToArray();
						if (loadSchedulesFor.Any())
						{
							var loaded = _scheduleReader.GetCurrentSchedules(strategy.CurrentTime, loadSchedulesFor);
							var loadedSchedules = loadSchedulesFor
								.Select(x => new scheduleData(_scheduleCacheStrategy.FilterSchedules(loaded.Where(l => l.PersonId == x.PersonId), strategy.CurrentTime), true, x.PersonId));
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
				}).ToArray();

			var tenant = _dataSource.CurrentName();
			Parallel.ForEach(
				transactions,
				new ParallelOptions {MaxDegreeOfParallelism = strategy.ParallelTransactions},
				sharedData =>
				{
					// make retry and warn logging an aspect?
					var attempt = 1;
					while (true)
					{
						try
						{
							Transaction(tenant, strategy, sharedData);
							break;
						}
						catch (DeadLockVictimException e)
						{
							attempt++;
							if (attempt > 3)
							{
								exceptions.Add(e);
								break;
							}
							LogManager.GetLogger(typeof(ContextLoader))
								.Warn($"Transaction deadlocked, running attempt {attempt}.", e);
						}
						catch (Exception e)
						{
							exceptions.Add(e);
							break;
						}
					}
				}
			);

			if (exceptions.Count == 1)
			{
				var e = exceptions.First();
				PreserveStack.For(e);
				throw e;
			}
			if (exceptions.Any())
				throw new System.AggregateException(exceptions);
		}

		[TenantScope]
		[LogInfo]
		protected virtual void Transaction<T>(
			string tenant,
			IStrategy<T> strategy,
			Lazy<transactionData> transactionData)
		{
			WithUnitOfWork(() =>
			{
				var data = transactionData.Value;
				var shared = data.shared.Value;

				data.agentStates.ForEach(state =>
				{
					strategy.Action.Invoke(new Context(
						strategy.CurrentTime,
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
				});
			});

		}

	}
}