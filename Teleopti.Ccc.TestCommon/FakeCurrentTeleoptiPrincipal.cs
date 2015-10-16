using Teleopti.Ccc.Domain.Security.Principal;

namespace Teleopti.Ccc.TestCommon
{
	public class FakeCurrentTeleoptiPrincipal : ICurrentTeleoptiPrincipal
	{
		private readonly ITeleoptiPrincipal _principal;

		public FakeCurrentTeleoptiPrincipal()
		{
		}

		public FakeCurrentTeleoptiPrincipal(ITeleoptiPrincipal principal)
		{
			_principal = principal;
		}

		public ITeleoptiPrincipal Current() { return _principal; }
	}
}