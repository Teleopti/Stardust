using System;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;


namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Settings
{
	public class GamificationSettingView : EntityContainer<IGamificationSetting>
	{

		public Description Description
		{
			get { return ContainedEntity.Description; }
			set { ContainedEntity.Description = value; }
		}

		public string UpdatedBy
		{
			get
			{
				if (ContainedEntity.UpdatedBy != null)
					return ContainedEntity.UpdatedBy.Name.ToString();
				return string.Empty;
			}
		}

		public DateTime? UpdatedOn
		{
			get { return ContainedEntity.UpdatedOn; }
		}

		public IBusinessUnit BusinessUnit
		{
			get
			{
				return ContainedEntity.GetOrFillWithBusinessUnit_DONTUSE();
			}
		}

		public GamificationSettingRuleSet GamificationSettingRuleSet
		{
			get { return ContainedEntity.GamificationSettingRuleSet; }
			set { ContainedEntity.GamificationSettingRuleSet = value; }
		}

		public bool AnsweredCallsBadgeEnabled
		{
			get { return ContainedEntity.AnsweredCallsBadgeEnabled; }
			set { ContainedEntity.AnsweredCallsBadgeEnabled = value; }
		}
		public bool AHTBadgeEnabled
		{
			get { return ContainedEntity.AHTBadgeEnabled; }
			set { ContainedEntity.AHTBadgeEnabled = value; }
		}
		public bool AdherenceBadgeEnabled
		{
			get { return ContainedEntity.AdherenceBadgeEnabled; }
			set { ContainedEntity.AdherenceBadgeEnabled = value; }
		}

		public int AnsweredCallsThreshold
		{
			get { return ContainedEntity.AnsweredCallsThreshold; }
			set { ContainedEntity.AnsweredCallsThreshold = value; }
		}
		public int AnsweredCallsBronzeThreshold
		{
			get { return ContainedEntity.AnsweredCallsBronzeThreshold; }
			set { ContainedEntity.AnsweredCallsBronzeThreshold = value; }
		}
		public int AnsweredCallsSilverThreshold
		{
			get { return ContainedEntity.AnsweredCallsSilverThreshold; }
			set { ContainedEntity.AnsweredCallsSilverThreshold = value; }
		}
		public int AnsweredCallsGoldThreshold
		{
			get { return ContainedEntity.AnsweredCallsGoldThreshold; }
			set { ContainedEntity.AnsweredCallsGoldThreshold = value; }
		}
		 
		public TimeSpan AHTThreshold 
		{ 
			get { return ContainedEntity.AHTThreshold; }
			set { ContainedEntity.AHTThreshold = value; }
		}
		public TimeSpan AHTBronzeThreshold {
			get { return ContainedEntity.AHTBronzeThreshold; }
			set { ContainedEntity.AHTBronzeThreshold = value; }
		}
		public TimeSpan AHTSilverThreshold
		{
			get { return ContainedEntity.AHTSilverThreshold; }
			set { ContainedEntity.AHTSilverThreshold = value; }
		}
		public TimeSpan AHTGoldThreshold
		{
			get { return ContainedEntity.AHTGoldThreshold; }
			set { ContainedEntity.AHTGoldThreshold = value; }
		}
		 
		public Percent AdherenceThreshold
		{
			get { return ContainedEntity.AdherenceThreshold; }
			set { ContainedEntity.AdherenceThreshold = value; }
		}
		public Percent AdherenceBronzeThreshold
		{
			get { return ContainedEntity.AdherenceBronzeThreshold; }
			set { ContainedEntity.AdherenceBronzeThreshold = value; }
		}
		public Percent AdherenceSilverThreshold
		{
			get { return ContainedEntity.AdherenceSilverThreshold; }
			set { ContainedEntity.AdherenceSilverThreshold = value; }
		}
		public Percent AdherenceGoldThreshold 
		{
			get { return ContainedEntity.AdherenceGoldThreshold; }
			set { ContainedEntity.AdherenceGoldThreshold = value; }
		}
		
		public int SilverToBronzeBadgeRate
		{
			get { return ContainedEntity.SilverToBronzeBadgeRate; }
			set { ContainedEntity.SilverToBronzeBadgeRate = value; }
		}
		public int GoldToSilverBadgeRate
		{
			get { return ContainedEntity.GoldToSilverBadgeRate; }
			set { ContainedEntity.GoldToSilverBadgeRate = value; }
		}


		public GamificationSettingView(IGamificationSetting setting)
        {
            // Sets the properties
            setContainedEntity(setting);
        }

		private void setContainedEntity(IGamificationSetting setting)
        {
            ContainedEntity = setting.EntityClone();
            ContainedOriginalEntity = setting;
        }

		public IGamificationSetting ContainedOriginalEntity { get; private set; }

		public void UpdateAfterMerge(IGamificationSetting updatedSetting)
        {
			setContainedEntity(updatedSetting);
        }
	}
}
