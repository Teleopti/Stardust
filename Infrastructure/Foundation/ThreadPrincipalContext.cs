using System.Security.Principal;
using System.Threading;
using Teleopti.Ccc.Domain.Logon;
using Teleopti.Ccc.Domain.Security.Principal;

namespace Teleopti.Ccc.Infrastructure.Foundation
{
	public class ThreadPrincipalContext : ICurrentPrincipalContext
	{
		public void SetCurrentPrincipal(ITeleoptiPrincipal principal)
		{
			Thread.CurrentPrincipal = principal;
		}

		public void ResetPrincipal()
		{
			Thread.CurrentPrincipal = new GenericPrincipal(new GenericIdentity(""), new string[0]);
		}
	}
}