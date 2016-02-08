using Teleopti.Ccc.Domain.Logon;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.TestCommon
{
	public class FakeCurrentTeleoptiPrincipal : ICurrentTeleoptiPrincipal, ICurrentPrincipalContext
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
		
		public void SetCurrentPrincipal(ITeleoptiPrincipal principal)
		{
			_principal = principal;
		}

		public void ResetPrincipal()
		{
			_principal = null;
		}
	}
}