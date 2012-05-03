using System.Globalization;

namespace Teleopti.Ccc.Web.Core.RequestContext
{
	public class CultureProvider : ICultureProvider
	{
		private readonly IPrincipalProvider _principalProvider;

		public CultureProvider(IPrincipalProvider principalProvider)
		{
			_principalProvider = principalProvider;
		}

		#region ICultureProvider Members

		public CultureInfo GetCulture()
		{
			var current = _principalProvider.Current();
			return current != null ? current.Regional.Culture : CultureInfo.CurrentCulture;
		}

		#endregion
	}
}