using System.Diagnostics;
using Teleopti.Ccc.Domain.Logon;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Infrastructure.Web;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Infrastructure.Foundation
{
	public class SelectivePrincipalContext : ICurrentPrincipalContext
	{
		private readonly ICurrentHttpContext _httpContext;
		private readonly WebRequestPrincipalContext _web;
		private readonly WindowsAppDomainPrincipalContext _appDomain;
		private readonly ThreadPrincipalContext _thread;

		public SelectivePrincipalContext(
			ICurrentHttpContext httpContext,
			WebRequestPrincipalContext web,
			WindowsAppDomainPrincipalContext appDomain,
			ThreadPrincipalContext thread
			)
		{
			_httpContext = httpContext;
			_web = web;
			_appDomain = appDomain;
			_thread = thread;
		}

		public void SetCurrentPrincipal(IPerson loggedOnUser, IDataSource dataSource, IBusinessUnit businessUnit)
		{
			selectContext().SetCurrentPrincipal(loggedOnUser, dataSource, businessUnit);
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
			if (Process.GetCurrentProcess().ProcessName.Contains("Teleopti.Ccc"))
				return _appDomain;
			return _thread;
		}
	}
}