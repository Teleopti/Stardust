using System;
using System.Security.Policy;
using System.Security.Principal;
using System.Threading;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Infrastructure.Foundation
{
	public class PrincipalManager : IPrincipalManager
	{
		private readonly IPrincipalFactory _factory;

		public PrincipalManager(IPrincipalFactory factory)
		{
			_factory = factory;
		}

		public void SetCurrentPrincipal(IPerson loggedOnUser, IDataSource dataSource, IBusinessUnit businessUnit, AuthenticationTypeOption teleoptiAuthenticationType)
		{
			var teleoptiPrincipal = _factory.MakePrincipal(loggedOnUser, dataSource, businessUnit, teleoptiAuthenticationType);

			var currentPrincipal = Thread.CurrentPrincipal as TeleoptiPrincipal;
			if (currentPrincipal == null)
			{
				try
				{
					AppDomain.CurrentDomain.SetThreadPrincipal(teleoptiPrincipal);
				}
				catch (PolicyException)
				{
					//This seems to happen some times when we already have set the default principal, but not for this thread apparently.
				}
				Thread.CurrentPrincipal = teleoptiPrincipal;
			}
			else
			{
				currentPrincipal.ChangePrincipal((TeleoptiPrincipal) teleoptiPrincipal);
			}
		}
	}
}