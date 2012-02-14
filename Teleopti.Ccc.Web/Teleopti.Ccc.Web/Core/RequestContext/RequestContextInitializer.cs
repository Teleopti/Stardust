using System.Web;

namespace Teleopti.Ccc.Web.Core.RequestContext
{
	public class RequestContextInitializer : IRequestContextInitializer
	{
		private readonly HttpContextBase _httpContextBase;
		private readonly IPrincipalProvider _principalProvider;
		private readonly ISetThreadCulture _setThreadCulture;

		public RequestContextInitializer(HttpContextBase httpContextBase,
													IPrincipalProvider principalProvider,
													ISetThreadCulture setThreadCulture)
		{
			_httpContextBase = httpContextBase;
			_principalProvider = principalProvider;
			_setThreadCulture = setThreadCulture;
		}

		public void AttachPrincipalForAuthenticatedUser()
		{
			var teleoptiPrincipal = _principalProvider.Generate();
			if (teleoptiPrincipal != null)
			{
				_httpContextBase.User = teleoptiPrincipal;
				System.Threading.Thread.CurrentPrincipal = teleoptiPrincipal;

				_setThreadCulture.SetCulture(teleoptiPrincipal.Regional);
			}
		}
	}
}