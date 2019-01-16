using Teleopti.Ccc.Domain.Config;
using Teleopti.Ccc.Infrastructure.Web;

namespace Teleopti.Ccc.Infrastructure.MultiTenancy.Server
{
	public class CurrentTenantUser : ICurrentTenantUser
	{
		private readonly ICurrentHttpContext _currentHttpContext;

		public CurrentTenantUser(ICurrentHttpContext currentHttpContext)
		{
			_currentHttpContext = currentHttpContext;
		}

		public PersonInfo CurrentUser()
		{
			return _currentHttpContext.Current().Items[WebTenantAuthenticationConfiguration.PersonInfoKey] as PersonInfo;
		}
	}
}