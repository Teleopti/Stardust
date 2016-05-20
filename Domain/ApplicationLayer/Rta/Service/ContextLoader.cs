using System;
using System.Linq;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.Resolvers;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.UnitOfWork;
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
		private readonly WithAnalyticsUnitOfWork _withAnalytics;
		private readonly WithUnitOfWork _withUnitOfWork;
		private readonly AppliedAdherence _appliedAdherence;
		private readonly ProperAlarm _appliedAlarm;

		public ContextLoader(
			IDatabaseLoader databaseLoader,
			INow now,
			IAgentStateReadModelUpdater agentStateReadModelUpdater,
			StateMapper stateMapper,
			IAgentStatePersister agentStatePersister,
			IMappingReader mappingReader,
			IDatabaseReader databaseReader,
			WithAnalyticsUnitOfWork withAnalytics,
			WithUnitOfWork withUnitOfWork,
			AppliedAdherence appliedAdherence,
			ProperAlarm appliedAlarm
			)
		{
			_dataSourceResolver = new DataSourceResolver(databaseLoader);
			_now = now;
			_agentStateReadModelUpdater = agentStateReadModelUpdater;
			_stateMapper = stateMapper;
			_agentStatePersister = agentStatePersister;
			_mappingReader = mappingReader;
			_databaseReader = databaseReader;
			_withAnalytics = withAnalytics;
			_withUnitOfWork = withUnitOfWork;
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

		public virtual void For(ExternalUserStateInputModel input, Action<Context> action)
		{
			var dataSourceId = validateSourceId(input);
			var userCode = input.UserCode;

			_databaseReader.LoadPersonOrganizationData(dataSourceId, userCode)
				.ForEach(x =>
				{
					_withAnalytics.Do(() =>
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
								return _withUnitOfWork.Get(() => _mappingReader.ReadFor(stateCodes, activities));
							},
							s => _agentStateReadModelUpdater.Update(s),
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
			_databaseReader.LoadAllPersonOrganizationData()
				.ForEach(x =>
				{
					_withAnalytics.Do(() =>
					{
						action.Invoke(new Context(
							null,
							x.PersonId,
							x.BusinessUnitId,
							x.TeamId,
							x.SiteId,
							() => _agentStatePersister.Get(x.PersonId),
							() => _databaseReader.GetCurrentSchedule(x.PersonId),
							s => _withUnitOfWork.Get(() => _mappingReader.Read()),
							s => _agentStateReadModelUpdater.Update(s),
							_now,
							_stateMapper,
							_appliedAdherence,
							_appliedAlarm
							));
					});
				});
		}

		[AnalyticsUnitOfWork]
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
					s => _withUnitOfWork.Get(() => _mappingReader.Read()),
					s => _agentStateReadModelUpdater.Update(s),
					_now,
					_stateMapper,
					_appliedAdherence,
					_appliedAlarm
					));
			});
		}

		[AnalyticsUnitOfWork]
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
						s => _withUnitOfWork.Get(() => _mappingReader.Read()),
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