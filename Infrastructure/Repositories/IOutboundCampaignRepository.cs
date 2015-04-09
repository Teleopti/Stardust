using System;
using Teleopti.Ccc.Domain.Outbound;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Infrastructure.Repositories
{
	public interface IOutboundCampaignRepository : IRepository<Campaign>
	{
		Campaign GetInFull(Guid id);
	}
}
