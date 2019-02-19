using System.Collections.Generic;
using NHibernate.Criterion;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Campaign = Teleopti.Ccc.Domain.Outbound.Campaign;

namespace Teleopti.Ccc.Infrastructure.Repositories
{
	public class OutboundCampaignRepository : Repository<IOutboundCampaign>, IOutboundCampaignRepository
	{
		public OutboundCampaignRepository(ICurrentUnitOfWork currentUnitOfWork)
			: base(currentUnitOfWork, null, null)
		{
		}

		public IList<IOutboundCampaign> GetCampaigns(DateOnlyPeriod period)
		{
			return Session.CreateCriteria<Campaign>()
				.Add(Restrictions.Conjunction()
					.Add(Restrictions.Ge("BelongsToPeriod.period.Maximum", period.StartDate))
					.Add(Restrictions.Le("BelongsToPeriod.period.Minimum", period.EndDate)))
				.AddOrder(Order.Asc("Name"))
				.List<IOutboundCampaign>();
		}
	}
}
