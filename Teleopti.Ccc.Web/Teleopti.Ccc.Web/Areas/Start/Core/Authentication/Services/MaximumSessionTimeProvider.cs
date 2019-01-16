using Teleopti.Ccc.Domain.MultiTenancy;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server;

namespace Teleopti.Ccc.Web.Areas.Start.Core.Authentication.Services
{
	public class MaximumSessionTimeProvider
	{
		private readonly IFindTenantByNameWithEnsuredTransaction _findTenantByNameWithEnsuredTransaction;

		public MaximumSessionTimeProvider(IFindTenantByNameWithEnsuredTransaction findTenantByNameWithEnsuredTransaction)
		{
			_findTenantByNameWithEnsuredTransaction = findTenantByNameWithEnsuredTransaction;
		}

		public int ForTenant(string tenantName)
		{
			var tenant = _findTenantByNameWithEnsuredTransaction.Find(tenantName);
			if (tenant == null)
				return 0;
			var maximumSessionTimeInMinutesFromConfig = tenant.GetApplicationConfig(TenantApplicationConfigKey.MaximumSessionTimeInMinutes);
			return int.TryParse(maximumSessionTimeInMinutesFromConfig, out int maximumSessionTimeInMinutes) ? maximumSessionTimeInMinutes : 0;
		}
	}
}