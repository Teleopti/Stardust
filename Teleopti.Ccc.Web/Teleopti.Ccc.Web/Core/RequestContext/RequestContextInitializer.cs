using System.Web;

namespace Teleopti.Ccc.Web.Core.RequestContext
{
	public class RequestContextInitializer : IRequestContextInitializer
	{
		private readonly HttpContextBase _httpContextBase;
		private readonly IPrincipalFactory _principalFactory;
		private readonly ISetThreadCulture _setThreadCulture;

		public RequestContextInitializer(HttpContextBase httpContextBase,
													IPrincipalFactory principalFactory,
													ISetThreadCulture setThreadCulture)
		{
			_httpContextBase = httpContextBase;
			_principalFactory = principalFactory;
			_setThreadCulture = setThreadCulture;
		}

		public void AttachPrincipalForAuthenticatedUser()
		{
			var teleoptiPrincipal = _principalFactory.Generate();
			if (teleoptiPrincipal == null) return;

			_httpContextBase.User = teleoptiPrincipal;
			System.Threading.Thread.CurrentPrincipal = teleoptiPrincipal;
			_setThreadCulture.SetCulture(teleoptiPrincipal.Regional);
		}
	}
}