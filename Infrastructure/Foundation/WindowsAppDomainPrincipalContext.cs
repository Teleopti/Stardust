using System;
using System.Security.Policy;
using System.Threading;
using Teleopti.Ccc.Domain.Logon;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Infrastructure.Foundation
{
	public class WindowsAppDomainPrincipalContext : ICurrentPrincipalContext
	{
		//private readonly IPrincipalFactory _factory;

		//public WindowsAppDomainPrincipalContext(IPrincipalFactory factory)
		//{
		//	_factory = factory;
		//}

		//public void SetCurrentPrincipal(IPerson loggedOnUser, IDataSource dataSource, IBusinessUnit businessUnit)
		//{
		//	var teleoptiPrincipal = _factory.MakePrincipal(loggedOnUser, dataSource, businessUnit);

		//	var currentPrincipal = Thread.CurrentPrincipal as TeleoptiPrincipal;
		//	if (currentPrincipal == null)
		//	{
		//		try
		//		{
		//			AppDomain.CurrentDomain.SetThreadPrincipal(teleoptiPrincipal);
		//		}
		//		catch (PolicyException)
		//		{
		//			//This seems to happen some times when we already have set the default principal, but not for this thread apparently.
		//		}
		//		Thread.CurrentPrincipal = teleoptiPrincipal;
		//	}
		//	else
		//	{
		//		currentPrincipal.ChangePrincipal((TeleoptiPrincipal) teleoptiPrincipal);
		//	}
		//}

		public void SetCurrentPrincipal(ITeleoptiPrincipal principal)
		{
			var currentPrincipal = Thread.CurrentPrincipal as TeleoptiPrincipal;
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
				Thread.CurrentPrincipal = principal;
			}
			else
			{
				currentPrincipal.ChangePrincipal((TeleoptiPrincipal)principal);
			}
		}

		public void ResetPrincipal()
		{
			throw new NotImplementedException();
		}
	}
}