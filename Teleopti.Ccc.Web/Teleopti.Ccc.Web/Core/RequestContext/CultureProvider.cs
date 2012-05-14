using System.Globalization;
using Teleopti.Ccc.Domain.Security.Principal;

namespace Teleopti.Ccc.Web.Core.RequestContext
{
	public class CultureProvider : ICultureProvider
	{
		private readonly ICurrentTeleoptiPrincipal _currentTeleoptiPrincipal;

		public CultureProvider(ICurrentTeleoptiPrincipal currentTeleoptiPrincipal)
		{
			_currentTeleoptiPrincipal = currentTeleoptiPrincipal;
		}

		#region ICultureProvider Members

		public CultureInfo GetCulture()
		{
			var current = _currentTeleoptiPrincipal.Current();
			return current != null ? current.Regional.Culture : CultureInfo.CurrentCulture;
		}

		#endregion
	}
}