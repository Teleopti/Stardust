using Teleopti.Ccc.Infrastructure.MultiTenancy.Client;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server;

namespace Teleopti.Ccc.Web.Areas.MultiTenancy.Core
{
	public class WebCurrentTenantCredentials : ICurrentTenantCredentials
	{
		private readonly ICurrentTenantUser _currentTenantUser;

		public WebCurrentTenantCredentials(ICurrentTenantUser currentTenantUser)
		{
			_currentTenantUser = currentTenantUser;
		}

		public TenantCredentials TenantCredentials()
		{
			return new TenantCredentials(_currentTenantUser.CurrentUser().Id, _currentTenantUser.CurrentUser().TenantPassword);
		}
	}
}