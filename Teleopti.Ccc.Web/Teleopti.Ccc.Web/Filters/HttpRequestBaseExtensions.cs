using System.Linq;
using System.Web;

namespace Teleopti.Ccc.Web.Filters
{
	public static class HttpRequestBaseExtensions
	{
		public static bool AcceptsJson(this HttpRequestBase request)
		{
			if (request.Headers.AllKeys.Contains("Accept"))
				return request.Headers["Accept"].Contains("application/json");
			return false;
		}
	}
}