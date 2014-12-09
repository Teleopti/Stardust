using Owin;

namespace Teleopti.Ccc.Web.Core.Hangfire
{
	public interface IHangfireServerStarter
	{
		void Start(IAppBuilder application);
	}
}