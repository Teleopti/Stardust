using System.Globalization;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Security.Principal
{
	public class UserUiCulture : IUserUiCulture
	{
		private readonly ICurrentTeleoptiPrincipal _currentPrincipal;

		public UserUiCulture(ICurrentTeleoptiPrincipal currentPrincipal)
		{
			_currentPrincipal = currentPrincipal;
		}

		public CultureInfo GetUiCulture()
		{
			var current = _currentPrincipal.Current();
			return current != null ? current.Regional.UICulture : CultureInfo.CurrentUICulture;
			
		}
	}
}