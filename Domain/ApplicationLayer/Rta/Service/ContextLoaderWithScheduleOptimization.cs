using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Config;
using Teleopti.Ccc.Domain.Logon.Aspects;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service
{
	public class ContextLoaderWithScheduleOptimization : ContextLoader
	{
		private readonly IScheduleCacheStrategy _scheduleCacheStrategy;
		private readonly IConfigReader _config;

		public class scheduleData : ScheduleState
		{
			public Guid PersonId;

			public scheduleData(IEnumerable<ScheduledActivity> schedules, bool cacheSchedules, Guid personId) : base(schedules, cacheSchedules)
			{
				PersonId = personId;
			}
		}

		public class batchData
		{
			public DateTime now;
			public IEnumerable<scheduleData> schedules;
			public MappingsState mappings;
			public IEnumerable<AgentStateFound> agentStates;
		}

		public ContextLoaderWithScheduleOptimization(IScheduleCacheStrategy scheduleCacheStrategy, IDatabaseLoader databaseLoader, INow now, StateMapper stateMapper, IAgentStatePersister agentStatePersister, IMappingReader mappingReader, IDatabaseReader databaseReader, IScheduleReader scheduleReader, AppliedAdherence appliedAdherence, ProperAlarm appliedAlarm, IConfigReader config) : base(databaseLoader, now, stateMapper, agentStatePersister, mappingReader, databaseReader, scheduleReader, appliedAdherence, appliedAlarm)
		{
			_scheduleCacheStrategy = scheduleCacheStrategy;
			_config = config;
		}

		public override void ForBatch(BatchInputModel batch, Action<Context> action)
		{
			var exceptions = new ConcurrentBag<Exception>();

			var batchData = new Lazy<batchData>(() =>
			{
				var now = _now.UtcDateTime();
				var dataSourceId = ValidateSourceId(batch);

				var userCodes = batch.States.Select(x => new ExternalLogon {DataSourceId = dataSourceId, UserCode = x.UserCode });
				var agentStates = _agentStatePersister.Find(userCodes, DeadLockVictim.No);

				userCodes
					.Where(x => agentStates.All(s => s.UserCode != x.UserCode))
					.Select(x => new InvalidUserCodeException($"No person found for UserCode {x}, DataSourceId {dataSourceId}, SourceId {batch.SourceId}"))
					.ForEach(exceptions.Add);

				var validated = agentStates
					.GroupBy(x => x.PersonId, (_, states) => states.First())
					.Select(x => new
					{
						state = x,
						valid = _scheduleCacheStrategy.ValidateCached(x, now)
					})
					.ToArray();
				var schedules = validated
					.Where(x => x.valid)
					.Select(x => new scheduleData(x.state.Schedule, false, x.state.PersonId));
				var loadSchedulesFor = validated
					.Where(x => !x.valid)
					.Select(x => x.state.PersonId)
					.ToArray();
				if (loadSchedulesFor.Any())
				{
					var loaded = ScheduleReader.GetCurrentSchedules(now, loadSchedulesFor);
					var loadedSchedules = loadSchedulesFor
						.Select(x => new scheduleData(_scheduleCacheStrategy.FilterSchedules(loaded.Where(l => l.PersonId == x), now), true, x));
					schedules = schedules.Concat(loadedSchedules);
				}
				schedules = schedules.ToArray();

				var mappings = new MappingsState(() => _mappingReader.Read());

				return new batchData
				{
					now = now,
					schedules = schedules,
					mappings = mappings,
					agentStates = agentStates
				};
			});

			// 7 transactions seems like a sweet spot when unit testing
			var transactionCount = (int) Math.Ceiling(batch.States.Count() / _config.ReadValue("RtaBatchTransactions", 7d));
			var transactions = batch.States.Batch(transactionCount);
			Parallel.ForEach(transactions, states =>
			{
				ForBatchSingle(batch, action, batchData, states)
					.ForEach(exceptions.Add);
			});

			if (exceptions.Any())
				throw new AggregateException(exceptions);
		}

		[TenantScope]
		protected virtual IEnumerable<Exception> ForBatchSingle(
			BatchInputModel batch,
			Action<Context> action,
			Lazy<batchData> batchData,
			IEnumerable<BatchStateInputModel> states)
		{
			var exceptions = new List<Exception>();

			WithUnitOfWork(() =>
			{
				var data = batchData.Value;

				states.ForEach(input =>
				{
					try
					{
						data.agentStates
							.Where(x => x.UserCode == input.UserCode)
							.ForEach(state =>
							{
								action.Invoke(new Context(
									data.now,
									new InputInfo
									{
										PlatformTypeId = batch.PlatformTypeId,
										SourceId = batch.SourceId,
										SnapshotId = batch.SnapshotId,
										StateCode = input.StateCode,
										StateDescription = input.StateDescription
									},
									state.PersonId,
									state.BusinessUnitId,
									state.TeamId.GetValueOrDefault(),
									state.SiteId.GetValueOrDefault(),
									() => state,
									() => data.schedules.Single(s => s.PersonId == state.PersonId),
									s => data.mappings,
									c => _agentStatePersister.Update(c.MakeAgentState()),
									_stateMapper,
									_appliedAdherence,
									_appliedAlarm
									));
							});
					}
					catch (Exception e)
					{
						exceptions.Add(e);
					}
				});
			});

			return exceptions;
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

				_agentStatePersister.Find(new ExternalLogon {DataSourceId = dataSourceId, UserCode = userCode}, DeadLockVictim.No)
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

		public override void ForActivityChanges(Action<Context> action)
		{
			var mappings = new MappingsState(() => _mappingReader.Read());
			var now = _now.UtcDateTime();

			IEnumerable<ExternalLogon> personIds = null;
			WithUnitOfWork(() =>
			{
				personIds = _agentStatePersister.FindAll();
			});

			personIds.ForEach(x =>
			{
				WithUnitOfWork(() =>
				{
					var states = _agentStatePersister.Find(x, DeadLockVictim.Yes);
					states.ForEach(state =>
					{
						action.Invoke(new Context(
							now,
							null,
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
			});
		}

		[AllBusinessUnitsUnitOfWork]
		[ReadModelUnitOfWork]
		public override void ForClosingSnapshot(DateTime snapshotId, string sourceId, Action<Context> action)
		{
			var stateCode = Rta.LogOutBySnapshot;
			var now = _now.UtcDateTime();

			var missingAgents = _agentStatePersister.GetStatesNotInSnapshot(snapshotId, sourceId);
			var agentsNotAlreadyLoggedOut =
				from a in missingAgents
				where a.StateCode != stateCode
				select a;

			var mappings = new MappingsState(() => _mappingReader.Read());

			agentsNotAlreadyLoggedOut.ForEach(state =>
			{
				action.Invoke(new Context(
					now,
					new InputInfo
					{
						StateCode = stateCode,
						PlatformTypeId = Guid.Empty.ToString(),
						SnapshotId = snapshotId
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

		}

		[AllBusinessUnitsUnitOfWork]
		[ReadModelUnitOfWork]
		public override void ForSynchronize(Action<Context> action)
		{
			var mappings = new MappingsState(() => _mappingReader.Read());
			var now = _now.UtcDateTime();

			_agentStatePersister.GetStates()
				.Where(x => x.StateCode != null)
				.ForEach(state =>
				{
					action.Invoke(new Context(
						now,
						new InputInfo
						{
							StateCode = state.StateCode,
							PlatformTypeId = state.PlatformTypeId.ToString()
						},
						state.PersonId,
						state.BusinessUnitId,
						state.TeamId.GetValueOrDefault(),
						state.SiteId.GetValueOrDefault(),
						null,
						() => makeScheduleState(state, now),
						s => mappings,
						null,
						_stateMapper,
						_appliedAdherence,
						_appliedAlarm
						));
				});
		}

		private ScheduleState makeScheduleState(AgentState state, DateTime now)
		{
			var cached = _scheduleCacheStrategy.ValidateCached(state, now);
			return new ScheduleState(
				cached
					? state.Schedule
					: _scheduleCacheStrategy.FilterSchedules(ScheduleReader.GetCurrentSchedule(now, state.PersonId), now),
				!cached);
		}
		
	}
}