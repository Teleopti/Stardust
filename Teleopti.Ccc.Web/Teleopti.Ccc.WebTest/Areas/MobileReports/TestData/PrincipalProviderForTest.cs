using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Web.Core.RequestContext;

namespace Teleopti.Ccc.WebTest.Areas.MobileReports.TestData
{
	internal class PrincipalProviderForTest : IPrincipalProvider
	{
		private readonly ITeleoptiPrincipal _teleoptiPrincipal;

		public PrincipalProviderForTest(ITeleoptiPrincipal teleoptiPrincipal)
		{
			_teleoptiPrincipal = teleoptiPrincipal;
		}

		public ITeleoptiPrincipal Current()
		{
			return _teleoptiPrincipal;
		}
	}
}