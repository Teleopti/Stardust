using Teleopti.Ccc.Domain.Security.Principal;

namespace Teleopti.Ccc.TestCommon
{
	public class FakeCurrentTeleoptiPrincipal : ICurrentTeleoptiPrincipal
	{
		private ITeleoptiPrincipal _principal;

		public FakeCurrentTeleoptiPrincipal()
		{
		}

		public FakeCurrentTeleoptiPrincipal(ITeleoptiPrincipal principal)
		{
			_principal = principal;
		}

		public void Fake(ITeleoptiPrincipal principal)
		{
			_principal = principal;
		}

		public ITeleoptiPrincipal Current() { return _principal; }
	}
}