using System;
using Teleopti.Ccc.Domain.MultiTenancy;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service
{
	public class TenantLoader
	{
		private readonly Lazy<IFindTenantNameByRtaKey> _tenantNameByRtaKey;
		private readonly Lazy<ICountTenants> _countTenants;

		public TenantLoader(
			Lazy<IFindTenantNameByRtaKey> tenantNameByRtaKey,
			Lazy<ICountTenants> countTenants)
		{
			_tenantNameByRtaKey = tenantNameByRtaKey;
			_countTenants = countTenants;
		}

		[TenantUnitOfWork]
		[NoTenantAuthentication]
		public virtual string TenantNameByKey(string rtaKey)
		{
			return _tenantNameByRtaKey.Value.Find(rtaKey);
		}

		[TenantUnitOfWork]
		[NoTenantAuthentication]
		public virtual bool Authenticate(string rtaKey)
		{
			if (_countTenants.Value.Count() > 1 && rtaKey == LegacyAuthenticationKey.MakeEncodingSafe(LegacyAuthenticationKey.TheKey))
				throw new LegacyAuthenticationKeyException(
					"Using the default authentication key with more than one tenant is not allowed");
			return _tenantNameByRtaKey.Value.Find(rtaKey) != null;
		}
	}
}