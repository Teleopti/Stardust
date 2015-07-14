using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.Outbound.core.Campaign.DataProvider
{
	public interface IProductionReplanHelper
	{
        void Replan(IOutboundCampaign campaign);
	}
}