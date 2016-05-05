namespace Teleopti.Ccc.Domain.Security.Principal
{
	public interface ICurrentAuthorization
	{
		IAuthorization Current();
	}

	public class CurrentAuthorization : ICurrentAuthorization
	{
		private readonly IAuthorization _authorization;
		private static IAuthorization _globalAuthorization;

		public CurrentAuthorization(IAuthorization authorization)
		{
			_authorization = authorization;
		}

		public static ICurrentAuthorization Make()
		{
			return new CurrentAuthorization(new PrincipalAuthorization(CurrentTeleoptiPrincipal.Make()));
		}

		public static void GloballyUse(IAuthorization authorization)
		{
			_globalAuthorization = authorization;
		}

		public IAuthorization Current()
		{
			if (_globalAuthorization != null)
				return _globalAuthorization;
			return _authorization;
		}

	}
}