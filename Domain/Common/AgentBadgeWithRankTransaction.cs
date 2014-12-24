using System;
using Teleopti.Ccc.Domain.Common.EntityBaseTypes;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Common
{
	public class AgentBadgeWithRankTransaction : SimpleAggregateRoot, IAgentBadgeWithRankTransaction
	{
		private DateOnly _calculatedDate;
		private DateTime _insertedOn;

		public virtual IPerson Person { get; set; }

		public virtual BadgeType BadgeType { get; set; }

		public virtual int BronzeBadgeAmount { get; set; }

		public virtual int SilverBadgeAmount { get; set; }

		public virtual int GoldBadgeAmount { get; set; }

		public virtual DateOnly CalculatedDate
		{
			get { return _calculatedDate; }
			set { _calculatedDate = value; }
		}

		public virtual string Description { get; set; }

		public virtual DateTime InsertedOn
		{
			get { return _insertedOn; }
			set { _insertedOn = value; }
		}
	}
}