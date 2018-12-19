using Teleopti.Ccc.Domain.Logon;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Infrastructure.Web;

namespace Teleopti.Ccc.Infrastructure.Foundation
{
	public class SelectivePrincipalContext : ICurrentPrincipalContext
	{
		private readonly ICurrentHttpContext _httpContext;
		private readonly WebRequestPrincipalContext _web;
		private readonly AppDomainPrincipalContext _appDomain;
		private readonly IThreadPrincipalContext _thread;
		private readonly CurrentProcess _currentProcess;

		public static SelectivePrincipalContext Make()
		{
			var httpContext = new CurrentHttpContext();
			var threadContext = new ThreadPrincipalContext();
			return new SelectivePrincipalContext(
				httpContext,
				new WebRequestPrincipalContext(httpContext, threadContext),
				new AppDomainPrincipalContext(CurrentTeleoptiPrincipal.Make(), threadContext),
				threadContext,
				new CurrentProcess());
		}

		public SelectivePrincipalContext(
			ICurrentHttpContext httpContext,
			WebRequestPrincipalContext web,
			AppDomainPrincipalContext appDomain,
			IThreadPrincipalContext thread,
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