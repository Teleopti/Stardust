using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service
{
	public interface IPersonLoader
	{
		IEnumerable<PersonOrganizationData> LoadPersonData(int dataSourceId, string userCode);
		IEnumerable<PersonOrganizationData> LoadAllPersonsData();
	}

	public class LoadByResolve : IPersonLoader
	{
		private readonly IDatabaseLoader _databaseLoader;

		public LoadByResolve(IDatabaseLoader databaseLoader)
		{
			_databaseLoader = databaseLoader;
		}

		public IEnumerable<PersonOrganizationData> LoadPersonData(int dataSourceId, string userCode)
		{
			var lookupKey = string.Format(CultureInfo.InvariantCulture, "{0}|{1}", dataSourceId, userCode).ToUpperInvariant();
			if (string.IsNullOrEmpty(userCode))
				lookupKey = string.Empty;

			var personOrganizationData = _databaseLoader.PersonOrganizationData();
			var externalLogons = _databaseLoader.ExternalLogOns();

			IEnumerable<ResolvedPerson> resolvedPersons;
			if (!externalLogons.TryGetValue(lookupKey, out resolvedPersons))
				return Enumerable.Empty<PersonOrganizationData>();

			var result = new List<PersonOrganizationData>();
			foreach (var p in resolvedPersons)
			{
				PersonOrganizationData person;
				if (!personOrganizationData.TryGetValue(p.PersonId, out person))
					continue;
				person.BusinessUnitId = p.BusinessUnitId;
				result.Add(person);
			}

			return result;
		}

		public IEnumerable<PersonOrganizationData> LoadAllPersonsData()
		{
			var personOrganizationData = _databaseLoader.PersonOrganizationData();
			var externalLogons = _databaseLoader.ExternalLogOns();

			var persons =
				from e in externalLogons
				from p in e.Value
				select p;

			var result = new List<PersonOrganizationData>();
			foreach (var p in persons)
			{
				PersonOrganizationData person;
				if (!personOrganizationData.TryGetValue(p.PersonId, out person))
					continue;
				person.BusinessUnitId = p.BusinessUnitId;
				result.Add(person);
			}

			return result;
		}
	}

	public class LoadFromDatabase : IPersonLoader
	{
		private readonly IDatabaseReader _databaseReader;

		public LoadFromDatabase(IDatabaseReader databaseReader)
		{
			_databaseReader = databaseReader;
		}

		public IEnumerable<PersonOrganizationData> LoadPersonData(int dataSourceId, string userCode)
		{
			return _databaseReader.LoadPersonOrganizationData(dataSourceId, userCode);
		}

		public IEnumerable<PersonOrganizationData> LoadAllPersonsData()
		{
			return _databaseReader.LoadAllPersonOrganizationData();
		}
	}

}