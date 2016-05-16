using System.Threading;

namespace Teleopti.Ccc.Domain.Security.Principal
{
	public interface ICurrentAuthorization
	{
		IAuthorization Current();
	}

	public interface IAuthorizationScope
	{
		void OnThisThreadUse(IAuthorization principalAuthorization);
	}

	public class ThisAuthorization : ICurrentAuthorization
	{
		private readonly IAuthorization _authorization;

		public ThisAuthorization(IAuthorization authorization)
		{
			_authorization = authorization;
		}

		public IAuthorization Current()
		{
			return _authorization;
		}
	}

	public class CurrentAuthorization : ICurrentAuthorization, IAuthorizationScope
	{
		private readonly IAuthorization _authorization;
		private static IAuthorization _globalAuthorization;
		private readonly ThreadLocal<IAuthorization> _threadAuthorization = new ThreadLocal<IAuthorization>();

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

		public void OnThisThreadUse(IAuthorization principalAuthorization)
		{
			_threadAuthorization.Value = principalAuthorization;
		}

		public IAuthorization Current()
		{
			if (_threadAuthorization.Value != null)
				return _threadAuthorization.Value;
			if (_globalAuthorization != null)
				return _globalAuthorization;
			return _authorization;
		}

	}
}