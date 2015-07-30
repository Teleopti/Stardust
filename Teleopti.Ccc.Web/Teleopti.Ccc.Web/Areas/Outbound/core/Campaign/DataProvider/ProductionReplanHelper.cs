using Teleopti.Ccc.Domain.Outbound;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.Outbound.core.Campaign.DataProvider
{
	public class ProductionReplanHelper : IProductionReplanHelper
	{
      private readonly IOutboundCampaignTaskManager _campaignTaskManager;	
		private readonly ICreateOrUpdateSkillDays _createOrUpdateSkillDays;

		public ProductionReplanHelper(IOutboundCampaignTaskManager campaignTaskManager, ICreateOrUpdateSkillDays createOrUpdateSkillDays)
		{
			_campaignTaskManager = campaignTaskManager;
			_createOrUpdateSkillDays = createOrUpdateSkillDays;
		}

		public void Replan(IOutboundCampaign campaign)
		{
			if (campaign == null) return;

			var incomingTask = _campaignTaskManager.GetIncomingTaskFromCampaign(campaign);
			incomingTask.RecalculateDistribution();
			//persist productionPlan
			_createOrUpdateSkillDays.UpdateSkillDays(campaign.Skill, incomingTask);
		}
	}
}