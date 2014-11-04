using Teleopti.Ccc.Domain.Security.Principal;

namespace Teleopti.Ccc.TestCommon
{
	public class MutableFakeCurrentTeleoptiPrincipal : ICurrentTeleoptiPrincipal
	{
		private ITeleoptiPrincipal _principal;

		public void SetPrincipal(ITeleoptiPrincipal principal)
		{
			_principal = principal;
		}

		public ITeleoptiPrincipal Current() { return _principal; }
	}
}