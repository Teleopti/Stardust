using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.Resolvers;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service
{
	public interface IStateContextLoader
	{
		void For(ExternalUserStateInputModel input, Action<StateContext> action);
		void For(IEnumerable<AgentStateReadModel> states, Action<StateContext> action);
		void ForAll(Action<StateContext> action);
		void ForNotInBatchOf(ExternalUserStateInputModel input, Action<StateContext> action);
	}

	public class PersonLocker
	{
		private readonly ConcurrentDictionary<Guid, object> personLocks = new ConcurrentDictionary<Guid, object>();

		public void LockFor(Guid personId, Action action)
		{
			lock (personLocks.GetOrAdd(personId, g => new object()))
			{
				action.Invoke();
			}
		}
	}

	public class LoadFromCache : IStateContextLoader
	{
		private readonly DataSourceResolver _dataSourceResolver;
		private readonly IDatabaseLoader _databaseLoader;
		protected readonly INow _now;
		protected readonly IAgentStateReadModelUpdater _agentStateReadModelUpdater;
		private readonly IAgentStateReadModelReader _agentStateReadModelReader;
		private readonly StateMapper _stateMapper;
		protected readonly IPreviousStateInfoLoader _previousStateInfoLoader;
		protected readonly PersonLocker _locker = new PersonLocker();

		public LoadFromCache(
			IDatabaseLoader databaseLoader,
			INow now,
			IAgentStateReadModelUpdater agentStateReadModelUpdater,
			IAgentStateReadModelReader agentStateReadModelReader,
			StateMapper stateMapper,
			IPreviousStateInfoLoader previousStateInfoLoader
			)
		{
			_dataSourceResolver = new DataSourceResolver(databaseLoader);
			_databaseLoader = databaseLoader;
			_now = now;
			_agentStateReadModelUpdater = agentStateReadModelUpdater;
			_agentStateReadModelReader = agentStateReadModelReader;
			_stateMapper = stateMapper;
			_previousStateInfoLoader = previousStateInfoLoader;
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

		public virtual void For(ExternalUserStateInputModel input, Action<StateContext> action)
		{
			var dataSourceId = validateSourceId(input);
			var userCode = input.UserCode;

			var lookupKey = string.Format(CultureInfo.InvariantCulture, "{0}|{1}", dataSourceId, userCode).ToUpperInvariant();
			if (string.IsNullOrEmpty(userCode))
				lookupKey = string.Empty;

			var personOrganizationData = _databaseLoader.PersonOrganizationData();
			var externalLogons = _databaseLoader.ExternalLogOns();

			IEnumerable<ResolvedPerson> resolvedPersons;
			if (!externalLogons.TryGetValue(lookupKey, out resolvedPersons))
				return;

			foreach (var p in resolvedPersons)
			{
				PersonOrganizationData person;
				if (!personOrganizationData.TryGetValue(p.PersonId, out person))
					continue;
				person.BusinessUnitId = p.BusinessUnitId;
				_locker.LockFor(person.PersonId, () =>
				{
					action.Invoke(new StateContext(
						input,
						person.PersonId,
						person.BusinessUnitId,
						person.TeamId,
						person.SiteId,
						() => _previousStateInfoLoader.Load(person.PersonId),
						_now,
						_agentStateReadModelUpdater
						));
				});
			}

		}

		public virtual void For(IEnumerable<AgentStateReadModel> states, Action<StateContext> action)
		{
			states.ForEach(s =>
			{
				_locker.LockFor(s.PersonId, () =>
				{
					action.Invoke(new StateContext(
						new ExternalUserStateInputModel
						{
							StateCode = s.StateCode,
							PlatformTypeId = s.PlatformTypeId.ToString()
						},
						s.PersonId,
						s.BusinessUnitId,
						s.TeamId.GetValueOrDefault(),
						s.SiteId.GetValueOrDefault(),
						null,
						_now,
						null
						));
				});
			});
		}

		public virtual void ForAll(Action<StateContext> action)
		{
			var personOrganizationData = _databaseLoader.PersonOrganizationData();
			var externalLogons = _databaseLoader.ExternalLogOns();

			var persons =
				from e in externalLogons
				from p in e.Value
				select p;

			foreach (var p in persons)
			{
				PersonOrganizationData person;
				if (!personOrganizationData.TryGetValue(p.PersonId, out person))
					continue;
				person.BusinessUnitId = p.BusinessUnitId;
				_locker.LockFor(person.PersonId, () =>
				{
					action.Invoke(new StateContext(
						null,
						person.PersonId,
						person.BusinessUnitId,
						person.TeamId,
						person.SiteId,
						() => _previousStateInfoLoader.Load(person.PersonId),
						_now,
						_agentStateReadModelUpdater
						));
				});
			}
		}

		public virtual void ForNotInBatchOf(ExternalUserStateInputModel input, Action<StateContext> action)
		{
			var missingAgents = _agentStateReadModelReader.GetMissingAgentStatesFromBatch(input.BatchId, input.SourceId);
			var agentsNotAlreadyLoggedOut = from a in missingAgents
				let state = _stateMapper.StateFor(
					a.BusinessUnitId,
					a.PlatformTypeId,
					a.StateCode,
					null)
				where !state.IsLogOutState
				select a;

			agentsNotAlreadyLoggedOut.ForEach(agent =>
			{
				_locker.LockFor(agent.PersonId, () =>
				{
					action.Invoke(new StateContext(
						input,
						agent.PersonId,
						agent.BusinessUnitId,
						agent.TeamId.GetValueOrDefault(),
						agent.SiteId.GetValueOrDefault(),
						() => _previousStateInfoLoader.Load(agent.PersonId),
						_now,
						_agentStateReadModelUpdater
						));
				});
			});

		}
	}

	public class LoadPersonFromDatabase : LoadFromCache
	{
		private readonly IDatabaseReader _databaseReader;

		public LoadPersonFromDatabase(
			IDatabaseLoader databaseLoader, 
			INow now, 
			IAgentStateReadModelUpdater agentStateReadModelUpdater, 
			IAgentStateReadModelReader agentStateReadModelReader, 
			StateMapper stateMapper, 
			IPreviousStateInfoLoader previousStateInfoLoader,
			IDatabaseReader databaseReader
			) : base(
				databaseLoader, 
				now, 
				agentStateReadModelUpdater, 
				agentStateReadModelReader, 
				stateMapper, 
				previousStateInfoLoader)
		{
			_databaseReader = databaseReader;
		}

		public override void For(ExternalUserStateInputModel input, Action<StateContext> action)
		{
			var dataSourceId = validateSourceId(input);
			var userCode = input.UserCode;

			_databaseReader.LoadPersonOrganizationData(dataSourceId, userCode)
				.ForEach(x =>
				{
					_locker.LockFor(x.PersonId, () =>
					{
						action.Invoke(new StateContext(
							input,
							x.PersonId,
							x.BusinessUnitId,
							x.TeamId,
							x.SiteId,
							() => _previousStateInfoLoader.Load(x.PersonId),
							_now,
							_agentStateReadModelUpdater
							));
					});
				});
		}
		
		public override void ForAll(Action<StateContext> action)
		{
			_databaseReader.LoadAllPersonOrganizationData()
				.ForEach(x =>
				{
					_locker.LockFor(x.PersonId, () =>
					{
						action.Invoke(new StateContext(
							null,
							x.PersonId,
							x.BusinessUnitId,
							x.TeamId,
							x.SiteId,
							() => _previousStateInfoLoader.Load(x.PersonId),
							_now,
							_agentStateReadModelUpdater
							));
					});
				});
		}
	}

}