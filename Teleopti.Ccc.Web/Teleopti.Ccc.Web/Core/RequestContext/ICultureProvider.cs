using System.Globalization;

namespace Teleopti.Ccc.Web.Core.RequestContext
{
	public interface ICultureProvider
	{
		CultureInfo GetCulture();
	}
}