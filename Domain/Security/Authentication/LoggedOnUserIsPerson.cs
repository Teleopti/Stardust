using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Security.Principal;

namespace Teleopti.Ccc.Domain.Security.Authentication
{
	public class LoggedOnUserIsPerson : ILoggedOnUserIsPerson
	{
		private readonly ICurrentTeleoptiPrincipal _principal;

		public LoggedOnUserIsPerson(ICurrentTeleoptiPrincipal principal)
		{
			_principal = principal;
		}
		
		public bool IsPerson(IPerson person)
		{
			var principal = _principal.Current();
			return principal?.Organisation.IsUser(person) ?? false;
		}
	}
}