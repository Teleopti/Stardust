using Teleopti.Ccc.Domain.MultiTenancy;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service
{
	public class TenantLoader : ITenantLoader
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

		public string TenantNameByKey(string rtaKey)
		{
			return _tenantNameByRtaKey.Find(rtaKey);
		}

		public bool AuthenticateKey(string rtaKey)
		{
			if (_countTenants.Count() > 1 && rtaKey == Rta.LegacyAuthenticationKey)
				throw new LegacyAuthenticationKeyException(
					"Using the default authentication key with more than one tenant is not allowed");
			return _tenantNameByRtaKey.Find(rtaKey) != null;
		}
	}
}