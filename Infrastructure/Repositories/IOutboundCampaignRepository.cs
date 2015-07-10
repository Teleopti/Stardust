using System.Collections.Generic;
using Teleopti.Ccc.Domain.Outbound;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Infrastructure.Repositories
{
	public interface IOutboundCampaignRepository : IRepository<Campaign>
	{
		IList<Campaign> GetPlannedCampaigns();
		IList<Campaign> GetDoneCampaigns();
		IList<Campaign> GetOnGoingCampaigns();
	}
}
