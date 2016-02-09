using System.Security.Principal;
using System.Threading;
using Teleopti.Ccc.Domain.Logon;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Infrastructure.Web;

namespace Teleopti.Ccc.Infrastructure.Foundation
{
	public class WebRequestPrincipalContext : ICurrentPrincipalContext
	{
		private readonly ICurrentHttpContext _httpContext;

		public WebRequestPrincipalContext(ICurrentHttpContext httpContext)
		{
			_httpContext = httpContext;
		}

		public void SetCurrentPrincipal(ITeleoptiPrincipal principal)
		{
			setPrincipal(principal);
		}

		public void ResetPrincipal()
		{
			setPrincipal(new GenericPrincipal(new GenericIdentity(""), new string[0]));
		}

		private void setPrincipal(IPrincipal principal)
		{
			Thread.CurrentPrincipal = principal;
			_httpContext.Current().User = principal;
		}

	}
}