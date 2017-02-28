﻿using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.SystemSetting.OutboundSetting;
using Teleopti.Ccc.Web.Areas.Outbound.core.Campaign.DataProvider;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.Outbound.core.Campaign.Rules
{
	public class CampaignOverstaffRule : CampaignWarningCheckRule
	{
		private readonly IOutboundCampaignTaskManager _campaignTaskManager;

		public CampaignOverstaffRule(IOutboundCampaignTaskManager campaignTaskManager)
		{
			_campaignTaskManager = campaignTaskManager;
		}

		public override string GetWarningName()
		{
			return "CampaignOverstaff";
		}

		public override IEnumerable<CampaignWarning> Validate(IOutboundCampaign campaign)
		{
			var campaignTasks = _campaignTaskManager.GetIncomingTaskFromCampaign(campaign);
			var response = new List<CampaignWarning>();

			var totalWorkloadMinutes =
				campaignTasks.GetEstimatedIncomingBacklogOnDate(campaignTasks.GetActivePeriod().DayCollection().First()).TotalMinutes;

			var overstaffMinutes =
				campaignTasks.GetActivePeriod().DayCollection().Sum(d => campaignTasks.GetOverstaffTimeOnDate(d).TotalMinutes);

			if (checkAgainstThreshold(overstaffMinutes, totalWorkloadMinutes))
				response.Add(new CampaignWarning
				{					
					TypeOfRule = typeof(CampaignOverstaffRule),
					Campaign = campaign,
					Threshold = Threshold,
					WarningThresholdType = ThresholdType,
					TargetValue = overstaffMinutes,
					WarningName = GetWarningName()
				});

			return response;
		}

		private bool checkAgainstThreshold(double overstaffMinutes, double totalWorkloadMinutes)
		{
			if (overstaffMinutes < 1) return false;
			switch (ThresholdType)
			{
				case WarningThresholdType.Absolute:
					return overstaffMinutes > Threshold;
				case WarningThresholdType.Relative:
					return overstaffMinutes > totalWorkloadMinutes * Threshold;
				default:
					return false;
			}
		}
	}
}