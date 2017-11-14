using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Portal;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.Gamification.Models
{
	public class GamificationSettingViewModel
	{
		public Guid? Id { get; set; }
		public string Name { get; set; }
		public string UpdatedBy { get; set; }
		public DateTime? UpdatedOn { get; set; }
		public GamificationSettingRuleSet GamificationSettingRuleSet { get; set; }
		public bool AnsweredCallsBadgeEnabled { get; set; }
		public bool AHTBadgeEnabled { get; set; }
		public bool AdherenceBadgeEnabled { get; set; }
		public int AnsweredCallsThreshold { get; set; }
		public int AnsweredCallsBronzeThreshold { get; set; }
		public int AnsweredCallsSilverThreshold { get; set; }
		public int AnsweredCallsGoldThreshold { get; set; }
		public TimeSpan AHTThreshold { get; set; }
		public TimeSpan AHTBronzeThreshold { get; set; }
		public TimeSpan AHTSilverThreshold { get; set; }
		public TimeSpan AHTGoldThreshold { get; set; }
		public Percent AdherenceThreshold { get; set; }
		public Percent AdherenceBronzeThreshold { get; set; }
		public Percent AdherenceSilverThreshold { get; set; }
		public Percent AdherenceGoldThreshold { get; set; }
		public List<ExternalBadgeSettingViewModel> ExternalBadgeSettings { get; set; }
		public int SilverToBronzeBadgeRate { get; set; }
		public int GoldToSilverBadgeRate { get; set; }
	}

	public class GamificationDescriptionViewModel
	{
		public Guid GamificationSettingId { get; set; }
		public Description Value { get; set; }
	}

	public class GamificationThresholdEnabledViewModel
	{
		public Guid GamificationSettingId { get; set; }
		public bool Value { get; set; }
	}

	public class GamificationAnsweredCallsThresholdViewModel
	{
		public Guid GamificationSettingId { get; set; }
		public int Value { get; set; }
	}

	public class GamificationAHTThresholdViewModel
	{
		public Guid GamificationSettingId { get; set; }
		public TimeSpan Value { get; set; }
	}

	public class GamificationAdherenceThresholdViewModel
	{
		public Guid GamificationSettingId { get; set; }
		public Percent Value { get; set; }
	}

	public class GamificationBadgeConversRateViewModel
	{
		public Guid GamificationSettingId { get; set; }
		public int Rate { get; set; }
	}

	public class GamificationChangeRuleForm
	{
		public Guid GamificationSettingId;
		public GamificationSettingRuleSet Rule;
	}

	public class TeamGamificationSettingViewModel
	{
		public SelectOptionItem Team {get; set;}
		public Guid GamificationSettingId { get; set; }
	}

	public class TeamsGamificationSettingForm
	{
		public List<Guid> TeamIds { get; set; }
		public Guid GamificationSettingId { get; set; }
	}

	public class UpdateExternalBadgeSettingViewModel
	{
		public Guid Id { get; set; }
		public Guid? ExternalBadgeSettingId { get; set; }
		public string Name { get; set; }
		public bool Enabled { get; set; }
		public int QualityId { get; set; }
		public bool LargerIsBetter { get; set; }
		public int Threshold { get; set; }
		public int BronzeThreshold { get; set; }
		public int SilverThreshold { get; set; }
		public int GoldThreshold { get; set; }
		public BadgeUnitType UnitType { get; set; }
	}

	public class GamificationDescriptionForm
	{
		public Guid GamificationSettingId { get; set; }
		public string Name { get; set; }
	}

	public class ExternalBadgeSettingDescriptionViewModel
	{
		public Guid GamificationSettingId { get; set; }
		public int QualityId { get; set; }
		public string Name { get; set; }
	}

	public class ExternalBadgeSettingThresholdViewModel
	{
		public Guid GamificationSettingId { get; set; }
		public int QualityId { get; set; }
		public BadgeUnitType UnitType { get; set; }
		public int ThresholdValue { get; set; }
	}

	public class ExternalBadgeSettingEnableViewModel
	{
		public Guid GamificationSettingId { get; set; }
		public int QualityId { get; set; }
		public bool IsEnabled { get; set; }
	}
}