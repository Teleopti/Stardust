using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Common.EntityBaseTypes;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;

namespace Teleopti.Ccc.Domain.Common
{
	public class BadgeTypeInfo
	{
		public bool IsExternal { get; set; }
		public int Id { get; set; }
	}

	[Serializable]
	public class GamificationSetting : AggregateRoot_Events_ChangeInfo_BusinessUnit, IGamificationSetting, IDeleteTag
	{
		private Description _description;
		private GamificationSettingRuleSet _gamificationSettingRuleSet;
		private GamificationRollingPeriodSet _rollingPeriodSet;

		private bool _answeredCallsBadgeEnabled;
		private bool _aHTBadgeEnabled;
		private bool _adherenceBadgeEnabled;

		private int _answeredCallsThreshold = 100;
		private int _answeredCallsBronzeThreshold = 100;
		private int _answeredCallsSilverThreshold = 120;
		private int _answeredCallsGoldThreshold = 160;

		private TimeSpan _aHTThreshold = new TimeSpan(0, 5, 0);
		private TimeSpan _aHTBronzeThreshold = new TimeSpan(0, 5, 0);
		private TimeSpan _aHTSilverThreshold = new TimeSpan(0, 4, 0);
		private TimeSpan _aHTGoldThreshold = new TimeSpan(0, 3, 0);

		private Percent _adherenceThreshold = new Percent(0.75);
		private Percent _adherenceBronzeThreshold = new Percent(0.75);
		private Percent _adherenceSilverThreshold = new Percent(0.80);
		private Percent _adherenceGoldThreshold = new Percent(0.85);

		private IList<IBadgeSetting> _badgeSettings = new List<IBadgeSetting>(); 

		private int _silverToBronzeBadgeRate = 5;
		private int _goldToSilverBadgeRate = 5;
		private bool _isDeleted;
		
        public GamificationSetting(string name) : this()
        {
			_gamificationSettingRuleSet = GamificationSettingRuleSet.RuleWithDifferentThreshold;
			_rollingPeriodSet = GamificationRollingPeriodSet.OnGoing;
            _description = new Description(name);
		}
		
		protected GamificationSetting()
        {
        }
		
        public virtual Description Description
        {
            get => _description;
			set => _description = value;
		}
		
		public virtual GamificationSettingRuleSet GamificationSettingRuleSet
        {
			get => _gamificationSettingRuleSet;
			set => _gamificationSettingRuleSet = value;
		}

		public virtual bool IsDeleted => _isDeleted;

		public virtual int AnsweredCallsThreshold
		{
			get => _answeredCallsThreshold;
			set => _answeredCallsThreshold = value;
		}

		public virtual int AnsweredCallsBronzeThreshold
		{
			get => _answeredCallsBronzeThreshold;
			set => _answeredCallsBronzeThreshold = value;
		}

		public virtual int AnsweredCallsSilverThreshold
		{
			get => _answeredCallsSilverThreshold;
			set => _answeredCallsSilverThreshold = value;
		}

		public virtual int AnsweredCallsGoldThreshold
		{
			get => _answeredCallsGoldThreshold;
			set => _answeredCallsGoldThreshold = value;
		}

		public virtual TimeSpan AHTThreshold
		{
			get => _aHTThreshold;
			set => _aHTThreshold = value;
		}
		public virtual TimeSpan AHTBronzeThreshold
		{
			get => _aHTBronzeThreshold;
			set => _aHTBronzeThreshold = value;
		}

		public virtual TimeSpan AHTSilverThreshold
		{
			get => _aHTSilverThreshold;
			set => _aHTSilverThreshold = value;
		}

		public virtual TimeSpan AHTGoldThreshold
		{
			get => _aHTGoldThreshold;
			set => _aHTGoldThreshold = value;
		}

		public virtual Percent AdherenceThreshold
		{
			get => _adherenceThreshold;
			set => _adherenceThreshold = value;
		}

		public virtual Percent AdherenceBronzeThreshold
		{
			get => _adherenceBronzeThreshold;
			set => _adherenceBronzeThreshold = value;
		}

		public virtual Percent AdherenceSilverThreshold
		{
			get => _adherenceSilverThreshold;
			set => _adherenceSilverThreshold = value;
		}

		public virtual Percent AdherenceGoldThreshold
		{
			get => _adherenceGoldThreshold;
			set => _adherenceGoldThreshold = value;
		}

		public virtual int SilverToBronzeBadgeRate
		{
			get => _silverToBronzeBadgeRate;
			set => _silverToBronzeBadgeRate = value;
		}

		public virtual int GoldToSilverBadgeRate
		{
			get => _goldToSilverBadgeRate;
			set => _goldToSilverBadgeRate = value;
		}

		public virtual GamificationRollingPeriodSet RollingPeriodSet
		{
			get => _rollingPeriodSet;
			set => _rollingPeriodSet = value;
		}

		public virtual bool AnsweredCallsBadgeEnabled
		{
			get => _answeredCallsBadgeEnabled;
			set => _answeredCallsBadgeEnabled = value;
		}

		public virtual bool AHTBadgeEnabled
		{
			get => _aHTBadgeEnabled;
			set => _aHTBadgeEnabled = value;
		}

		public virtual bool AdherenceBadgeEnabled
		{
			get => _adherenceBadgeEnabled;
			set => _adherenceBadgeEnabled = value;
		}

		public virtual IList<IBadgeSetting> BadgeSettings
		{
			get => _badgeSettings;
			set => _badgeSettings = value;
		}

		public virtual void SetDeleted()
		{
			_isDeleted = true;
		}

		public virtual void AddBadgeSetting(IBadgeSetting newBadgeSetting)
		{
			InParameter.NotNull(nameof(newBadgeSetting), newBadgeSetting);
			newBadgeSetting.SetParent(this);
			
			_badgeSettings.Add(newBadgeSetting);
		}

		public virtual object Clone()
		{
			return MemberwiseClone();
		}

		public virtual IGamificationSetting NoneEntityClone()
		{
			var clone = (GamificationSetting) MemberwiseClone();
			clone._badgeSettings = _badgeSettings.ToList();
			return clone;
		}

		public virtual IGamificationSetting EntityClone()
		{
			var clone = (GamificationSetting)MemberwiseClone();
			clone._badgeSettings = _badgeSettings.ToList();
			return clone;
		}

		public virtual IEnumerable<BadgeTypeInfo> EnabledBadgeTypes()
		{
			var enabledBadgeTypes = new List<BadgeTypeInfo>();

			if (AnsweredCallsBadgeEnabled)
				enabledBadgeTypes.Add(new BadgeTypeInfo { IsExternal = false, Id = BadgeType.AnsweredCalls });

			if (AdherenceBadgeEnabled)
				enabledBadgeTypes.Add(new BadgeTypeInfo { IsExternal = false, Id = BadgeType.Adherence });

			if (AHTBadgeEnabled)
				enabledBadgeTypes.Add(new BadgeTypeInfo { IsExternal = false, Id = BadgeType.AverageHandlingTime });

			enabledBadgeTypes.AddRange(BadgeSettings
				.Where(bs => bs.Enabled)
				.Select(bs => new BadgeTypeInfo { IsExternal = true, Id = bs.QualityId })
			);

			return enabledBadgeTypes;
		}

		public virtual string GetExternalBadgeTypeName(int badgeType) => BadgeSettings.FirstOrDefault(bs => bs.QualityId == badgeType)?.Name;

	}
}
