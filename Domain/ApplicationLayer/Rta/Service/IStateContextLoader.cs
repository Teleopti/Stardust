using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.Resolvers;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service
{
	public interface IStateContextLoader
	{
		IEnumerable<StateContext> LoadFor(ExternalUserStateInputModel input);
		IEnumerable<StateContext> LoadAll();
	}

	public class LoadFromCache : IStateContextLoader
	{
		private readonly DataSourceResolver _dataSourceResolver;
		private readonly IDatabaseLoader _databaseLoader;
		private readonly INow _now;
		private readonly IAgentStateReadModelUpdater _agentStateReadModelUpdater;
		private readonly IPreviousStateInfoLoader _previousStateInfoLoader;

		public LoadFromCache(
			IDatabaseLoader databaseLoader,
			INow now,
			IAgentStateReadModelUpdater agentStateReadModelUpdater,
			IPreviousStateInfoLoader previousStateInfoLoader
			)
		{
			_dataSourceResolver = new DataSourceResolver(databaseLoader);
			_databaseLoader = databaseLoader;
			_now = now;
			_agentStateReadModelUpdater = agentStateReadModelUpdater;
			_previousStateInfoLoader = previousStateInfoLoader;
		}

		public IEnumerable<StateContext> LoadFor(ExternalUserStateInputModel input)
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
				return Enumerable.Empty<StateContext>();

			var result = new List<StateContext>();
			foreach (var p in resolvedPersons)
			{
				PersonOrganizationData person;
				if (!personOrganizationData.TryGetValue(p.PersonId, out person))
					continue;
				person.BusinessUnitId = p.BusinessUnitId;
				result.Add(
					new StateContext(
						input,
						person.PersonId,
						person.BusinessUnitId,
						person.TeamId,
						person.SiteId,
						_now,
						_agentStateReadModelUpdater,
						_previousStateInfoLoader
						)
					);
			}

			return result;
		}

		private int validateSourceId(ExternalUserStateInputModel input)
		{
			if (string.IsNullOrEmpty(input.SourceId))
				throw new InvalidSourceException("Source id is required");
			int dataSourceId;
			if (!_dataSourceResolver.TryResolveId(input.SourceId, out dataSourceId))
				throw new InvalidSourceException(string.Format("Source id not found {0}", input.SourceId));
			return dataSourceId;
		}

		public IEnumerable<StateContext> LoadAll()
		{
			var personOrganizationData = _databaseLoader.PersonOrganizationData();
			var externalLogons = _databaseLoader.ExternalLogOns();

			var persons =
				from e in externalLogons
				from p in e.Value
				select p;

			var result = new List<StateContext>();
			foreach (var p in persons)
			{
				PersonOrganizationData person;
				if (!personOrganizationData.TryGetValue(p.PersonId, out person))
					continue;
				person.BusinessUnitId = p.BusinessUnitId;
				result.Add(
					new StateContext(
						null,
						person.PersonId,
						person.BusinessUnitId,
						person.TeamId,
						person.SiteId,
						_now,
						_agentStateReadModelUpdater,
						_previousStateInfoLoader
						)
					);
			}

			return result;
		}

	}

	public class LoadPersonFromDatabase : IStateContextLoader
	{
		private readonly DataSourceResolver _dataSourceResolver;
		private readonly IDatabaseReader _databaseReader;
		private readonly INow _now;
		private readonly IAgentStateReadModelUpdater _agentStateReadModelUpdater;
		private readonly IPreviousStateInfoLoader _previousStateInfoLoader;

		public LoadPersonFromDatabase(
			IDatabaseLoader databaseLoader,
			IDatabaseReader databaseReader,
			INow now,
			IAgentStateReadModelUpdater agentStateReadModelUpdater,
			IPreviousStateInfoLoader previousStateInfoLoader
			)
		{
			_dataSourceResolver = new DataSourceResolver(databaseLoader);
			_databaseReader = databaseReader;
			_now = now;
			_agentStateReadModelUpdater = agentStateReadModelUpdater;
			_previousStateInfoLoader = previousStateInfoLoader;
		}

		public IEnumerable<StateContext> LoadFor(ExternalUserStateInputModel input)
		{
			var dataSourceId = validateSourceId(input);
			var userCode = input.UserCode;

			return _databaseReader.LoadPersonOrganizationData(dataSourceId, userCode)
				.Select(x => new StateContext(
					input,
					x.PersonId,
					x.BusinessUnitId,
					x.TeamId,
					x.SiteId,
					_now,
					_agentStateReadModelUpdater,
					_previousStateInfoLoader
					));
		}

		public IEnumerable<StateContext> LoadAll()
		{
			return _databaseReader.LoadAllPersonOrganizationData()
				.Select(x => new StateContext(
					null,
					x.PersonId,
					x.BusinessUnitId,
					x.TeamId,
					x.SiteId,
					_now,
					_agentStateReadModelUpdater,
					_previousStateInfoLoader
					));
		}

		private int validateSourceId(ExternalUserStateInputModel input)
		{
			if (string.IsNullOrEmpty(input.SourceId))
				throw new InvalidSourceException("Source id is required");
			int dataSourceId;
			if (!_dataSourceResolver.TryResolveId(input.SourceId, out dataSourceId))
				throw new InvalidSourceException(string.Format("Source id not found {0}", input.SourceId));
			return dataSourceId;
		}

	}

}