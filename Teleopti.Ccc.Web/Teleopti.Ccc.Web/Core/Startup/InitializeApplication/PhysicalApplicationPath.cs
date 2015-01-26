using System.Web;

namespace Teleopti.Ccc.Web.Core.Startup.InitializeApplication
{
	public class PhysicalApplicationPath : IPhysicalApplicationPath
	{
		public string Get()
		{
			return HttpRuntime.AppDomainAppPath;
		}
	}
}