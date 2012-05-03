using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Web.Core.RequestContext;

namespace Teleopti.Ccc.WebTest
{
	public class FakePrincipalProvider : IPrincipalProvider
	{
		private readonly ITeleoptiPrincipal _teleoptiPrincipal;

		public FakePrincipalProvider(ITeleoptiPrincipal teleoptiPrincipal)
		{
			_teleoptiPrincipal = teleoptiPrincipal;
		}

		public ITeleoptiPrincipal Current()
		{
			return _teleoptiPrincipal;
		}
	}
}