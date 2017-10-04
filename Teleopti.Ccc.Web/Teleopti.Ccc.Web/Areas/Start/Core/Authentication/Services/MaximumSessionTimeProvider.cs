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
			var maximumSessionTimeInMinutesFromConfig = _findTenantByNameWithEnsuredTransaction.Find(tenantName).GetApplicationConfig(TenantApplicationConfigKey.MaximumSessionTimeInMinutes);
			return int.TryParse(maximumSessionTimeInMinutesFromConfig, out int maximumSessionTimeInMinutes) ? maximumSessionTimeInMinutes : 0;
		}
	}
}