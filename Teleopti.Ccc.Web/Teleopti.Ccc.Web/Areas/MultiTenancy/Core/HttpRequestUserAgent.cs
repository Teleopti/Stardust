using System.Web;

namespace Teleopti.Ccc.Web.Areas.MultiTenancy.Core
{
	public class HttpRequestUserAgent : IHttpRequestUserAgent
	{
		public string Fetch()
		{
			return HttpContext.Current.Request.UserAgent;
		}
	}
}