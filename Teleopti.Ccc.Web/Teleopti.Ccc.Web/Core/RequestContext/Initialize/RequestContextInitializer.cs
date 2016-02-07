using System.Globalization;
using Teleopti.Ccc.Domain.Logon;
using Teleopti.Ccc.Infrastructure.Foundation;

namespace Teleopti.Ccc.Web.Core.RequestContext.Initialize
{
	public class RequestContextInitializer : IRequestContextInitializer
	{
		private readonly ISessionPrincipalFactory _sessionPrincipalFactory;
		private readonly ISetThreadCulture _setThreadCulture;
		private readonly ICurrentPrincipalContext _currentPrincipalContext;

		public RequestContextInitializer(
			ISessionPrincipalFactory sessionPrincipalFactory,
			ISetThreadCulture setThreadCulture,
			ICurrentPrincipalContext currentPrincipalContext)
		{
			_sessionPrincipalFactory = sessionPrincipalFactory;
			_setThreadCulture = setThreadCulture;
			_currentPrincipalContext = currentPrincipalContext;
		}

		public void SetupPrincipalAndCulture(bool forceUseGregorianCalendar)
		{
			var teleoptiPrincipal = _sessionPrincipalFactory.Generate();
			if (teleoptiPrincipal == null) return;
			
			_currentPrincipalContext.SetCurrentPrincipal(teleoptiPrincipal);
			
			teleoptiPrincipal.Regional.ForceUseGregorianCalendar = forceUseGregorianCalendar;

			_setThreadCulture.SetCulture(teleoptiPrincipal.Regional);
		

		}
		
		
	}
}