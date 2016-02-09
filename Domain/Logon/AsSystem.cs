using System;
using System.Collections.Concurrent;
using System.Globalization;
using System.IdentityModel.Claims;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Domain.Logon
{
	public class AsSystem
	{
		private readonly ILogOnOff _logOnOff;
		private readonly IRepositoryFactory _repositoryFactory;
		private readonly ClaimSetForApplicationRole _claimSetForApplicationRole;
		private readonly IDataSourceForTenant _dataSourceForTenant;
		private readonly ICurrentTeleoptiPrincipal _principal;
		private readonly claimCache _claimCache;

		public AsSystem(
			ILogOnOff logOnOff,
			IRepositoryFactory repositoryFactory,
			ClaimSetForApplicationRole claimSetForApplicationRole,
			IDataSourceForTenant dataSourceForTenant,
			ITime time,
			ICurrentTeleoptiPrincipal principal
			)
		{
			_logOnOff = logOnOff;
			_repositoryFactory = repositoryFactory;
			_claimSetForApplicationRole = claimSetForApplicationRole;
			_dataSourceForTenant = dataSourceForTenant;
			_principal = principal;
			_claimCache = new claimCache(time);
		}

		public void Logon(string tenant, Guid businessUnitId)
		{
			Logon(_dataSourceForTenant.Tenant(tenant), businessUnitId);
		}
		
		public void Logon(IDataSource dataSource, Guid businessUnitId)
		{
			IPerson systemUser;
			using (var uow = dataSource.Application.CreateAndOpenUnitOfWork())
			{
				systemUser = _repositoryFactory.CreatePersonRepository(uow).LoadPersonAndPermissions(SystemUser.Id_AvoidUsing_This);
			}

			using (var unitOfWork = dataSource.Application.CreateAndOpenUnitOfWork())
			{
				var businessUnit = _repositoryFactory.CreateBusinessUnitRepository(unitOfWork).Get(businessUnitId);
				unitOfWork.Remove(businessUnit); //To make sure that business unit doesn't belong to this uow any more
				
				_logOnOff.LogOn(dataSource, systemUser, businessUnit);

				setCorrectPermissionsOnUser(dataSource.Application);
			}
		}

		private void setCorrectPermissionsOnUser(IUnitOfWorkFactory unitOfWorkFactory)
		{
			var person = _principal.Current().GetPerson(_repositoryFactory.CreatePersonRepository(unitOfWorkFactory.CurrentUnitOfWork()));
			foreach (var applicationRole in person.PermissionInformation.ApplicationRoleCollection)
			{
				var cachedClaim = _claimCache.Get(unitOfWorkFactory.Name, applicationRole.Id.GetValueOrDefault());
				if (cachedClaim == null)
				{
					cachedClaim = _claimSetForApplicationRole.Transform(applicationRole, unitOfWorkFactory.Name);
					_claimCache.Add(cachedClaim, unitOfWorkFactory.Name, applicationRole.Id.GetValueOrDefault());
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