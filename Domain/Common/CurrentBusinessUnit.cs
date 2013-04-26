using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Common
{
	public class CurrentBusinessUnit : ICurrentBusinessUnit
	{
		private readonly ICurrentTeleoptiPrincipal _currentTeleoptiPrincipal;

		public CurrentBusinessUnit(ICurrentTeleoptiPrincipal currentTeleoptiPrincipal)
		{
			_currentTeleoptiPrincipal = currentTeleoptiPrincipal;
		}

		public IBusinessUnit Current()
		{
			var currentPrincipal = _currentTeleoptiPrincipal.Current();
			return currentPrincipal == null ? null : ((TeleoptiIdentity)currentPrincipal.Identity).BusinessUnit;
		}
	}
}