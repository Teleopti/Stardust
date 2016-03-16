using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.Resolvers;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service
{
	public interface IStateContextLoader
	{
		void For(ExternalUserStateInputModel input, Action<StateContext> action);
		void ForAll(Action<StateContext> action);
		void ForClosingSnapshot(ExternalUserStateInputModel input, Action<StateContext> action);
		void ForSynchronize(Action<StateContext> action);
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
		protected readonly IDatabaseLoader _databaseLoader;
		protected readonly INow _now;
		protected readonly IAgentStateReadModelUpdater _agentStateReadModelUpdater;
		private readonly IAgentStateReadModelPersister _agentStateReadModelPersister;
		private readonly StateMapper _stateMapper;
		protected readonly IPreviousStateInfoLoader _previousStateInfoLoader;
		protected readonly IMappingLoader MappingLoader;
		protected readonly WithAnalyticsUnitOfWork _unitOfWork;
		protected readonly PersonLocker _locker = new PersonLocker();

		public LoadFromCache(
			IDatabaseLoader databaseLoader,
			INow now,
			IAgentStateReadModelUpdater agentStateReadModelUpdater,
			IAgentStateReadModelPersister agentStateReadModelPersister,
			StateMapper stateMapper,
			IPreviousStateInfoLoader previousStateInfoLoader,
			IMappingLoader mappingLoader,
			WithAnalyticsUnitOfWork unitOfWork
			)
		{
			_dataSourceResolver = new DataSourceResolver(databaseLoader);
			_databaseLoader = databaseLoader;
			_now = now;
			_agentStateReadModelUpdater = agentStateReadModelUpdater;
			_agentStateReadModelPersister = agentStateReadModelPersister;
			_stateMapper = stateMapper;
			_previousStateInfoLoader = previousStateInfoLoader;
			MappingLoader = mappingLoader;
			_unitOfWork = unitOfWork;
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
				PersonOrganizationData x;
				if (!personOrganizationData.TryGetValue(p.PersonId, out x))
					continue;
				x.BusinessUnitId = p.BusinessUnitId;
				_locker.LockFor(x.PersonId, () =>
				{
					action.Invoke(new StateContext(
						input,
						x.PersonId,
						x.BusinessUnitId,
						x.TeamId,
						x.SiteId,
						() => _unitOfWork.Get(() => _previousStateInfoLoader.Load(x.PersonId)),
						() => _databaseLoader.GetCurrentSchedule(x.PersonId),
						() => MappingLoader.Load(),
						s => _unitOfWork.Do(() => _agentStateReadModelUpdater.Update(s)), 
						_now));
				});
			}

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
				PersonOrganizationData x;
				if (!personOrganizationData.TryGetValue(p.PersonId, out x))
					continue;
				x.BusinessUnitId = p.BusinessUnitId;
				_locker.LockFor(x.PersonId, () =>
				{
					action.Invoke(new StateContext(
						null,
						x.PersonId,
						x.BusinessUnitId,
						x.TeamId,
						x.SiteId,
						() => _unitOfWork.Get(() => _previousStateInfoLoader.Load(x.PersonId)),
						() => _databaseLoader.GetCurrentSchedule(x.PersonId),
						() => MappingLoader.Load(),
						s => _unitOfWork.Do(() => _agentStateReadModelUpdater.Update(s)),
						_now));
				});
			}
		}

		public virtual void ForClosingSnapshot(ExternalUserStateInputModel input, Action<StateContext> action)
		{
			var missingAgents = _agentStateReadModelPersister.GetNotInSnapshot(input.BatchId, input.SourceId);
			var agentsNotAlreadyLoggedOut =
				from a in missingAgents
				let state = _stateMapper.StateFor(
					MappingLoader.Load(),
					a.BusinessUnitId,
					a.PlatformTypeId,
					a.StateCode,
					null)
				where !state.IsLogOutState
				select a;

			agentsNotAlreadyLoggedOut.ForEach(x =>
			{
				_locker.LockFor(x.PersonId, () =>
				{
					action.Invoke(new StateContext(
						input,
						x.PersonId,
						x.BusinessUnitId,
						x.TeamId.GetValueOrDefault(),
						x.SiteId.GetValueOrDefault(),
						() => _unitOfWork.Get(() => _previousStateInfoLoader.Load(x.PersonId)),
						() => _databaseLoader.GetCurrentSchedule(x.PersonId),
						() => MappingLoader.Load(),
						s => _unitOfWork.Do(() => _agentStateReadModelUpdater.Update(s)),
						_now));
				});
			});

		}

		public virtual void ForSynchronize(Action<StateContext> action)
		{
			_unitOfWork.Get(() => _agentStateReadModelPersister.GetAll())
				.ForEach(x =>
				{
					_locker.LockFor(x.PersonId, () =>
					{
						action.Invoke(new StateContext(
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
							() => _databaseLoader.GetCurrentSchedule(x.PersonId),
							() => MappingLoader.Load(),
							null,
							_now));
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
			IAgentStateReadModelPersister agentStateReadModelPersister,
			StateMapper stateMapper, 
			IPreviousStateInfoLoader previousStateInfoLoader, 
			IMappingLoader mappingLoader, 
			IDatabaseReader databaseReader,
			WithAnalyticsUnitOfWork unitOfWork)
			: base(
				databaseLoader, 
				now, 
				agentStateReadModelUpdater, 
				agentStateReadModelPersister, 
				stateMapper, 
				previousStateInfoLoader,
				mappingLoader,
				unitOfWork)
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
							() => _unitOfWork.Get(() => _previousStateInfoLoader.Load(x.PersonId)),
							() => _databaseLoader.GetCurrentSchedule(x.PersonId),
							() => MappingLoader.Load(),
							s => _unitOfWork.Do(() => _agentStateReadModelUpdater.Update(s)),
							_now));
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
							() => _unitOfWork.Get(() => _previousStateInfoLoader.Load(x.PersonId)),
							() => _databaseLoader.GetCurrentSchedule(x.PersonId),
							() => MappingLoader.Load(),
							s => _unitOfWork.Do(() => _agentStateReadModelUpdater.Update(s)),
							_now));
					});
				});
		}
	}







	public class LoadAllFromDatabase : IStateContextLoader
	{
		private readonly DataSourceResolver _dataSourceResolver;
		protected readonly IDatabaseLoader _databaseLoader;
		protected readonly INow _now;
		protected readonly IAgentStateReadModelUpdater _agentStateReadModelUpdater;
		private readonly IAgentStateReadModelPersister _agentStateReadModelPersister;
		private readonly StateMapper _stateMapper;
		protected readonly IPreviousStateInfoLoader _previousStateInfoLoader;
		protected readonly IMappingLoader MappingLoader;
		private readonly IMappingReader _mappingReader;
		private readonly IDatabaseReader _databaseReader;
		private readonly WithAnalyticsUnitOfWork _unitOfWork;

		public LoadAllFromDatabase(
			IDatabaseLoader databaseLoader,
			INow now,
			IAgentStateReadModelUpdater agentStateReadModelUpdater,
			IAgentStateReadModelPersister agentStateReadModelPersister,
			StateMapper stateMapper,
			IPreviousStateInfoLoader previousStateInfoLoader,
			IMappingLoader mappingLoader,
			IMappingReader mappingReader,
			IDatabaseReader databaseReader,
			WithAnalyticsUnitOfWork unitOfWork
			)
		{
			_dataSourceResolver = new DataSourceResolver(databaseLoader);
			_databaseLoader = databaseLoader;
			_now = now;
			_agentStateReadModelUpdater = agentStateReadModelUpdater;
			_agentStateReadModelPersister = agentStateReadModelPersister;
			_stateMapper = stateMapper;
			_previousStateInfoLoader = previousStateInfoLoader;
			MappingLoader = mappingLoader;
			_mappingReader = mappingReader;
			_databaseReader = databaseReader;
			_unitOfWork = unitOfWork;
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

			_databaseReader.LoadPersonOrganizationData(dataSourceId, userCode)
				.ForEach(x =>
				{
					_unitOfWork.Do(() =>
					{
						action.Invoke(new StateContext(
							input,
							x.PersonId,
							x.BusinessUnitId,
							x.TeamId,
							x.SiteId,
							() => _previousStateInfoLoader.Load(x.PersonId),
							() => _databaseReader.GetCurrentSchedule(x.PersonId),
							() => _mappingReader.Read(),
							s => _agentStateReadModelUpdater.Update(s),
							_now));
					});
				});
		}

		public virtual void ForAll(Action<StateContext> action)
		{
			_databaseReader.LoadAllPersonOrganizationData()
				.ForEach(x =>
				{
					_unitOfWork.Do(() =>
					{
						action.Invoke(new StateContext(
							null,
							x.PersonId,
							x.BusinessUnitId,
							x.TeamId,
							x.SiteId,
							() => _previousStateInfoLoader.Load(x.PersonId),
							() => _databaseReader.GetCurrentSchedule(x.PersonId),
							() => _mappingReader.Read(),
							s => _agentStateReadModelUpdater.Update(s),
							_now));
					});
				});
		}

		[AnalyticsUnitOfWork]
		public virtual void ForClosingSnapshot(ExternalUserStateInputModel input, Action<StateContext> action)
		{
			var missingAgents = _agentStateReadModelPersister.GetNotInSnapshot(input.BatchId, input.SourceId);
			var agentsNotAlreadyLoggedOut =
				from a in missingAgents
				let state = _stateMapper.StateFor(
					MappingLoader.Load(),
					a.BusinessUnitId,
					a.PlatformTypeId,
					a.StateCode,
					null)
				where !state.IsLogOutState
				select a;

			agentsNotAlreadyLoggedOut.ForEach(x =>
			{
				action.Invoke(new StateContext(
					input,
					x.PersonId,
					x.BusinessUnitId,
					x.TeamId.GetValueOrDefault(),
					x.SiteId.GetValueOrDefault(),
					() => _previousStateInfoLoader.Load(x.PersonId),
					() => _databaseReader.GetCurrentSchedule(x.PersonId),
					() => _mappingReader.Read(),
					s => _agentStateReadModelUpdater.Update(s),
					_now));
			});
		}

		[AnalyticsUnitOfWork]
		public virtual void ForSynchronize(Action<StateContext> action)
		{
			_agentStateReadModelPersister.GetAll()
				.ForEach(x =>
				{
					action.Invoke(new StateContext(
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
						() => _mappingReader.Read(),
						null,
						_now));
				});
		}

	}

}