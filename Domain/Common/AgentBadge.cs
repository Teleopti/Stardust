using System;
using Teleopti.Ccc.Domain.Common.EntityBaseTypes;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Common
{
	public class AgentBadge : AggregateEntity, IAgentBadge
	{
		private int _bronzeBadge;
		private int _silverBadge;
		private int _goldBadge;
		private BadgeType _badgeType;
		private DateOnly _lastCalculatedDate;

		public virtual int BronzeBadge
		{
			get { return _bronzeBadge; }
			set { _bronzeBadge = value; }
		}

		public virtual int SilverBadge
		{
			get { return _silverBadge; }
			set { _silverBadge = value; }
		}

		public virtual int GoldBadge
		{
			get { return _goldBadge; }
			set { _goldBadge = value; }
		}

		public virtual BadgeType BadgeType
		{
			get { return _badgeType; }
			set { _badgeType = value; }
		}

		public virtual DateOnly LastCalculatedDate
		{
			get { return _lastCalculatedDate; }
			set { _lastCalculatedDate = value; }
		}

		public virtual bool BronzeBadgeAdded { get; set; }
		public virtual bool SilverBadgeAdded { get; set; }
		public virtual bool GoldBadgeAdded { get; set; }
	}
}