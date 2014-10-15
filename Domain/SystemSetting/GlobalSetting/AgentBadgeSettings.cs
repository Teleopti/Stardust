using System;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.SystemSetting.GlobalSetting
{
	[Serializable]
	public class AgentBadgeSettings : SettingValue, IAgentBadgeSettings
	{
		private int _answeredCallsThreshold = 100;
		private TimeSpan _ahtThreshold = new TimeSpan(0, 5, 0);
		private Percent _adherenceThreshold = new Percent(0.75);

		private int _silverToBronzeBadgeRate = 2;
		private int _goldToSilverBadgeRate = 5;

		public virtual bool BadgeEnabled { get; set; }

		public virtual int AnsweredCallsThreshold
		{
			get { return _answeredCallsThreshold; }
			set { _answeredCallsThreshold = value; }
		}

		public virtual TimeSpan AHTThreshold
		{
			get { return _ahtThreshold; }
			set { _ahtThreshold = value; }
		}

		public virtual Percent AdherenceThreshold
		{
			get { return _adherenceThreshold; }
			set { _adherenceThreshold = value; }
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
