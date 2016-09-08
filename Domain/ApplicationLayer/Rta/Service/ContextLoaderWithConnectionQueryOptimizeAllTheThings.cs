using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Teleopti.Ccc.Domain.Aop;
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
							() => scheduleIsValid(state, now)
								? state.Schedule
								: _scheduleProjectionReadOnlyReader.GetCurrentSchedule(now, state.PersonId),
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
			var exceptions = new ConcurrentBag<Exception>();

			var batchData = new Lazy<batchData>(() =>
			{
				var now = _now.UtcDateTime();
				var dataSourceId = ValidateSourceId(batch);

				var userCodes = batch.States.Select(x => x.UserCode);
				var agentStates = _agentStatePersister.Find(dataSourceId, userCodes);

				userCodes
					.Where(x => agentStates.All(s => s.UserCode != x))
					.Select(x => new InvalidUserCodeException($"No person found for UserCode {x}, DataSourceId {dataSourceId}, SourceId {batch.SourceId}"))
					.ForEach(exceptions.Add);

				IEnumerable<ScheduledActivity> schedules = agentStates
					.Where(x => scheduleIsValid(x, now))
					.SelectMany(x => x.Schedule)
					.ToArray();
				var querySchedulesFor = agentStates
					.Where(x => !scheduleIsValid(x, now))
					.Select(x => x.PersonId)
					.ToArray();
				if (querySchedulesFor.Any())
				{
					schedules = schedules.Concat(
						_scheduleProjectionReadOnlyReader.GetCurrentSchedules(now, querySchedulesFor)
						).ToArray();
				}

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
				BatchTransaction(batch, action, batchData, states)
					.ForEach(exceptions.Add);
			});

			if (exceptions.Any())
				throw new AggregateException(exceptions);
		}

		public class batchData
		{
			public DateTime now;
			public IEnumerable<ScheduledActivity> schedules;
			public MappingsState mappings;
			public IEnumerable<AgentStateFound> agentStates;
		}

		[TenantScope]
		protected virtual IEnumerable<Exception> BatchTransaction(
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
									() => data.schedules.Where(s => s.PersonId == state.PersonId).ToArray(),
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

		public override void ForAll(Action<Context> action)
		{
			var mappings = new MappingsState(() => _mappingReader.Read());
			var now = _now.UtcDateTime();

			IEnumerable<Guid> allPersons = null;
			WithUnitOfWork(() =>
			{
				allPersons = _agentStatePersister.GetAllPersonIds();
			});

			var transactions = allPersons
				.Batch(_config.ReadValue("RtaAllBatchSize", 100))
				.Select(personIds =>
				{
					return new Lazy<allData>(() =>
					{
						var agentStates = _agentStatePersister.Get(personIds);
						IEnumerable<ScheduledActivity> schedules = agentStates
							.Where(x => scheduleIsValid(x, now))
							.SelectMany(x => x.Schedule)
							.ToArray();
						var querySchedulesFor = agentStates
							.Where(x => !scheduleIsValid(x, now))
							.Select(x => x.PersonId)
							.ToArray();
						if (querySchedulesFor.Any())
						{
							schedules = schedules.Concat(
								_scheduleProjectionReadOnlyReader.GetCurrentSchedules(now, querySchedulesFor)
								).ToArray();
						}
						mappings.Use(); // force load all here

						return new allData
						{
							now = now,
							agentStates = agentStates,
							mappings = mappings,
							schedules = schedules
						};
					});
				});

			Parallel.ForEach(
				transactions,
				new ParallelOptions {MaxDegreeOfParallelism = _config.ReadValue("RtaAllParallelTransactions", 7)},
				sharedData => ForAllSingleTransaction(_dataSource.CurrentName(), sharedData, action));
		}

		[TenantScope]
		protected virtual void ForAllSingleTransaction(
			string tenant,
			Lazy<allData> sharedData,
			Action<Context> action)
		{
			WithUnitOfWork(() =>
			{
				var data = sharedData.Value;

				data.agentStates.ForEach(state =>
				{
					action.Invoke(new Context(
						data.now,
						null,
						state.PersonId,
						state.BusinessUnitId,
						state.TeamId.GetValueOrDefault(),
						state.SiteId.GetValueOrDefault(),
						() => state,
						() => data.schedules.Where(s => s.PersonId == state.PersonId).ToArray(),
						s => data.mappings,
						c => _agentStatePersister.Update(c.MakeAgentState()),
						_stateMapper,
						_appliedAdherence,
						_appliedAlarm
						));
				});
			});
		}

		public class allData
		{
			public DateTime now;
			public IEnumerable<ScheduledActivity> schedules;
			public MappingsState mappings;
			public IEnumerable<AgentState> agentStates;
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
						() => scheduleIsValid(state, now)
							? state.Schedule
							: _scheduleProjectionReadOnlyReader.GetCurrentSchedule(now, state.PersonId),
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
						() => scheduleIsValid(state, now)
							? state.Schedule
							: _scheduleProjectionReadOnlyReader.GetCurrentSchedule(now, state.PersonId),
						s => mappings,
						null,
						_stateMapper,
						_appliedAdherence,
						_appliedAlarm
						));
				});
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