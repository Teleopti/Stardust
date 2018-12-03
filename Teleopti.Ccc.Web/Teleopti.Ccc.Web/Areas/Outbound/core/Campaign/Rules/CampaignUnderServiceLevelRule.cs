using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.SystemSetting.OutboundSetting;
using Teleopti.Ccc.Web.Areas.Outbound.core.Campaign.DataProvider;


namespace Teleopti.Ccc.Web.Areas.Outbound.core.Campaign.Rules
{
	public class CampaignUnderServiceLevelRule : CampaignWarningCheckRule
	{
		private readonly IOutboundCampaignTaskManager _campaignTaskManager;

		public CampaignUnderServiceLevelRule(IOutboundCampaignTaskManager campaignTaskManager)
		{
			_campaignTaskManager = campaignTaskManager;
		}

		public override string GetWarningName()
		{
			return "CampaignUnderSLA";
		}

		public override IEnumerable<CampaignWarning> Validate(IOutboundCampaign campaign, IEnumerable<DateOnly> skipDates)
		{
			var campaignTasks = _campaignTaskManager.GetIncomingTaskFromCampaign(campaign, skipDates.ToList());
			var response = new List<CampaignWarning>();

			var totalWorkloadMinutes =
			   campaignTasks.GetEstimatedIncomingBacklogOnDate(campaignTasks.GetActivePeriod().DayCollection().First()).TotalMinutes;
			var outsideSlaMinutes = campaignTasks.GetTimeOutsideSLA().TotalMinutes;

			if (checkAgainstThreshold(outsideSlaMinutes, totalWorkloadMinutes))
			{
				response.Add(new CampaignWarning
				{
					TypeOfRule = typeof(CampaignUnderServiceLevelRule),
					Campaign = campaign,
					Threshold = Threshold,
					WarningThresholdType = ThresholdType,
					TargetValue = outsideSlaMinutes,
					WarningName = GetWarningName()
				});
			}
			return response;
		}

		private bool checkAgainstThreshold(double outsideSlaMinutes, double totalWorkloadMinutes)
		{
			if (outsideSlaMinutes < 1) return false;
			switch (ThresholdType)
			{
				case WarningThresholdType.Absolute:
					return outsideSlaMinutes > Threshold;
				case WarningThresholdType.Relative:
					return outsideSlaMinutes > totalWorkloadMinutes * Threshold;
				default:
					return false;
			}
		}

	}
}