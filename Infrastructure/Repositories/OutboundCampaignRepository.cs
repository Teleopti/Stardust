using System;
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
				.Add(Restrictions.Gt("SpanningPeriod.period.Minimum", DateTime.Today))
				.AddOrder(Order.Asc("SpanningPeriod.period.Minimum"))
				.List<IOutboundCampaign>();
		}

		public IList<IOutboundCampaign> GetDoneCampaigns()
		{
			return Session.CreateCriteria<Campaign>()
				.Add(Restrictions.Lt("SpanningPeriod.period.Maximum", DateTime.Today))
				.AddOrder(Order.Desc("SpanningPeriod.period.Minimum"))
				.List<IOutboundCampaign>();
		}

		public IList<IOutboundCampaign> GetOnGoingCampaigns()
		{
			return Session.CreateCriteria<Campaign>()
				.Add(Restrictions.Conjunction()
					.Add(Restrictions.Le("SpanningPeriod.period.Minimum", DateTime.Today))
					.Add(Restrictions.Ge("SpanningPeriod.period.Maximum", DateTime.Today)))
				.AddOrder(Order.Desc("SpanningPeriod.period.Minimum"))
				.List<IOutboundCampaign>();
		}

		public IList<IOutboundCampaign> GetCampaigns(DateTimePeriod period)
		{
			return Session.CreateCriteria<Campaign>()
				.Add(Restrictions.Conjunction()
					.Add(Restrictions.Ge("SpanningPeriod.period.Maximum", period.StartDateTime))
					.Add(Restrictions.Le("SpanningPeriod.period.Minimum", period.EndDateTime)))
					.AddOrder(Order.Desc("SpanningPeriod.period.Minimum"))
				.List<IOutboundCampaign>();
		}
	}
}
