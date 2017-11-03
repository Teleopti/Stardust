using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.Common.EntityBaseTypes;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Common
{
	[Serializable]
	public class GamificationSetting : NonversionedAggregateRootWithBusinessUnit, IGamificationSetting, IDeleteTag
	{
		private Description _description;
		private GamificationSettingRuleSet _gamificationSettingRuleSet;

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

		private IEnumerable<IExternalBadgeSetting> _externalBadgeSettings; 

		private int _silverToBronzeBadgeRate = 5;
		private int _goldToSilverBadgeRate = 5;
		private bool _isDeleted;

		/// <inheritdoc />
		/// <summary>
		/// Creates a new instance of badge setting
		/// </summary>
		/// <param name="name">Name of badge setting</param>
        public GamificationSetting(string name) : this()
        {
			_gamificationSettingRuleSet = GamificationSettingRuleSet.RuleWithDifferentThreshold;
            _description = new Description(name);
		}

        /// <summary>
        /// Constructor for NHibernate
        /// </summary>
		protected GamificationSetting()
        {
        }

        /// <summary>
		/// Name of badge setting
        /// </summary>
        public virtual Description Description
        {
            get => _description;
			set => _description = value;
		}

        /// <summary>
		/// Type of AgentBadgeSettingRuleSet
        /// </summary>
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

		public virtual IEnumerable<IExternalBadgeSetting> ExternalBadgeSettings
		{
			get => _externalBadgeSettings;
			set => _externalBadgeSettings = value;
		}

		public virtual void SetDeleted()
		{
			_isDeleted = true;
		}

		public virtual object Clone()
		{
			return MemberwiseClone();
		}

		public virtual IGamificationSetting NoneEntityClone()
		{
			return (IGamificationSetting) MemberwiseClone();
		}

		public virtual IGamificationSetting EntityClone()
		{
			return (IGamificationSetting)MemberwiseClone();
		}
	}
}
