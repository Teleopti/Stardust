using System;
using System.Collections.Concurrent;
using System.Globalization;
using System.IdentityModel.Claims;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Logon
{
	public class AsSystem
	{
		private readonly ILogOnOff _logOnOff;
		private readonly IPersonRepository _personRepository;
		private readonly IBusinessUnitRepository _businessUnitRepository;
		private readonly ClaimSetForApplicationRole _claimSetForApplicationRole;
		private readonly IDataSourceForTenant _dataSourceForTenant;
		private readonly ICurrentTeleoptiPrincipal _principal;
		private readonly claimCache _claimCache;

		public AsSystem(
			ILogOnOff logOnOff,
			IPersonRepository personRepository,
			IBusinessUnitRepository businessUnitRepository,
			ClaimSetForApplicationRole claimSetForApplicationRole,
			IDataSourceForTenant dataSourceForTenant,
			ITime time,
			ICurrentTeleoptiPrincipal principal
			)
		{
			_logOnOff = logOnOff;
			_personRepository = personRepository;
			_businessUnitRepository = businessUnitRepository;
			_claimSetForApplicationRole = claimSetForApplicationRole;
			_dataSourceForTenant = dataSourceForTenant;
			_principal = principal;
			_claimCache = new claimCache(time);
		}

		public void Logon(string tenant, Guid businessUnitId)
		{
			var dataSource = _dataSourceForTenant.Tenant(tenant);
			using (dataSource.Application.CreateAndOpenUnitOfWork())
			{
				var systemUser = _personRepository.LoadPersonAndPermissions(SystemUser.Id);
				var businessUnit = _businessUnitRepository.Get(businessUnitId);
				_logOnOff.LogOn(dataSource, systemUser, businessUnit);
				setCorrectPermissionsOnUser(systemUser, dataSource.DataSourceName);
			}
		}

		private void setCorrectPermissionsOnUser(IPerson person, string dataSourceName)
		{
			foreach (var applicationRole in person.PermissionInformation.ApplicationRoleCollection)
			{
				var cachedClaim = _claimCache.Get(dataSourceName, applicationRole.Id.GetValueOrDefault());
				if (cachedClaim == null)
				{
					cachedClaim = _claimSetForApplicationRole.Transform(applicationRole, dataSourceName);
					_claimCache.Add(cachedClaim, dataSourceName, applicationRole.Id.GetValueOrDefault());
				}
				_principal.Current().AddClaimSet(cachedClaim);
			}
		}

		private sealed class claimCache : IDisposable
		{
			private readonly ConcurrentDictionary<string, ClaimSet> _cache = new ConcurrentDictionary<string, ClaimSet>();
			private readonly IDisposable _timer;

			public claimCache(ITime time)
			{
				_timer = time.StartTimer(emptyCache, null, TimeSpan.FromMilliseconds(-1), TimeSpan.FromMinutes(30));
			}

			private void emptyCache(object state)
			{
				_cache.Clear();
			}

			public void Add(ClaimSet claimSet, string dataSourceName, Guid roleId)
			{
				var key = getKey(dataSourceName, roleId);
				_cache.AddOrUpdate(key, claimSet, (s, set) => claimSet);
			}

			private static string getKey(string dataSourceName, Guid roleId)
			{
				var key = string.Format(CultureInfo.InvariantCulture, "{0}-{1}", dataSourceName, roleId);
				return key;
			}

			public ClaimSet Get(string dataSourceName, Guid roleId)
			{
				var key = getKey(dataSourceName, roleId);
				ClaimSet foundClaimSet;
				return (_cache.TryGetValue(key, out foundClaimSet)) ? foundClaimSet : null;
			}

			public void Dispose()
			{
				Dispose(true);
				GC.SuppressFinalize(this);
			}

			private void Dispose(bool disposing)
			{
				if (disposing)
				{
					if (_timer != null)
						_timer.Dispose();
					_cache.Clear();
				}
			}
		}

	}
}