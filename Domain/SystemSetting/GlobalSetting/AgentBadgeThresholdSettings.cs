using System;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.SystemSetting.GlobalSetting
{
	[Serializable]
	public class AgentBadgeThresholdSettings : SettingValue, IAgentBadgeThresholdSettings
	{
		private bool _enableBadge;
		private int _answeredCallsThreshold = 100;
		private bool _answeredCallsBadgeTypeSelected;
		private TimeSpan _aHTThreshold = new TimeSpan(0, 5, 0);
		private bool _aHTBadgeTypeSelected;
		private Percent _adherenceThreshold = new Percent(0.75);
		private bool _adherenceBadgeTypeSelected;
		private int _silverToBronzeBadgeRate = 2;
		private int _goldToSilverBadgeRate = 5;

		public virtual bool EnableBadge
		{
			get { return _enableBadge; }
			set { _enableBadge = value; }
		}

		public virtual int AnsweredCallsThreshold
		{
			get { return _answeredCallsThreshold; }
			set { _answeredCallsThreshold = value; }
		}

		public virtual TimeSpan AHTThreshold
		{
			get { return _aHTThreshold; }
			set { _aHTThreshold = value; }
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

		public virtual bool AnsweredCallsBadgeTypeSelected
		{
			get { return _answeredCallsBadgeTypeSelected; }
			set { _answeredCallsBadgeTypeSelected = value; }
		}

		public virtual bool AHTBadgeTypeSelected
		{
			get { return _aHTBadgeTypeSelected; }
			set { _aHTBadgeTypeSelected = value; }
		}

		public virtual bool AdherenceBadgeTypeSelected
		{
			get { return _adherenceBadgeTypeSelected; }
			set { _adherenceBadgeTypeSelected = value; }
		}

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
