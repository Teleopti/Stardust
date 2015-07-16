using System.Collections.Generic;
using Teleopti.Ccc.Web.Areas.Outbound.Models;

namespace Teleopti.Ccc.Web.Areas.Outbound.core.Campaign.DataProvider
{    
    public class CampaignListOrderProvider : ICampaignListOrderProvider
    {
        public IEnumerable<CampaignStatus> GetCampaignListOrder()
        {
            return new List<CampaignStatus>
            {
                CampaignStatus.Ongoing,
                CampaignStatus.Planned,
                CampaignStatus.Scheduled,
                CampaignStatus.Done
            };
        }
    }
}