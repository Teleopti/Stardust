﻿using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Outbound;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.TestCommon.FakeData
{
	public class FakeCampaignRepository : IOutboundCampaignRepository
	{
		private IList<IOutboundCampaign> _campaigns; 

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

        public IList<IOutboundCampaign> LoadAll()
		{
			return _campaigns;
		}

        public IOutboundCampaign Load(Guid id)
		{
			return Get(id);
		}

		public long CountAllEntities()
		{
			return _campaigns.Count;
		}

	    public void AddRange(IEnumerable<IOutboundCampaign> entityCollection)
	    {
	        throw new NotImplementedException();
	    }	  

		public IUnitOfWork UnitOfWork { get; private set; }
        public IList<IOutboundCampaign> GetPlannedCampaigns()
        {
            return _campaigns.Where(c => c.SpanningPeriod.StartDate.Date > DateOnly.Today.Date).ToList();
        }

        public IList<IOutboundCampaign> GetDoneCampaigns()
        {
            return _campaigns.Where(c => c.SpanningPeriod.EndDate.Date < DateOnly.Today.Date).ToList();
        }

        public IList<IOutboundCampaign> GetOnGoingCampaigns()
        {
            return _campaigns.Where(c => c.SpanningPeriod.Contains(DateOnly.Today)).ToList();
        }
	}
}
