using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Common.EntityBaseTypes;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Common
{
	public class AgentBadgeWithRank : AggregateRoot, IAgentBadgeWithRank
	{
		public static IList<IAgentBadgeWithRank> FromAgentBadgeWithRanksTransaction(
			ICollection<IAgentBadgeWithRankTransaction> agentBadgeWithRankTransactions)
		{
			return agentBadgeWithRankTransactions.GroupBy(x => new
			{
				PersonId = x.Person.Id.GetValueOrDefault(),
				x.BadgeType,
				x.IsExternal
			}).Select(g => new AgentBadgeWithRank
			{
				Person = g.Key.PersonId,
				BadgeType = g.Key.BadgeType,
				IsExternal = g.Key.IsExternal,
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
		private int _badgeType;
		private int _bronzeBadgeAmount;
		private int _silverBadgeAmount;
		private int _goldBadgeAmount;
		private DateTime _lastCalculatedDate;
		private bool _isExternal;

		public virtual Guid Person
		{
			get => _person;
			set => _person = value;
		}

		public virtual int BadgeType
		{
			get => _badgeType;
			set => _badgeType = value;
		}

		public virtual int BronzeBadgeAmount
		{
			get => _bronzeBadgeAmount;
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
			get => _silverBadgeAmount;
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
			get => _goldBadgeAmount;
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
			get => _lastCalculatedDate;
			set => _lastCalculatedDate = value;
		}

		public virtual bool IsBronzeBadgeAdded =>
			_bronzeBadgeInitialized && (_bronzeBadgeAmount > _previousBronzeBadgeAmount);

		public virtual bool IsSilverBadgeAdded =>
			_silverBadgeInitialized && (_silverBadgeAmount > _previousSilverBadgeAmount);

		public virtual bool IsGoldBadgeAdded =>
			_goldBadgeInitialized && (_goldBadgeAmount > _previousGoldBadgeAmount);

		public virtual bool IsExternal
		{
			get => _isExternal;
			set => _isExternal = value;
		}

	}
}