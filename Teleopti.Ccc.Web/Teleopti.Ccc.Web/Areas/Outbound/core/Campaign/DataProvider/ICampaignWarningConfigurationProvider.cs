using System;
using Teleopti.Ccc.Domain.SystemSetting.OutboundSetting;

namespace Teleopti.Ccc.Web.Areas.Outbound.core.Campaign.DataProvider
{	
	public class CampaignWarningConfiguration
	{
		public double Threshold { get; set; }
		public WarningThresholdType ThresholdType { get; set; }
	}

	public interface ICampaignWarningConfigurationProvider
	{
		CampaignWarningConfiguration GetConfiguration(Type rule);
	}

	
}