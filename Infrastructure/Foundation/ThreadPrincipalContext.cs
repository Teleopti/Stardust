using System.Threading;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Infrastructure.Foundation
{
	public class ThreadPrincipalContext : ICurrentPrincipalContext
	{
		private readonly IPrincipalFactory _factory;

		public ThreadPrincipalContext(IPrincipalFactory factory)
		{
			_factory = factory;
		}

		public void SetCurrentPrincipal(IPerson loggedOnUser, IDataSource dataSource, IBusinessUnit businessUnit, AuthenticationTypeOption teleoptiAuthenticationType)
		{
			Thread.CurrentPrincipal = _factory.MakePrincipal(loggedOnUser, dataSource, businessUnit, teleoptiAuthenticationType);
		}

		public void SetCurrentPrincipal(ITeleoptiPrincipal principal)
		{
			Thread.CurrentPrincipal = principal;
		}
	}
}