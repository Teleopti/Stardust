using System.Security.Principal;
using System.Threading;
using Teleopti.Ccc.Domain.Logon;

namespace Teleopti.Ccc.Domain.Security.Principal
{
	public interface IThreadPrincipalContext : ICurrentPrincipalContext, ICurrentTeleoptiPrincipal
	{
	}

	public class FakeAppDomainPrincipalContext : IThreadPrincipalContext
	{
		private ITeleoptiPrincipal _principal;

		public void SetCurrentPrincipal(ITeleoptiPrincipal principal)
		{
			_principal = principal;
		}

		public ITeleoptiPrincipal Current()
		{
			return _principal;
		}
	}

	public class FakeThreadPrincipalContext : IThreadPrincipalContext
	{
		private readonly ThreadLocal<ITeleoptiPrincipal> _principal = new ThreadLocal<ITeleoptiPrincipal>();

		public void SetCurrentPrincipal(ITeleoptiPrincipal principal)
		{
			_principal.Value = principal;
		}

		public ITeleoptiPrincipal Current()
		{
			return _principal.Value;
		}
	}

	public class ThreadPrincipalContext : IThreadPrincipalContext
	{
		public void SetCurrentPrincipal(ITeleoptiPrincipal principal)
		{
			if (principal == null)
				Thread.CurrentPrincipal = new GenericPrincipal(new GenericIdentity(""), new string[0]);
			else
				Thread.CurrentPrincipal = principal;
		}

		public ITeleoptiPrincipal Current()
		{
			return Thread.CurrentPrincipal as ITeleoptiPrincipal;
		}
	}
}