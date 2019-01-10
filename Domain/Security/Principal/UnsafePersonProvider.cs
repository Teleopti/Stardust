using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Security.Principal
{
	public class UnsafePersonProvider : IUnsafePersonProvider
	{
		public IPerson CurrentUser()
		{
			return ((IUnsafePerson)TeleoptiPrincipal.CurrentPrincipal).Person;
		}
	}
}