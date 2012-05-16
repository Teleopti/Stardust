using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Core.RequestContext
{
	public class CurrentBusinessUnitProvider : ICurrentBusinessUnitProvider
	{
		private readonly ICurrentTeleoptiPrincipal _currentTeleoptiPrincipal;

		public CurrentBusinessUnitProvider(ICurrentTeleoptiPrincipal currentTeleoptiPrincipal)
		{
			_currentTeleoptiPrincipal = currentTeleoptiPrincipal;
		}

		public IBusinessUnit CurrentBusinessUnit()
		{
			var currentPrincipal = _currentTeleoptiPrincipal.Current();
			return currentPrincipal == null ? null : ((TeleoptiIdentity)currentPrincipal.Identity).BusinessUnit;
		}
	}
}