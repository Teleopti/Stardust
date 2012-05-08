using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Core.RequestContext
{
	public class CurrentBusinessUnitProvider : ICurrentBusinessUnitProvider
	{
		private readonly ICurrentPrincipalProvider _currentPrincipalProvider;

		public CurrentBusinessUnitProvider(ICurrentPrincipalProvider currentPrincipalProvider)
		{
			_currentPrincipalProvider = currentPrincipalProvider;
		}

		public IBusinessUnit CurrentBusinessUnit()
		{
			var currentPrincipal = _currentPrincipalProvider.Current();
			return currentPrincipal == null ? null : ((TeleoptiIdentity) _currentPrincipalProvider.Current().Identity).BusinessUnit;
		}
	}
}