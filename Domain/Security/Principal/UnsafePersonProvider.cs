using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Security.Principal
{
	public class UnsafePersonProvider : IUnsafePersonProvider
	{
		public IPerson CurrentUser()
		{
			return ((IUnsafePerson) TeleoptiPrincipal.CurrentPrincipal).Person;
		}
	}
}