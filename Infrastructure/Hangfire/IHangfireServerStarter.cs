using Owin;

namespace Teleopti.Ccc.Infrastructure.Hangfire
{
	public interface IHangfireServerStarter
	{
		void Start(IAppBuilder application);
	}
}