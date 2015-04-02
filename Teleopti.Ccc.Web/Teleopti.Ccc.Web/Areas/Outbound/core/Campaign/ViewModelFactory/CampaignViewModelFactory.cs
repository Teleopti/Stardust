using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Teleopti.Ccc.Web.Areas.Outbound.core.Campaign.DataProvider;
using Teleopti.Ccc.Web.Areas.Outbound.Models;

namespace Teleopti.Ccc.Web.Areas.Outbound.core.Campaign.ViewModelFactory
{
	public class CampaignViewModelFactory
	{
		private IOutboundCampaignPersister _persister;

		public CampaignViewModelFactory(IOutboundCampaignPersister persister)
		{
			_persister = persister;
		}

		public CampaignViewModel Create(string name)
		{
			
		}

		
	}
}