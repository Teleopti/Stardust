using System.Globalization;

namespace Teleopti.Ccc.Web.Core.RequestContext
{
	public class CultureProvider : ICultureProvider
	{
		private readonly ICurrentPrincipalProvider _currentPrincipalProvider;

		public CultureProvider(ICurrentPrincipalProvider currentPrincipalProvider)
		{
			_currentPrincipalProvider = currentPrincipalProvider;
		}

		#region ICultureProvider Members

		public CultureInfo GetCulture()
		{
			var current = _currentPrincipalProvider.Current();
			return current != null ? current.Regional.Culture : CultureInfo.CurrentCulture;
		}

		#endregion
	}
}