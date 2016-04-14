using Teleopti.Ccc.Domain.Security.Principal;

namespace Teleopti.Ccc.Domain.Common
{
	public class CurrentIdentity : ICurrentIdentity
	{
		private readonly ICurrentTeleoptiPrincipal _principal;

		public static ICurrentIdentity Make()
		{
			return new CurrentIdentity(CurrentTeleoptiPrincipal.Make());
		}

		public CurrentIdentity(ICurrentTeleoptiPrincipal principal)
		{
			_principal = principal;
		}

		public ITeleoptiIdentity Current()
		{
			var principal = _principal.Current();
			if (principal == null)
				return null;
			return principal.Identity as ITeleoptiIdentity;
		}
	}
}