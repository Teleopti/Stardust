using System.Globalization;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Security.Principal
{
	public class UserCulture : IUserCulture
	{
		private readonly ICurrentTeleoptiPrincipal _currentTeleoptiPrincipal;

		public UserCulture(ICurrentTeleoptiPrincipal currentTeleoptiPrincipal)
		{
			_currentTeleoptiPrincipal = currentTeleoptiPrincipal;
		}

		public CultureInfo GetCulture()
		{
			var current = _currentTeleoptiPrincipal.Current();
			return current != null ? current.Regional.Culture : CultureInfo.CurrentCulture;
		}
	}
}