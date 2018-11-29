using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;


namespace Teleopti.Ccc.Web.Areas.Outbound.core.Campaign.DataProvider
{
	public interface IOutboundScheduledResourcesCacher
	{
		Dictionary<DateOnly, TimeSpan> GetScheduledTime(IOutboundCampaign campaign);
		Dictionary<DateOnly, TimeSpan> GetForecastedTime(IOutboundCampaign campaign);
		void SetScheduledTime(IOutboundCampaign campaign, Dictionary<DateOnly, TimeSpan> value);
		void SetForecastedTime(IOutboundCampaign campaign, Dictionary<DateOnly, TimeSpan> value);
		void Reset();
		IEnumerable<IOutboundCampaign> FilterNotCached(IEnumerable<IOutboundCampaign> campaigns);

	}
}