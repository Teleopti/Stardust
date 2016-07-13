using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Common.EntityBaseTypes;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Common
{
	public class AgentBadgeWithRank : SimpleAggregateRoot, IAgentBadgeWithRank
	{
		static public IList<IAgentBadgeWithRank> FromAgentBadgeWithRanksTransaction(
			ICollection<IAgentBadgeWithRankTransaction> agentBadgeWithRankTransactions)
		{
			return agentBadgeWithRankTransactions.GroupBy(x => new
			{
				PersonId = x.Person.Id.GetValueOrDefault(),
				x.BadgeType
			}).Select(g => new AgentBadgeWithRank
			{
				Person = g.Key.PersonId,
				BadgeType = g.Key.BadgeType,
				BronzeBadgeAmount = g.Sum(x => x.BronzeBadgeAmount),
				SilverBadgeAmount = g.Sum(x => x.SilverBadgeAmount),
				GoldBadgeAmount = g.Sum(x => x.GoldBadgeAmount)
			}).Cast<IAgentBadgeWithRank>().ToList();
		}


		private bool _bronzeBadgeInitialized;
		private bool _silverBadgeInitialized;
		private bool _goldBadgeInitialized;
		private int _previousBronzeBadgeAmount;
		private int _previousSilverBadgeAmount;
		private int _previousGoldBadgeAmount;

		private Guid _person;
		private BadgeType _badgeType;
		private int _bronzeBadgeAmount;
		private int _silverBadgeAmount;
		private int _goldBadgeAmount;
		private DateTime _lastCalculatedDate;

		public virtual Guid Person
		{
			get { return _person; }
			set { _person = value; }
		}

		public virtual BadgeType BadgeType
		{
			get { return _badgeType; }
			set { _badgeType = value; }
		}

		public virtual int BronzeBadgeAmount
		{
			get { return _bronzeBadgeAmount; }
			set
			{
				if (!_bronzeBadgeInitialized)
				{
					_bronzeBadgeInitialized = true;
					_previousBronzeBadgeAmount = value;
				}
				else
				{
					_previousBronzeBadgeAmount = _bronzeBadgeAmount;
				}
				_bronzeBadgeAmount = value;
			}
		}

		public virtual int SilverBadgeAmount
		{
			get { return _silverBadgeAmount; }
			set
			{
				if (!_silverBadgeInitialized)
				{
					_silverBadgeInitialized = true;
					_previousSilverBadgeAmount = value;
				}
				else
				{
					_previousSilverBadgeAmount = _silverBadgeAmount;
				}
				_silverBadgeAmount = value;
			}
		}

		public virtual int GoldBadgeAmount
		{
			get { return _goldBadgeAmount; }
			set
			{
				if (!_goldBadgeInitialized)
				{
					_goldBadgeInitialized = true;
					_previousGoldBadgeAmount = value;
				}
				else
				{
					_previousGoldBadgeAmount = _goldBadgeAmount;
				}
				_goldBadgeAmount = value;
			}
		}

		public virtual DateTime LastCalculatedDate
		{
			get { return _lastCalculatedDate; }
			set { _lastCalculatedDate = value; }
		}

		public virtual bool IsBronzeBadgeAdded
		{
			get { return _bronzeBadgeInitialized && (_bronzeBadgeAmount > _previousBronzeBadgeAmount); }
		}

		public virtual bool IsSilverBadgeAdded
		{
			get { return _silverBadgeInitialized && (_silverBadgeAmount > _previousSilverBadgeAmount); }
		}

		public virtual bool IsGoldBadgeAdded
		{
			get { return _goldBadgeInitialized && (_goldBadgeAmount > _previousGoldBadgeAmount); }
		}
	}
}