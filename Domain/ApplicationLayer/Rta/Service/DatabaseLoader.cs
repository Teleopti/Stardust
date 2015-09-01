using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.MultiTenancy;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service
{
	public class DatabaseLoader : IDatabaseLoader
	{
		private readonly IDatabaseReader _databaseReader;
		private readonly IFindTenantNameByRtaKey _tenantNameByRtaKey;
		private readonly ICountTenants _countTenants;

		public DatabaseLoader(
			IDatabaseReader databaseReader, 
			IFindTenantNameByRtaKey tenantNameByRtaKey,
			ICountTenants countTenants)
		{
			_databaseReader = databaseReader;
			_tenantNameByRtaKey = tenantNameByRtaKey;
			_countTenants = countTenants;
		}

		public IEnumerable<ScheduleLayer> GetCurrentSchedule(Guid personId)
		{
			return _databaseReader.GetCurrentSchedule(personId);
		}

		public ConcurrentDictionary<string, int> Datasources()
		{
			return _databaseReader.Datasources();
		}

		public IDictionary<Guid, PersonOrganizationData> PersonOrganizationData()
		{
			return _databaseReader.PersonOrganizationData().ToDictionary(data => data.PersonId);
		}

		public ConcurrentDictionary<string, IEnumerable<ResolvedPerson>> ExternalLogOns()
		{
			return _databaseReader.ExternalLogOns();
		}

		public string TenantNameByKey(string rtaKey)
		{
			return _tenantNameByRtaKey.Find(rtaKey);
		}

		public bool AuthenticateKey(string rtaKey)
		{
			if (_countTenants.Count() > 1 && rtaKey == Rta.LegacyAuthenticationKey)
				throw new LegacyAuthenticationKeyException("Using the default authentication key with more than one tenant is not allowed");
			return _tenantNameByRtaKey.Find(rtaKey) != null;
		}
	}
}