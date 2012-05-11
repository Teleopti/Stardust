using System.Web;

namespace Teleopti.Ccc.Web.Core.RequestContext
{
	public class CurrentHttpContext : ICurrentHttpContext
	{
		public HttpContextBase Current() { return new HttpContextWrapper(HttpContext.Current); }
	}
}