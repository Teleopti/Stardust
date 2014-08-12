using System;
using Teleopti.Ccc.Domain.Common.EntityBaseTypes;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Common
{
	public class AgentBadgeThresholdSettings : VersionedAggregateRootWithBusinessUnit, IAgentBadgeThresholdSettings
	{
		private bool _enableBadge;
		private int _answeredCallsThreshold;
		private TimeSpan _aHTThreshold;
		private Percent _adherenceThreshold;
		private int _silverToBronzeBadgeRate;
		private int _goldToSilverBadgeRate;

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
	}
}