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
			process(batch.States, (states, addException) =>
			{
				var dataSourceId = ValidateSourceId(batch);
				var userCodes = states.Select(x => x.UserCode);
				var agentStates = _agentStatePersister.Find(dataSourceId, userCodes);

				userCodes
                   .Where(x => agentStates.All(s => s.UserCode != x))
                   .Select(x => new InvalidUserCodeException($"No person found for UserCode {x}, DataSourceId {dataSourceId}, SourceId {batch.SourceId}"))
                   .ForEach(addException);

				return agentStates;
			}, state =>
			{
				var s = state as AgentStateFound;
				var input = batch.States.Single(x => x.UserCode == s.UserCode);
				return new InputInfo
				{
					PlatformTypeId = batch.PlatformTypeId,
					SnapshotId = batch.SnapshotId,
					SourceId = batch.SourceId,
					StateCode = input.StateCode,
					StateDescription = input.StateDescription,
				};
			}, action);
		}
		
		public override void ForAll(Action<Context> action)
		{
			IEnumerable<Guid> personIds = null;
			WithUnitOfWork(() =>
			{
				personIds = _agentStatePersister.GetAllPersonIds();
			});

			process(personIds, (ids, _) => _agentStatePersister.Get(ids), null, action);
		}

		public class sharedData
		{
			public DateTime now;
			public MappingsState mappings;
		}

		public class transactionData
		{
			public IEnumerable<ScheduledActivity> schedules;
			public IEnumerable<AgentState> agentStates;
			public Lazy<sharedData> shared;

			public Func<AgentState, InputInfo> getInputFor;

			public InputInfo inputFor(AgentState state)
			{
				return getInputFor?.Invoke(state);
			}
		}

		private void process<T>(
			IEnumerable<T> allThings,
			Func<IEnumerable<T>, Action<Exception>, IEnumerable<AgentState>> getAgentStates,
			Func<AgentState, InputInfo> getInputFor,
			Action<Context> action)
		{
			var exceptions = new ConcurrentBag<Exception>();

			var allThingsSize = allThings.Count();
			if (allThingsSize == 0)
				return;
			var someThingsSize = _config.ReadValue("RtaMaxTransactionSize", 100);
			var transactionCount = _config.ReadValue("RtaParallelTransactions", 7);
			if (allThingsSize < someThingsSize*transactionCount)
				someThingsSize = (int) Math.Ceiling(allThingsSize / (double) transactionCount);

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
				.Batch(someThingsSize)
				.Select(someThings =>
				{
					return new Lazy<transactionData>(() =>
					{
						var agentStates = getAgentStates(someThings, exceptions.Add);

						IEnumerable<ScheduledActivity> schedules = agentStates
							.Where(x => scheduleIsValid(x, shared.Value.now))
							.SelectMany(x => x.Schedule)
							.ToArray();
						var querySchedulesFor = agentStates
							.Where(x => !scheduleIsValid(x, shared.Value.now))
							.Select(x => x.PersonId)
							.ToArray();
						if (querySchedulesFor.Any())
						{
							schedules = schedules.Concat(
								_scheduleProjectionReadOnlyReader.GetCurrentSchedules(shared.Value.now, querySchedulesFor)
								).ToArray();
						}

						return new transactionData
						{
							agentStates = agentStates,
							shared = shared,
							schedules = schedules,
							getInputFor = getInputFor,
						};
					});
				});

			var tenant = _dataSource.CurrentName();
			Parallel.ForEach(
				transactions,
				new ParallelOptions {MaxDegreeOfParallelism = transactionCount},
				sharedData =>
					Transaction(tenant, sharedData, action)
						.ForEach(exceptions.Add)
				);

			if (exceptions.Any())
				throw new AggregateException(exceptions);
		}

		[TenantScope]
		protected virtual IEnumerable<Exception> Transaction(
			string tenant,
			Lazy<transactionData> sharedData,
			Action<Context> action)
		{
			var exceptions = new List<Exception>();

			WithUnitOfWork(() =>
			{
				var data = sharedData.Value;
				var shared = data.shared.Value;

				data.agentStates.ForEach(state =>
				{
					try
					{
						action.Invoke(new Context(
							shared.now,
							data.inputFor(state),
							state.PersonId,
							state.BusinessUnitId,
							state.TeamId.GetValueOrDefault(),
							state.SiteId.GetValueOrDefault(),
							() => state,
							() => data.schedules.Where(s => s.PersonId == state.PersonId).ToArray(),
							s => shared.mappings,
							c => _agentStatePersister.Update(c.MakeAgentState()),
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