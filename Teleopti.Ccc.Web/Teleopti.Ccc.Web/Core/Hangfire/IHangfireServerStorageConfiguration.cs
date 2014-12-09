using Hangfire;

namespace Teleopti.Ccc.Web.Core.Hangfire
{
	public interface IHangfireServerStorageConfiguration
	{
		void ConfigureStorage(IBootstrapperConfiguration bootstrapperConfiguration);
	}
}