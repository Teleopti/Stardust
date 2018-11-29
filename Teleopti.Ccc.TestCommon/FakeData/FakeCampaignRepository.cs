using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Infrastructure.Repositories;


namespace Teleopti.Ccc.TestCommon.FakeData
{
	public class FakeCampaignRepository : IOutboundCampaignRepository
	{
		private readonly IList<IOutboundCampaign> _campaigns;

		public FakeCampaignRepository()
		{
			_campaigns = new List<IOutboundCampaign>();
		}

		public void Add(IOutboundCampaign campaign)
		{
			_campaigns.Add(campaign);
		}

		public void Remove(IOutboundCampaign campaign)
		{
			_campaigns.Remove(_campaigns.First(x => x.Id == campaign.Id));
		}

		public IOutboundCampaign Get(Guid id)
		{
			return _campaigns.First(x => x.Id == id);
		}

		public IEnumerable<IOutboundCampaign> LoadAll()
		{
			return _campaigns;
		}

		public IOutboundCampaign Load(Guid id)
		{
			return Get(id);
		}

		public IList<IOutboundCampaign> GetCampaigns(DateOnlyPeriod period)
		{
			return _campaigns.Where(campaign => period.Contains(campaign.BelongsToPeriod)).ToList();
		}
	}
}
