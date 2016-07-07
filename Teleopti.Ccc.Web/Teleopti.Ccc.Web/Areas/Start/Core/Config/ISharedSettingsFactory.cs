using Teleopti.Ccc.Infrastructure.MultiTenancy.Client;

namespace Teleopti.Ccc.Web.Areas.Start.Core.Config
{
	public interface ISharedSettingsFactory
	{
		SharedSettings Create();
	}
}