using Teleopti.Ccc.Domain.Logon;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Infrastructure.Web;

namespace Teleopti.Ccc.Infrastructure.Foundation
{
	public class WebRequestPrincipalContext : ICurrentPrincipalContext
	{
		private readonly ICurrentHttpContext _httpContext;
		private readonly IThreadPrincipalContext _threadPrincipalContext;

		public WebRequestPrincipalContext(ICurrentHttpContext httpContext, IThreadPrincipalContext threadPrincipalContext)
		{
			_httpContext = httpContext;
			_threadPrincipalContext = threadPrincipalContext;
		}

		public void SetCurrentPrincipal(ITeleoptiPrincipal principal)
		{
			_threadPrincipalContext.SetCurrentPrincipal(principal);
			_httpContext.Current().User = principal;
		}
		
	}
}