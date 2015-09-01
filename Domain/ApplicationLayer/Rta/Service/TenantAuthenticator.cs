using Teleopti.Ccc.Domain.MultiTenancy;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service
{
	public class TenantAuthenticator : IAuthenticator
	{
		private readonly IFindTenantNameByRtaKey _findTenantByRtaKey;
		private readonly ICountTenants _countTenants;

		public TenantAuthenticator(IFindTenantNameByRtaKey findTenantByRtaKey, ICountTenants countTenants)
		{
			_findTenantByRtaKey = findTenantByRtaKey;
			_countTenants = countTenants;
		}

		public bool Authenticate(string authenticationKey)
		{
			if (_countTenants.Count() > 1 && authenticationKey == Rta.LegacyAuthenticationKey)
				throw new LegacyAuthenticationKeyException("Using the default authentication key with more than one tenant is not allowed");
			return _findTenantByRtaKey.Find(authenticationKey) != null;
		}
	}
}