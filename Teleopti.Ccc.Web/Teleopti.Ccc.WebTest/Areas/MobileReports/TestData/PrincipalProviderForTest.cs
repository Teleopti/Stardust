using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Web.Core.RequestContext;

namespace Teleopti.Ccc.WebTest.Areas.MobileReports.TestData
{
	internal class PrincipalProviderForTest : IPrincipalProvider
	{
		private readonly TeleoptiPrincipal _teleoptiPrincipal;

		public PrincipalProviderForTest(TeleoptiPrincipal teleoptiPrincipal)
		{
			_teleoptiPrincipal = teleoptiPrincipal;
		}

		public TeleoptiPrincipal Current()
		{
			return _teleoptiPrincipal;
		}
	}
}