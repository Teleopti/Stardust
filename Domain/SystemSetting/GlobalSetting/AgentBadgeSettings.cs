using System;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.SystemSetting.GlobalSetting
{
	[Serializable]
	public class AgentBadgeSettings : SettingValue, IAgentBadgeSettings
	{
		private int _answeredCallsThreshold = 100;
		private int _answeredCallsBronzeThreshold = 100;
		private int _answeredCallsSilverThreshold = 120;
		private int _answeredCallsGoldThreshold = 160;

		private TimeSpan _ahtThreshold = new TimeSpan(0, 5, 0);
		private TimeSpan _ahtBronzeThreshold = new TimeSpan(0, 5, 0);
		private TimeSpan _ahtSilverThreshold = new TimeSpan(0, 4, 0);
		private TimeSpan _ahtGoldThreshold = new TimeSpan(0, 3, 0);

		private Percent _adherenceThreshold = new Percent(0.75);
		private Percent _adherenceBronzeThreshold = new Percent(0.75);
		private Percent _adherenceSilverThreshold = new Percent(0.80);
		private Percent _adherenceGoldThreshold = new Percent(0.85);

		private int _silverToBronzeBadgeRate = 2;
		private int _goldToSilverBadgeRate = 5;

		public virtual bool BadgeEnabled { get; set; }

		public bool EnableDifferentLevelBadgeCalculation { get; set; }

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
			get { return _ahtThreshold; }
			set { _ahtThreshold = value; }
		}
		public virtual TimeSpan AHTBronzeThreshold
		{
			get { return _ahtBronzeThreshold; }
			set { _ahtBronzeThreshold = value; }
		}

		public virtual TimeSpan AHTSilverThreshold
		{
			get { return _ahtSilverThreshold; }
			set { _ahtSilverThreshold = value; }
		}

		public virtual TimeSpan AHTGoldThreshold
		{
			get { return _ahtGoldThreshold; }
			set { _ahtGoldThreshold = value; }
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

		public virtual bool AnsweredCallsBadgeEnabled { get; set; }

		public virtual bool AHTBadgeEnabled { get; set; }

		public virtual bool AdherenceBadgeEnabled { get; set; }

		public bool Equals(IEntity other)
		{
			throw new NotImplementedException();
		}

		public Guid? Id { get; private set; }
		public void SetId(Guid? newId)
		{
			throw new NotImplementedException();
		}

		public void ClearId()
		{
			throw new NotImplementedException();
		}
	}
}
