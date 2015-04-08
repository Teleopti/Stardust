using Teleopti.Ccc.Infrastructure.MultiTenancy.Client;
using Teleopti.Ccc.Infrastructure.Web;

namespace Teleopti.Ccc.Web.Areas.Start.Core.Config
{
	public interface ISharedSettingsFactory
	{
		SharedSettings Create();
	}
}