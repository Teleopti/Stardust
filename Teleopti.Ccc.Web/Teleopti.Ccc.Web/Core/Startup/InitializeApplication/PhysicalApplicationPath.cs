using System.Web;

namespace Teleopti.Ccc.Web.Core.Startup.InitializeApplication
{
	public class PhysicalApplicationPath : IPhysicalApplicationPath
	{
		public string Get()
		{
			//return HttpContext.Current.Request.PhysicalApplicationPath;
			return HttpRuntime.AppDomainAppPath;
		}
	}
}