using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service
{
	public interface IContextLoader
	{
		void For(StateInputModel input, Action<Context> action);
		void ForBatch(BatchInputModel batch, Action<Context> action);
		void ForAll(Action<Context> action);
		void ForClosingSnapshot(DateTime snapshotId, string sourceId, Action<Context> action);
		void ForSynchronize(Action<Context> action);
	}

	public class ContextLoader : IContextLoader
	{
		private readonly IDatabaseLoader _databaseLoader;
		protected readonly INow _now;
		protected readonly StateMapper _stateMapper;
		protected readonly IAgentStatePersister _agentStatePersister;
		protected readonly IMappingReader _mappingReader;
		protected readonly IDatabaseReader _databaseReader;
		protected readonly IScheduleProjectionReadOnlyReader _scheduleProjectionReadOnlyReader;
		protected readonly AppliedAdherence _appliedAdherence;
		protected readonly ProperAlarm _appliedAlarm;

		public ContextLoader(
			IDatabaseLoader databaseLoader,
			INow now,
			StateMapper stateMapper,
			IAgentStatePersister agentStatePersister,
			IMappingReader mappingReader,
			IDatabaseReader databaseReader,
			IScheduleProjectionReadOnlyReader scheduleProjectionReadOnlyReader,
			AppliedAdherence appliedAdherence,
			ProperAlarm appliedAlarm
			)
		{
			_databaseLoader = databaseLoader;
			_now = now;
			_stateMapper = stateMapper;
			_agentStatePersister = agentStatePersister;
			_mappingReader = mappingReader;
			_databaseReader = databaseReader;
			_scheduleProjectionReadOnlyReader = scheduleProjectionReadOnlyReader;
			_appliedAdherence = appliedAdherence;
			_appliedAlarm = appliedAlarm;
		}

		protected int ValidateSourceId(IValidatable input)
		{
			if (string.IsNullOrEmpty(input.SourceId))
				throw new InvalidSourceException("Source id is required");
			int dataSourceId;
			if (!_databaseLoader.Datasources().TryGetValue(input.SourceId, out dataSourceId))
				throw new InvalidSourceException(string.Format("Source id not found {0}", input.SourceId));
			return dataSourceId;
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

		public virtual void For(StateInputModel input, Action<Context> action)
		{
			var found = false;

			WithUnitOfWork(() =>
			{
				var dataSourceId = ValidateSourceId(input);
				var userCode = input.UserCode;
				var now = _now.UtcDateTime();

				_databaseReader.LoadPersonOrganizationData(dataSourceId, userCode)
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
							state.TeamId,
							state.SiteId,
							() => _agentStatePersister.Get(state.PersonId),
							() => new ScheduleState(_scheduleProjectionReadOnlyReader.GetCurrentSchedule(now, state.PersonId), false),
							s =>
							{
								return new MappingsState(() =>
								{
									var stateCodes =
										new[] {s.Stored?.StateCode, s.Input.StateCode}
											.Distinct()
											.ToArray();
									var activities =
										new[] {s.Schedule.CurrentActivityId(), s.Schedule.PreviousActivityId(), s.Schedule.NextActivityId()}
											.Distinct()
											.ToArray();
									return _mappingReader.ReadFor(stateCodes, activities);
								});
							},
							c => _agentStatePersister.Update(c.MakeAgentState()),
							_stateMapper,
							_appliedAdherence,
							_appliedAlarm
							));
					});
			});

			if (!found)
				throw new InvalidUserCodeException(string.Format("No person found for SourceId {0} and UserCode {1}", input.SourceId, input.UserCode));

		}

		public virtual void ForBatch(BatchInputModel batch, Action<Context> action)
		{
			var exceptions = new List<Exception>();
			var inputs = from s in batch.States
				select new StateInputModel
				{
					AuthenticationKey = batch.AuthenticationKey,
					PlatformTypeId = batch.PlatformTypeId,
					SourceId = batch.SourceId,
					SnapshotId = batch.SnapshotId,
					UserCode = s.UserCode,
					StateCode = s.StateCode,
					StateDescription = s.StateDescription
				};
			inputs.ForEach(input =>
			{
				try
				{
					For(input, action);
				}
				catch (Exception e)
				{
					exceptions.Add(e);
				}
			});
			if (exceptions.Any())
				throw new AggregateException(exceptions);
		}

		public virtual void ForAll(Action<Context> action)
		{
			var mappings = new MappingsState(() => _mappingReader.Read());
			var now = _now.UtcDateTime();

			IEnumerable<PersonOrganizationData> persons = null;
			WithUnitOfWork(() =>
			{
				persons = _databaseReader.LoadAllPersonOrganizationData();
			});

			persons.ForEach(x =>
			{
				WithUnitOfWork(() =>
				{
					var state = _agentStatePersister.Get(x.PersonId);
					if (state == null)
						return;
					action.Invoke(new Context(
						now,
						null,
						x.PersonId,
						x.BusinessUnitId,
						x.TeamId,
						x.SiteId,
						() => state,
						() => new ScheduleState(_scheduleProjectionReadOnlyReader.GetCurrentSchedule(now, state.PersonId), false),
						s => mappings,
						c => _agentStatePersister.Update(c.MakeAgentState()),
						_stateMapper,
						_appliedAdherence,
						_appliedAlarm
						));
				});
			});
		}

		[AllBusinessUnitsUnitOfWork]
		[ReadModelUnitOfWork]
		public virtual void ForClosingSnapshot(DateTime snapshotId, string sourceId, Action<Context> action)
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
					() => new ScheduleState(_scheduleProjectionReadOnlyReader.GetCurrentSchedule(now, state.PersonId), false), 
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
		public virtual void ForSynchronize(Action<Context> action)
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
						() => new ScheduleState(_scheduleProjectionReadOnlyReader.GetCurrentSchedule(now, state.PersonId), false),
						s => mappings,
						null,
						_stateMapper,
						_appliedAdherence,
						_appliedAlarm
						));
				});
		}
	}

}