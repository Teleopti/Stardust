using System;
using System.Security.Policy;
using Teleopti.Ccc.Domain.Logon;

namespace Teleopti.Ccc.Domain.Security.Principal
{
	public class WindowsAppDomainPrincipalContext : ICurrentPrincipalContext
	{
		private readonly ICurrentTeleoptiPrincipal _currentTeleoptiPrincipal;
		private readonly IThreadPrincipalContext _threadPrincipalContext;

		public WindowsAppDomainPrincipalContext(
			ICurrentTeleoptiPrincipal currentTeleoptiPrincipal, 
			IThreadPrincipalContext threadPrincipalContext)
		{
			_currentTeleoptiPrincipal = currentTeleoptiPrincipal;
			_threadPrincipalContext = threadPrincipalContext;
		}

		public void SetCurrentPrincipal(ITeleoptiPrincipal principal)
		{
			var currentPrincipal = _currentTeleoptiPrincipal.Current() as TeleoptiPrincipal;
			if (currentPrincipal == null)
			{
				try
				{
					AppDomain.CurrentDomain.SetThreadPrincipal(principal);
				}
				catch (PolicyException)
				{
					//This seems to happen some times when we already have set the default principal, but not for this thread apparently.
				}
				_threadPrincipalContext.SetCurrentPrincipal(principal);
			}
			else
			{
				currentPrincipal.ChangePrincipal((TeleoptiPrincipal)principal);
			}
		}
	}
}