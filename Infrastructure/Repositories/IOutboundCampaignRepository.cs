using System.Collections.Generic;
using Teleopti.Ccc.Domain.Outbound;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Infrastructure.Repositories
{
    public interface IOutboundCampaignRepository : IRepository<IOutboundCampaign>
	{
        IList<IOutboundCampaign> GetPlannedCampaigns();
        IList<IOutboundCampaign> GetDoneCampaigns();
        IList<IOutboundCampaign> GetOnGoingCampaigns();
	}
}
