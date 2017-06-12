using System;
using Teleopti.Ccc.Web.Areas.Outbound.Models;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.Outbound.core.Campaign.DataProvider
{
	public interface ICampaignVisualizationProvider
	{
		CampaignVisualizationViewModel ProvideVisualization(Guid id, params DateOnly[] skipDates);
	}
}