using System;
using System.Collections.Generic;
using Teleopti.Ccc.Web.Areas.Outbound.core.Campaign.DataProvider;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebTest.Areas.Outbound.Core
{
	public class FakeOutboundScheduledResourcesCacher : IOutboundScheduledResourcesCacher
	{
		public Dictionary<DateOnly, TimeSpan> GetScheduledTime(IOutboundCampaign campaign)
		{
			throw new NotImplementedException();
		}

		public Dictionary<DateOnly, TimeSpan> GetForecastedTime(IOutboundCampaign campaign)
		{
			throw new NotImplementedException();
		}

		public void SetScheduledTime(IOutboundCampaign campaign, Dictionary<DateOnly, TimeSpan> value)
		{
			throw new NotImplementedException();
		}

		public void SetForecastedTime(IOutboundCampaign campaign, Dictionary<DateOnly, TimeSpan> value)
		{
			throw new NotImplementedException();
		}

		public void Reset()
		{
			throw new NotImplementedException();
		}
	}
}