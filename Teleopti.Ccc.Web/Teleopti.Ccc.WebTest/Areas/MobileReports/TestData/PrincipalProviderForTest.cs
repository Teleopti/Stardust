namespace Teleopti.Ccc.WebTest.Areas.MobileReports.TestData
{
	using Teleopti.Ccc.Domain.Security.Principal;
	using Teleopti.Ccc.Web.Core.RequestContext;

	internal class PrincipalProviderForTest : IPrincipalProvider
	{
		private readonly TeleoptiPrincipal _teleoptiPrincipal;

		public PrincipalProviderForTest(TeleoptiPrincipal teleoptiPrincipal)
		{
			this._teleoptiPrincipal = teleoptiPrincipal;
		}

		public TeleoptiPrincipal Current()
		{
			return this._teleoptiPrincipal;
		}
	}
}