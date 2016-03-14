using Teleopti.Ccc.Domain.Logon;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Infrastructure.Web;

namespace Teleopti.Ccc.Infrastructure.Foundation
{
	public class SelectivePrincipalContext : ICurrentPrincipalContext
	{
		private readonly ICurrentHttpContext _httpContext;
		private readonly WebRequestPrincipalContext _web;
		private readonly WindowsAppDomainPrincipalContext _appDomain;
		private readonly ThreadPrincipalContext _thread;
		private readonly CurrentProcess _currentProcess;

		public SelectivePrincipalContext(
			ICurrentHttpContext httpContext,
			WebRequestPrincipalContext web,
			WindowsAppDomainPrincipalContext appDomain,
			ThreadPrincipalContext thread,
			CurrentProcess currentProcess
			)
		{
			_httpContext = httpContext;
			_web = web;
			_appDomain = appDomain;
			_thread = thread;
			_currentProcess = currentProcess;
		}

		public void SetCurrentPrincipal(ITeleoptiPrincipal principal)
		{
			selectContext().SetCurrentPrincipal(principal);
		}

		public void ResetPrincipal()
		{
			selectContext().ResetPrincipal();
		}

		private ICurrentPrincipalContext selectContext()
		{
			if (_httpContext.Current() != null)
				return _web;
			if (_currentProcess.Name().Contains("Teleopti.Ccc"))
				return _appDomain;
			return _thread;
		}
	}
}