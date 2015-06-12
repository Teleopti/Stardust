using Teleopti.Ccc.Infrastructure.MultiTenancy.Server;
using Teleopti.Ccc.Infrastructure.Web;

namespace Teleopti.Ccc.Web.Areas.MultiTenancy.Core
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
			return _currentHttpContext.Current().Items[WebTenantAuthentication.PersonInfoKey] as PersonInfo;
		}
	}
}