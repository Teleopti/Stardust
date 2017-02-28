using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Infrastructure.Repositories
{
	public interface IOutboundCampaignRepository : IRepository<IOutboundCampaign>
	{
		IList<IOutboundCampaign> GetCampaigns(DateOnlyPeriod period);
	}
}
