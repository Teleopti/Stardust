using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Web.Areas.Outbound.core.Campaign.Rules;


namespace Teleopti.Ccc.Web.Areas.Outbound.core.Campaign.DataProvider
{
	public interface ICampaignWarningProvider
	{
		IEnumerable<CampaignWarning> CheckCampaign(IOutboundCampaign campaign, IEnumerable<DateOnly> skipDates);
	}
}