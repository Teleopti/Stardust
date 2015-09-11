using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Infrastructure.Repositories
{
	public interface IOutboundCampaignRepository : IRepository<IOutboundCampaign>
	{
		IList<IOutboundCampaign> GetPlannedCampaigns();
		IList<IOutboundCampaign> GetDoneCampaigns();
		IList<IOutboundCampaign> GetOnGoingCampaigns();
		IList<IOutboundCampaign> GetPlannedCampaigns(DateTimePeriod period);
		IList<IOutboundCampaign> GetDoneCampaigns(DateTimePeriod period);
		IList<IOutboundCampaign> GetOnGoingCampaigns(DateTimePeriod period);
		IList<IOutboundCampaign> GetCampaigns(DateTimePeriod period);
	}
}
