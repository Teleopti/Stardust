using System.Threading;

namespace Teleopti.Ccc.Domain.Security.Principal
{
	public class CurrentTeleoptiPrincipal : ICurrentTeleoptiPrincipal
	{
		public ITeleoptiPrincipal Current()
		{
			return Thread.CurrentPrincipal as ITeleoptiPrincipal;
		}
	}
}