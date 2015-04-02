using System.Collections.Generic;
using Teleopti.Ccc.Domain.Outbound;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

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

		public Campaign GetDefaultCampaign(string name)
		{
			var campaign = new Campaign()
			{
				Name = name,
				CallListLen = 100,
				TargetRate = 50,
				Skill = null,
				ConnectRate = 20,
				RightPartyConnectRate = 20,
				ConnectAverageHandlingTime = 30,
				RightPartyAverageHandlingTime = 120,
				UnproductiveTime = 30,
				StartDate = DateOnly.Today,
				EndDate = DateOnly.Today,
				CampaignStatus = CampaignStatus.Draft,
				CampaignWorkingPeriods = new List<CampaignWorkingPeriod>()
			};

			return campaign;
		}


	}
}
