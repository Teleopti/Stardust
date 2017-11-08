using System;
using Teleopti.Ccc.Domain.Common.EntityBaseTypes;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Common
{
	public class AgentBadgeWithRankTransaction : SimpleAggregateRoot, IAgentBadgeWithRankTransaction
	{
		private IPerson _person;
		private BadgeType _badgeType;
		private int _bronzeBadgeAmount;
		private int _silverBadgeAmount;
		private int _goldBadgeAmount;
		private DateOnly _calculatedDate;
		private string _description;
		private DateTime _insertedOn;

		public virtual IPerson Person
		{
			get => _person;
			set => _person = value;
		}

		public virtual BadgeType BadgeType
		{
			get => _badgeType;
			set => _badgeType = value;
		}

		public virtual int BronzeBadgeAmount
		{
			get => _bronzeBadgeAmount;
			set => _bronzeBadgeAmount = value;
		}

		public virtual int SilverBadgeAmount
		{
			get => _silverBadgeAmount;
			set => _silverBadgeAmount = value;
		}

		public virtual int GoldBadgeAmount
		{
			get => _goldBadgeAmount;
			set => _goldBadgeAmount = value;
		}

		public virtual DateOnly CalculatedDate
		{
			get => _calculatedDate;
			set => _calculatedDate = value;
		}

		public virtual string Description
		{
			get => _description;
			set => _description = value;
		}

		public virtual DateTime InsertedOn
		{
			get => _insertedOn;
			set => _insertedOn = value;
		}
	}
}