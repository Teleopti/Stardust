using Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server.Queries;

namespace Teleopti.Ccc.Infrastructure.MultiTenancy.Server
{
	public class FindTenantForRta : IFindTenantForRta
	{
		private readonly IFindTenantByRtaKey _findTenantByRtaKey;
		private readonly ICountTenants _countTenants;

		public FindTenantForRta(IFindTenantByRtaKey findTenantByRtaKey, ICountTenants countTenants)
		{
			_findTenantByRtaKey = findTenantByRtaKey;
			_countTenants = countTenants;
		}

		public string Find(string rtaKey)
		{
			if (_countTenants.Count() > 1 && rtaKey == Domain.ApplicationLayer.Rta.Service.Rta.LegacyAuthenticationKey)
				throw new LegacyAuthenticationKeyException("Using the default authentication key with more than one tenant is not allowed");
			var tenant = _findTenantByRtaKey.Find(rtaKey);
			return tenant != null ? tenant.Name : null;
		}
	}
}