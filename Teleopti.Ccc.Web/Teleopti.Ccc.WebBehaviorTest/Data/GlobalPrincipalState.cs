using System.Security.Principal;
using System.Threading;
using Teleopti.Ccc.Domain.Security.Principal;

namespace Teleopti.Ccc.WebBehaviorTest.Data
{
	public static class GlobalPrincipalState
	{
		public static TeleoptiPrincipal Principal;

		public static void EnsureThreadPrincipal()
		{
			if (Thread.CurrentPrincipal.GetType() == typeof(GenericPrincipal))
				Thread.CurrentPrincipal = Principal;
		}
	}
}