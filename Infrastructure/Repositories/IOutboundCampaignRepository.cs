using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Infrastructure.Repositories
{
	public interface IOutboundCampaignRepository : IRepository<IOutboundCampaign>
	{
		IList<IOutboundCampaign> GetCampaigns(DateTimePeriod period);
	}
}
