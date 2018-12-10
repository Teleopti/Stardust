using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.SystemSetting.OutboundSetting;
using Teleopti.Ccc.Web.Areas.Outbound.core.Campaign.DataProvider;


namespace Teleopti.Ccc.Web.Areas.Outbound.core.Campaign.Rules
{
	public class CampaignWarning
	{
		public Type TypeOfRule { get; set; }
		public string WarningName { get; set; }
		public IOutboundCampaign Campaign { get; set; }
		public double? Threshold { get; set; }
		public WarningThresholdType WarningThresholdType { get; set; }
		public double? TargetValue { get; set; }
	}

	abstract public class CampaignWarningCheckRule
	{
		protected WarningThresholdType ThresholdType { get; set; }
		protected double Threshold { get; set; }

		protected CampaignWarningCheckRule()
		{
			ThresholdType = WarningThresholdType.Absolute;
			Threshold = 1;
		}

		public abstract string GetWarningName();
		public abstract IEnumerable<CampaignWarning> Validate(IOutboundCampaign campaign, IEnumerable<DateOnly> skipDates);

		public void Configure(CampaignWarningConfiguration configuration)
		{
			if (configuration == null) return;
			ThresholdType = configuration.ThresholdType;
			Threshold = configuration.Threshold;
		}
	}	
}