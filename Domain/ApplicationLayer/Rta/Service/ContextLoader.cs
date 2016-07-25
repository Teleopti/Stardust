using System;
using System.Collections.Generic;
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
		private readonly StateMapper _stateMapper;
		private readonly IAgentStatePersister _agentStatePersister;
		private readonly IMappingReader _mappingReader;
		private readonly IDatabaseReader _databaseReader;
		private readonly AppliedAdherence _appliedAdherence;
		private readonly ProperAlarm _appliedAlarm;

		public ContextLoader(
			IDatabaseLoader databaseLoader,
			INow now,
			StateMapper stateMapper,
			IAgentStatePersister agentStatePersister,
			IMappingReader mappingReader,
			IDatabaseReader databaseReader,
			AppliedAdherence appliedAdherence,
			ProperAlarm appliedAlarm
			)
		{
			_dataSourceResolver = new DataSourceResolver(databaseLoader);
			_now = now;
			_stateMapper = stateMapper;
			_agentStatePersister = agentStatePersister;
			_mappingReader = mappingReader;
			_databaseReader = databaseReader;
			_appliedAdherence = appliedAdherence;
			_appliedAlarm = appliedAlarm;
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

		[ReadModelUnitOfWork]
		protected virtual T WithReadModelUnitOfWork<T>(Func<T> action)
		{
			return action.Invoke();
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

			WithUnitOfWork(() =>
			{
				_databaseReader.LoadPersonOrganizationData(dataSourceId, userCode)
					.ForEach(x =>
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
									return WithReadModelUnitOfWork(() => _mappingReader.ReadFor(stateCodes, activities));
								});
							},
							c => _agentStatePersister.Persist(c.MakeAgentState()),
							_now,
							_stateMapper,
							_appliedAdherence,
							_appliedAlarm
							));
					});
			});
		}

		public virtual void ForAll(Action<Context> action)
		{
			var mappings = new MappingsState(() => _mappingReader.Read());

			IEnumerable<PersonOrganizationData> persons = null;
			WithUnitOfWork(() =>
			{
				persons = _databaseReader.LoadAllPersonOrganizationData();
			});

			persons.ForEach(x =>
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
						s => mappings,
						c => _agentStatePersister.Persist(c.MakeAgentState()),
						_now,
						_stateMapper,
						_appliedAdherence,
						_appliedAlarm
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
			var mappings = new MappingsState(() => _mappingReader.Read());

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
					s => mappings,
					c => _agentStatePersister.Persist(c.MakeAgentState()),
					_now,
					_stateMapper,
					_appliedAdherence,
					_appliedAlarm
					));
			});
		}

		[AllBusinessUnitsUnitOfWork]
		public virtual void ForSynchronize(Action<Context> action)
		{
			var mappings = new MappingsState(() => _mappingReader.Read());

			_agentStatePersister.GetAll()
				.Where(x => x.StateCode != null)
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
						s => mappings,
						null,
						_now,
						_stateMapper,
						_appliedAdherence,
						_appliedAlarm
						));
				});
		}

	}

}