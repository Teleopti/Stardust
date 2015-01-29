using System.Web;

namespace Teleopti.Ccc.Web.Areas.Tenant.Core
{
	public class HttpRequestUserAgent : IHttpRequestUserAgent
	{
		public string Fetch()
		{
			return HttpContext.Current.Request.UserAgent;
		}
	}
}