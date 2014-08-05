using System;
using Teleopti.Ccc.Domain.Common.EntityBaseTypes;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Common
{
	public class AgentBadgeThresholdSettings : VersionedAggregateRootWithBusinessUnit, IAgentBadgeThresholdSettings
	{
		private bool _enableBadge;
		private TimeSpan _calculationTime;
		private int _answeredCallsThreshold;
		private TimeSpan _aHTThreshold;
		private Percent _adherenceThreshold;
		private int _silverBadgeDaysThreshold;
		private int _goldBadgeDaysThreshold;

		public virtual bool EnableBadge
		{
			get { return _enableBadge; }
			set { _enableBadge = value; }
		}

		public virtual TimeSpan CalculationTime
		{
			get { return _calculationTime; }
			set { _calculationTime = value; }
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

		public virtual int SilverBadgeDaysThreshold
		{
			get { return _silverBadgeDaysThreshold; }
			set { _silverBadgeDaysThreshold = value; }
		}

		public virtual int GoldBadgeDaysThreshold
		{
			get { return _goldBadgeDaysThreshold; }
			set { _goldBadgeDaysThreshold = value; }
		}
	}
}