using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Outbound;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.TestCommon.FakeData
{
	public class FakeCampaignRepository : IOutboundCampaignRepository
	{
		private IList<Domain.Outbound.Campaign> _campaigns; 

		public FakeCampaignRepository()
		{
			_campaigns = new List<Domain.Outbound.Campaign>();
		}

		public void Add(Domain.Outbound.Campaign campaign)
		{
			_campaigns.Add(campaign);
		}

		public void Remove(Domain.Outbound.Campaign campaign)
		{
			_campaigns.Remove(_campaigns.First(x => x.Id == campaign.Id));
		}

		public Domain.Outbound.Campaign Get(Guid id)
		{
			return _campaigns.First(x => x.Id == id);
		}

		public IList<Domain.Outbound.Campaign> LoadAll()
		{
			return _campaigns;
		}

		public Domain.Outbound.Campaign Load(Guid id)
		{
			return Get(id);
		}

		public long CountAllEntities()
		{
			return _campaigns.Count;
		}

		public void AddRange(IEnumerable<Domain.Outbound.Campaign> entityCollection)
		{
			throw new NotImplementedException();
		}

		public IUnitOfWork UnitOfWork { get; private set; }
		public IList<Campaign> GetPlannedCampaigns()
		{
			throw new NotImplementedException();
		}

		public IList<Campaign> GetDoneCampaigns()
		{
			throw new NotImplementedException();
		}

		public IList<Campaign> GetOnGoingCampaigns()
		{
			throw new NotImplementedException();
		}
	}
}
