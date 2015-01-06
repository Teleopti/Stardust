using System;
using System.Security.Cryptography.X509Certificates;
using Teleopti.Ccc.Domain.Common.EntityBaseTypes;
using Teleopti.Ccc.Domain.SystemSetting;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Domain.Common
{
	[Serializable]
	public class DifferentialAgentBadgeSettings : VersionedAggregateRootWithBusinessUnit, IDifferentialAgentBadgeSettings, IDeleteTag
	{
		private Description _description;
		private AgentBadgeSettingRuleSet _badgeSettingRuleSet;

		private bool _answeredCallsBadgeEnabled = false;
		private bool _aHTBadgeEnabled = false;
		private bool _adherenceBadgeEnabled = false;

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

		private int _silverToBronzeBadgeRate = 2;
		private int _goldToSilverBadgeRate = 5;
		private bool _isDeleted;

		/// <summary>
        /// Creates a new instance of badge setting
        /// </summary>
		/// <param name="name">Name of badge setting</param>
        public DifferentialAgentBadgeSettings(string name) : this()
        {
			_badgeSettingRuleSet = AgentBadgeSettingRuleSet.RuleWithRatioConvertor;
            _description = new Description(name);
        }

        /// <summary>
        /// Constructor for NHibernate
        /// </summary>
		protected DifferentialAgentBadgeSettings()
        {
        }

        /// <summary>
		/// Name of badge setting
        /// </summary>
        public virtual Description Description
        {
            get { return _description; }
            set { _description = value; }
        }

        /// <summary>
		/// Type of AgentBadgeSettingRuleSet
        /// </summary>
		public virtual AgentBadgeSettingRuleSet BadgeSettingRuleSet
        {
			get { return _badgeSettingRuleSet; }
			set { _badgeSettingRuleSet = value; }
        }

		public virtual bool IsDeleted
		{
			get { return _isDeleted; }
		}
		public virtual int AnsweredCallsThreshold
		{
			get { return _answeredCallsThreshold; }
			set { _answeredCallsThreshold = value; }
		}

		public virtual int AnsweredCallsBronzeThreshold
		{
			get { return _answeredCallsBronzeThreshold; }
			set { _answeredCallsBronzeThreshold = value; }
		}

		public virtual int AnsweredCallsSilverThreshold
		{
			get { return _answeredCallsSilverThreshold; }
			set { _answeredCallsSilverThreshold = value; }
		}

		public virtual int AnsweredCallsGoldThreshold
		{
			get { return _answeredCallsGoldThreshold; }
			set { _answeredCallsGoldThreshold = value; }
		}

		public virtual TimeSpan AHTThreshold
		{
			get { return _aHTThreshold; }
			set { _aHTThreshold = value; }
		}
		public virtual TimeSpan AHTBronzeThreshold
		{
			get { return _aHTBronzeThreshold; }
			set { _aHTBronzeThreshold = value; }
		}

		public virtual TimeSpan AHTSilverThreshold
		{
			get { return _aHTSilverThreshold; }
			set { _aHTSilverThreshold = value; }
		}

		public virtual TimeSpan AHTGoldThreshold
		{
			get { return _aHTGoldThreshold; }
			set { _aHTGoldThreshold = value; }
		}

		public virtual Percent AdherenceThreshold
		{
			get { return _adherenceThreshold; }
			set { _adherenceThreshold = value; }
		}

		public virtual Percent AdherenceBronzeThreshold
		{
			get { return _adherenceBronzeThreshold; }
			set { _adherenceBronzeThreshold = value; }
		}

		public virtual Percent AdherenceSilverThreshold
		{
			get { return _adherenceSilverThreshold; }
			set { _adherenceSilverThreshold = value; }
		}

		public virtual Percent AdherenceGoldThreshold
		{
			get { return _adherenceGoldThreshold; }
			set { _adherenceGoldThreshold = value; }
		}

		public virtual int SilverToBronzeBadgeRate
		{
			get { return _silverToBronzeBadgeRate; }
			set { _silverToBronzeBadgeRate = value; }
		}

		public virtual int GoldToSilverBadgeRate
		{
			get { return _goldToSilverBadgeRate; }
			set { _goldToSilverBadgeRate = value; }
		}

		public virtual bool AnsweredCallsBadgeEnabled
		{
			get { return _answeredCallsBadgeEnabled; }
			set { _answeredCallsBadgeEnabled = value; }
		}

		public virtual bool AHTBadgeEnabled
		{
			get { return _aHTBadgeEnabled; }
			set { _aHTBadgeEnabled = value; }
		}

		public virtual bool AdherenceBadgeEnabled 
		{
			get { return _adherenceBadgeEnabled; }
			set { _adherenceBadgeEnabled = value; }
		}

		public virtual void SetDeleted()
		{
			_isDeleted = true;
		}
	}
}
