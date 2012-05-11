using System.Web;

namespace Teleopti.Ccc.Web.Core.RequestContext
{
	public class RequestContextInitializer : IRequestContextInitializer
	{
		private readonly HttpContextBase _httpContextBase;
		private readonly ISessionPrincipalFactory _sessionPrincipalFactory;
		private readonly ISetThreadCulture _setThreadCulture;

		public RequestContextInitializer(HttpContextBase httpContextBase,
													ISessionPrincipalFactory sessionPrincipalFactory,
													ISetThreadCulture setThreadCulture)
		{
			_httpContextBase = httpContextBase;
			_sessionPrincipalFactory = sessionPrincipalFactory;
			_setThreadCulture = setThreadCulture;
		}

		public void AttachPrincipalForAuthenticatedUser()
		{
			var teleoptiPrincipal = _sessionPrincipalFactory.Generate();
			if (teleoptiPrincipal == null) return;

			_httpContextBase.User = teleoptiPrincipal;
			System.Threading.Thread.CurrentPrincipal = teleoptiPrincipal;
			_setThreadCulture.SetCulture(teleoptiPrincipal.Regional);
		}
	}
}