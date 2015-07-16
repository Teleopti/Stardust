using System.Collections.Generic;
using Teleopti.Ccc.Web.Areas.Outbound.core.Campaign.DataProvider;
using Teleopti.Ccc.Web.Areas.Outbound.Models;

namespace Teleopti.Ccc.WebTest.Core.Common.DataProvider
{
    class FakeOutboundCampaignListOrderProvider : ICampaignListOrderProvider
    {
        private readonly List<CampaignStatus> listOrder = new List<CampaignStatus>(); 

        public IEnumerable<CampaignStatus> GetCampaignListOrder()
        {
            return listOrder;
        }

        public void SetCampaignListOrder(List<CampaignStatus> order)
        {
            listOrder.Clear();
            foreach (var status in order)
            {
                listOrder.Add(status);
            }
        }
    }
}
