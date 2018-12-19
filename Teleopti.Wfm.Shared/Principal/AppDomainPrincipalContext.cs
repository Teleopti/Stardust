using System;
using System.Security.Policy;
using Teleopti.Ccc.Domain.Logon;

namespace Teleopti.Ccc.Domain.Security.Principal
{
	public class AppDomainPrincipalContext : ICurrentPrincipalContext
	{
		private readonly ICurrentTeleoptiPrincipal _currentTeleoptiPrincipal;
		private readonly IThreadPrincipalContext _threadPrincipalContext;

		public AppDomainPrincipalContext(
			ICurrentTeleoptiPrincipal currentTeleoptiPrincipal, 
			IThreadPrincipalContext threadPrincipalContext)
		{
			_currentTeleoptiPrincipal = currentTeleoptiPrincipal;
			_threadPrincipalContext = threadPrincipalContext;
		}

		public void SetCurrentPrincipal(ITeleoptiPrincipal principal)
		{
			if (_currentTeleoptiPrincipal.Current() is ITeleoptiPrincipalWithUnsafePerson currentPrincipal)
			{
				currentPrincipal.ChangePrincipal((ITeleoptiPrincipalWithUnsafePerson) principal);
			}
			else
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
		}
	}
}