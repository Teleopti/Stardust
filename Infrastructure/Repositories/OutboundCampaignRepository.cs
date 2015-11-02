using System.Collections.Generic;
using NHibernate.Criterion;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;
using Campaign = Teleopti.Ccc.Domain.Outbound.Campaign;

namespace Teleopti.Ccc.Infrastructure.Repositories
{
	public class OutboundCampaignRepository : Repository<IOutboundCampaign>, IOutboundCampaignRepository
	{
		public OutboundCampaignRepository(IUnitOfWork unitOfWork)
#pragma warning disable 618
			: base(unitOfWork)
#pragma warning restore 618
		{
		}

		public OutboundCampaignRepository(ICurrentUnitOfWork currentUnitOfWork)
			: base(currentUnitOfWork)
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
