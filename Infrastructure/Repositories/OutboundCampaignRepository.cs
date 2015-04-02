using System;
using NHibernate;
using NHibernate.Criterion;
using NHibernate.Transform;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Infrastructure;
using Campaign = Teleopti.Ccc.Domain.Outbound.Campaign;

namespace Teleopti.Ccc.Infrastructure.Repositories
{
	public class OutboundCampaignRepository : Repository<Campaign>, IOutboundCampaignRepository
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

		public Campaign GetInFull(Guid id)
		{
			return Session.CreateCriteria(typeof (Campaign))
				.SetFetchMode("CampaignWorkingPeriods", FetchMode.Join)
				.SetFetchMode("CampaignWorkingPeriods.CampaignWorkingPeriodAssignments", FetchMode.Join)
				.Add(Restrictions.Eq("Id", id))				
				.SetResultTransformer(Transformers.DistinctRootEntity)				
				.UniqueResult<Campaign>();
		}
	}
}
