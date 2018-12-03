using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Outbound;


namespace Teleopti.Ccc.Web.Areas.Outbound.core.Campaign.DataProvider
{
	public class ProductionReplanHelper : IProductionReplanHelper
	{
		private readonly IOutboundCampaignTaskManager _campaignTaskManager;
		private readonly ICreateOrUpdateSkillDays _createOrUpdateSkillDays;
		private readonly IOutboundScheduledResourcesCacher _outboundScheduledResourcesCacher;

		public ProductionReplanHelper(IOutboundCampaignTaskManager campaignTaskManager,
			ICreateOrUpdateSkillDays createOrUpdateSkillDays, IOutboundScheduledResourcesCacher outboundScheduledResourcesCacher)
		{
			_campaignTaskManager = campaignTaskManager;
			_createOrUpdateSkillDays = createOrUpdateSkillDays;
			_outboundScheduledResourcesCacher = outboundScheduledResourcesCacher;
		}

		public void Replan(IOutboundCampaign campaign, params DateOnly[] skipDates)
		{
			if (campaign == null) return;

			var incomingTask = skipDates.Any() ? _campaignTaskManager.GetIncomingTaskFromCampaign(campaign, skipDates) 
				: _campaignTaskManager.GetIncomingTaskFromCampaign(campaign);
			incomingTask.RecalculateDistribution();

			var forecasts = _createOrUpdateSkillDays.UpdateSkillDays(campaign.Skill, incomingTask);
			//persist productionPlan		
			_outboundScheduledResourcesCacher.SetForecastedTime(campaign, forecasts);
		}
	}
}