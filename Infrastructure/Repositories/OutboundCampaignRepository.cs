using System.Collections.Generic;
using NHibernate.Criterion;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;
using Campaign = Teleopti.Ccc.Domain.Outbound.Campaign;

namespace Teleopti.Ccc.Infrastructure.Repositories
{
	public class OutboundCampaignRepository : Repository<IOutboundCampaign>, IOutboundCampaignRepository
	{
		public OutboundCampaignRepository(IUnitOfWork unitOfWork)
            : base(unitOfWork)
        {
        }

		public OutboundCampaignRepository(IUnitOfWorkFactory unitOfWorkFactory)
			: base(unitOfWorkFactory)
		{
		}

		public OutboundCampaignRepository(ICurrentUnitOfWork currentUnitOfWork)
			: base(currentUnitOfWork)
		{
		}

		protected void SetRepositoryFactory(IRepositoryFactory repositoryFactory)
		{
		}

        public IList<IOutboundCampaign> GetPlannedCampaigns()
		{
			return Session.CreateCriteria<Campaign>()
			  .Add(Restrictions.Gt("SpanningPeriod.period.Minimum", DateOnly.Today))
              .List<IOutboundCampaign>();
		}

        public IList<IOutboundCampaign> GetDoneCampaigns()
		{
			return Session.CreateCriteria<Campaign>()
				.Add(Restrictions.Lt("SpanningPeriod.period.Maximum", DateOnly.Today))
                .List<IOutboundCampaign>();
		}

        public IList<IOutboundCampaign> GetOnGoingCampaigns()
		{
			var startEarlierThanToday = Restrictions.Le("SpanningPeriod.period.Minimum", DateOnly.Today);
			var endLaterThanToday = Restrictions.Ge("SpanningPeriod.period.Maximum", DateOnly.Today);

			return Session.CreateCriteria<Campaign>()
				.Add(Restrictions.And(startEarlierThanToday, endLaterThanToday))
                .List<IOutboundCampaign>();
		}
	}
}
