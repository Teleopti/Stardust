using Teleopti.Ccc.Domain.MultiTenancy;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service
{
	public class TenantLoader
	{
		private readonly IFindTenantNameByRtaKey _tenantNameByRtaKey;
		private readonly ICountTenants _countTenants;

		public TenantLoader(
			IFindTenantNameByRtaKey tenantNameByRtaKey,
			ICountTenants countTenants)
		{
			_tenantNameByRtaKey = tenantNameByRtaKey;
			_countTenants = countTenants;
		}

		[TenantUnitOfWork]
		[NoTenantAuthentication]
		public virtual string TenantNameByKey(string rtaKey)
		{
			return _tenantNameByRtaKey.Find(rtaKey);
		}

		[TenantUnitOfWork]
		[NoTenantAuthentication]
		public virtual bool Authenticate(string rtaKey)
		{
			if (_countTenants.Count() > 1 && rtaKey == ConfiguredKeyAuthenticator.InternalEncodingFixedLegacyAuthenticationKey)
				throw new LegacyAuthenticationKeyException(
					"Using the default authentication key with more than one tenant is not allowed");
			return _tenantNameByRtaKey.Find(rtaKey) != null;
		}
	}
}