namespace Teleopti.Ccc.Domain.Security.Principal
{
	public interface ICurrentPrincipalAuthorization
	{
		IPrincipalAuthorization Current();
	}

	public class CurrentPrincipalAuthorization : ICurrentPrincipalAuthorization
	{
		private readonly IPrincipalAuthorization _principalAuthorization;
		private static IPrincipalAuthorization _globalPrincipalAuthorization;

		public CurrentPrincipalAuthorization(IPrincipalAuthorization principalAuthorization)
		{
			_principalAuthorization = principalAuthorization;
		}

		public static ICurrentPrincipalAuthorization Make()
		{
			return new CurrentPrincipalAuthorization(new PrincipalAuthorization(CurrentTeleoptiPrincipal.Make()));
		}

		public static void GloballyUse(IPrincipalAuthorization principalAuthorization)
		{
			_globalPrincipalAuthorization = principalAuthorization;
		}

		public IPrincipalAuthorization Current()
		{
			if (_globalPrincipalAuthorization != null)
				return _globalPrincipalAuthorization;
			return _principalAuthorization;
		}

	}
}