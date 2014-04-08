using System.Web;

namespace Teleopti.Ccc.Web.WindowsIdentityProvider.Core
{
	public class CurrentHttpContext : ICurrentHttpContext
	{
		public HttpContextBase Current()
		{
			return new HttpContextWrapper(HttpContext.Current);
		}
	}
}