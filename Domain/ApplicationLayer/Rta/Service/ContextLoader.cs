using System;
using System.Linq;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.Resolvers;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Interfaces;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service
{
	public class ContextLoader
	{
		private readonly DataSourceResolver _dataSourceResolver;
		private readonly INow _now;
		private readonly IAgentStateReadModelUpdater _agentStateReadModelUpdater;
		private readonly StateMapper _stateMapper;
		private readonly IAgentStatePersister _agentStatePersister;
		private readonly IMappingReader _mappingReader;
		private readonly IDatabaseReader _databaseReader;
		private readonly AppliedAdherence _appliedAdherence;
		private readonly ProperAlarm _appliedAlarm;
		private readonly IJsonSerializer _jsonSerializer;

		public ContextLoader(
			IDatabaseLoader databaseLoader,
			INow now,
			IAgentStateReadModelUpdater agentStateReadModelUpdater,
			StateMapper stateMapper,
			IAgentStatePersister agentStatePersister,
			IMappingReader mappingReader,
			IDatabaseReader databaseReader,
			AppliedAdherence appliedAdherence,
			ProperAlarm appliedAlarm,
			IJsonSerializer jsonSerializer
			)
		{
			_dataSourceResolver = new DataSourceResolver(databaseLoader);
			_now = now;
			_agentStateReadModelUpdater = agentStateReadModelUpdater;
			_stateMapper = stateMapper;
			_agentStatePersister = agentStatePersister;
			_mappingReader = mappingReader;
			_databaseReader = databaseReader;
			_appliedAdherence = appliedAdherence;
			_appliedAlarm = appliedAlarm;
			_jsonSerializer = jsonSerializer;
		}

		protected int validateSourceId(ExternalUserStateInputModel input)
		{
			if (string.IsNullOrEmpty(input.SourceId))
				throw new InvalidSourceException("Source id is required");
			int dataSourceId;
			if (!_dataSourceResolver.TryResolveId(input.SourceId, out dataSourceId))
				throw new InvalidSourceException(string.Format("Source id not found {0}", input.SourceId));
			return dataSourceId;
		}

		[AllBusinessUnitsUnitOfWork]
		protected virtual void WithUnitOfWork(Action action)
		{
			action.Invoke();
		}

		public virtual void For(ExternalUserStateInputModel input, Action<Context> action)
		{
			var dataSourceId = validateSourceId(input);
			var userCode = input.UserCode;

			_databaseReader.LoadPersonOrganizationData(dataSourceId, userCode)
				.ForEach(x =>
				{
					WithUnitOfWork(() =>
					{
						action.Invoke(new Context(
							input,
							x.PersonId,
							x.BusinessUnitId,
							x.TeamId,
							x.SiteId,
							() => _agentStatePersister.Get(x.PersonId),
							() => _databaseReader.GetCurrentSchedule(x.PersonId),
							s =>
							{
								var stateCodes =
									new[] {s.Stored.StateCode(), s.Input.StateCode}
										.Distinct()
										.ToArray();
								var activities =
									new[] {s.Schedule.CurrentActivityId(), s.Schedule.PreviousActivityId(), s.Schedule.NextActivityId()}
										.Distinct()
										.ToArray();
								return _mappingReader.ReadFor(stateCodes, activities);
							},
							new UpdateStuff(_agentStateReadModelUpdater),
							_now,
							_stateMapper,
							_appliedAdherence,
							_appliedAlarm,
							_jsonSerializer
							));
					});
				});
		}

		public virtual void ForAll(Action<Context> action)
		{
			_databaseReader.LoadAllPersonOrganizationData()
				.ForEach(x =>
				{
					WithUnitOfWork(() =>
					{
						action.Invoke(new Context(
							null,
							x.PersonId,
							x.BusinessUnitId,
							x.TeamId,
							x.SiteId,
							() => _agentStatePersister.Get(x.PersonId),
							() => _databaseReader.GetCurrentSchedule(x.PersonId),
							s => _mappingReader.Read(),
							new UpdateStuff(_agentStateReadModelUpdater),
							_now,
							_stateMapper,
							_appliedAdherence,
							_appliedAlarm,
							_jsonSerializer
							));
					});
				});
		}

		[AllBusinessUnitsUnitOfWork]
		public virtual void ForClosingSnapshot(ExternalUserStateInputModel input, Action<Context> action)
		{
			var missingAgents = _agentStatePersister.GetNotInSnapshot(input.BatchId, input.SourceId);
			var agentsNotAlreadyLoggedOut =
				from a in missingAgents
				where a.StateCode != input.StateCode
				select a;

			agentsNotAlreadyLoggedOut.ForEach(x =>
			{
				action.Invoke(new Context(
					input,
					x.PersonId,
					x.BusinessUnitId,
					x.TeamId.GetValueOrDefault(),
					x.SiteId.GetValueOrDefault(),
					() => x,
					() => _databaseReader.GetCurrentSchedule(x.PersonId),
					s => _mappingReader.Read(),
					new UpdateStuff(_agentStateReadModelUpdater), 
					_now,
					_stateMapper,
					_appliedAdherence,
					_appliedAlarm,
					_jsonSerializer
					));
			});
		}

		[AllBusinessUnitsUnitOfWork]
		public virtual void ForSynchronize(Action<Context> action)
		{
			_agentStatePersister.GetAll()
				.ForEach(x =>
				{
					action.Invoke(new Context(
						new ExternalUserStateInputModel
						{
							StateCode = x.StateCode,
							PlatformTypeId = x.PlatformTypeId.ToString()
						},
						x.PersonId,
						x.BusinessUnitId,
						x.TeamId.GetValueOrDefault(),
						x.SiteId.GetValueOrDefault(),
						null,
						() => _databaseReader.GetCurrentSchedule(x.PersonId),
						s => _mappingReader.Read(),
						new DontUpdateStuff(), 
						_now,
						_stateMapper,
						_appliedAdherence,
						_appliedAlarm,
						_jsonSerializer
						));
				});
		}

	}

}