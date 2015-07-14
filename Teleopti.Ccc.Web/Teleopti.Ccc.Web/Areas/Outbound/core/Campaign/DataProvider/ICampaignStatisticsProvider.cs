using System.Collections.Generic;
using Teleopti.Ccc.Web.Areas.Outbound.Models;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.Outbound.core.Campaign.DataProvider
{
	public interface ICampaignStatisticsProvider
	{
		CampaignStatistics GetWholeStatistics();
		IList<IOutboundCampaign> GetScheduledCampaigns();
		IList<IOutboundCampaign> GetPlannedCampaigns();
	}
}