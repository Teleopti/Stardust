using System.Web;

namespace Teleopti.Ccc.Infrastructure.Web
{
	public class CurrentHttpContext : ICurrentHttpContext
	{
		public HttpContextBase Current()
		{
			return HttpContext.Current == null ? null : new HttpContextWrapper(HttpContext.Current);
		}
	}
}