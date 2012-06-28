using Teleopti.Ccc.Domain.Security.Principal;

namespace Teleopti.Ccc.WebTest
{
	public class FakeCurrentTeleoptiPrincipal : ICurrentTeleoptiPrincipal
	{
		private readonly ITeleoptiPrincipal _teleoptiPrincipal;

		public FakeCurrentTeleoptiPrincipal(ITeleoptiPrincipal teleoptiPrincipal)
		{
			_teleoptiPrincipal = teleoptiPrincipal;
		}

		public ITeleoptiPrincipal Current()
		{
			return _teleoptiPrincipal;
		}
	}
}