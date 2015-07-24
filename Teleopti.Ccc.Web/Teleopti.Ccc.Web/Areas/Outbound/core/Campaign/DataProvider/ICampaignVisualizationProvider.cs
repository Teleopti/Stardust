using System;
using Teleopti.Ccc.Web.Areas.Outbound.Models;

namespace Teleopti.Ccc.Web.Areas.Outbound.core.Campaign.DataProvider
{
	public interface ICampaignVisualizationProvider
	{
		CampaignVisualizationViewModel ProvideVisualization(Guid id);
	}
}