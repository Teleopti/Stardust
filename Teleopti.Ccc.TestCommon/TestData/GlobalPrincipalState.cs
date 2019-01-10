using System.Security.Principal;
using System.Threading;
using Teleopti.Ccc.Domain.Security.Principal;

namespace Teleopti.Ccc.TestCommon.TestData
{
	public static class GlobalPrincipalState
	{
		public static TeleoptiPrincipalForLegacy Principal;

		public static void EnsureThreadPrincipal()
		{
			if (Thread.CurrentPrincipal.GetType() == typeof(GenericPrincipal))
				Thread.CurrentPrincipal = Principal;
		}
	}
}