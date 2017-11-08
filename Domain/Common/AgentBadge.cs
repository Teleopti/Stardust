using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Common
{
	public class AgentBadge
	{
		public static IList<AgentBadge> FromAgentBadgeTransaction(ICollection<IAgentBadgeTransaction> agentBadgeTransaction)
		{
			return agentBadgeTransaction.GroupBy(x => new
			{
				PersonId = x.Person.Id.GetValueOrDefault(),
				x.BadgeType
			}).Select(g => new AgentBadge
			{
				Person = g.Key.PersonId,
				BadgeType = g.Key.BadgeType,
				TotalAmount = g.Sum(x => x.Amount)
			}).ToList();
		}

		private bool _initialized;
		private int _lastAmount;
		private bool _bronzeBadgeAdded;
		private bool _silverBadgeAdded;
		private bool _goldBadgeAdded;

		private int _totalAmount;

		public Guid Person { get; set; }

		public BadgeType BadgeType { get; set; }

		public int TotalAmount
		{
			get => _totalAmount;
			set
			{
				if (!_initialized)
				{
					_lastAmount = value;
					_totalAmount = value;
					_initialized = true;
				}
				else
				{
					_lastAmount = _totalAmount;
					_totalAmount = value;
				}
			}
		}

		public virtual int GetBronzeBadge(int silverToBronzeRate, int goldToSilverRate)
		{
			updateBadgeAddedFlag(silverToBronzeRate, goldToSilverRate);
			return getBronzeBadgeCount(_totalAmount, silverToBronzeRate);
		}

		public virtual int GetSilverBadge(int silverToBronzeRate, int goldToSilverRate)
		{
			updateBadgeAddedFlag(silverToBronzeRate, goldToSilverRate);
			return getSilverBadgeCount(_totalAmount, silverToBronzeRate, goldToSilverRate);
		}

		public virtual int GetGoldBadge(int silverToBronzeRate, int goldToSilverRate)
		{
			updateBadgeAddedFlag(silverToBronzeRate, goldToSilverRate);
			return getGoldBadgeCount(_totalAmount, silverToBronzeRate, goldToSilverRate);
		}

		public virtual bool IsBronzeBadgeAdded(int silverToBronzeRate, int goldToSilverRate)
		{
			updateBadgeAddedFlag(silverToBronzeRate, goldToSilverRate);
			return _bronzeBadgeAdded;
		}

		public virtual bool IsSilverBadgeAdded(int silverToBronzeRate, int goldToSilverRate)
		{
			updateBadgeAddedFlag(silverToBronzeRate, goldToSilverRate);
			return _silverBadgeAdded;
		}

		public virtual bool IsGoldBadgeAdded(int silverToBronzeRate, int goldToSilverRate)
		{
			updateBadgeAddedFlag(silverToBronzeRate, goldToSilverRate);
			return _goldBadgeAdded;
		}

		private void updateBadgeAddedFlag(int silverToBronzeRate, int goldToSilverRate)
		{
			var previousBronzeBadge = getBronzeBadgeCount(_lastAmount, silverToBronzeRate);
			var previousSilverBadge = getSilverBadgeCount(_lastAmount, silverToBronzeRate, goldToSilverRate);
			var previousGoldBadge = getGoldBadgeCount(_lastAmount, silverToBronzeRate, goldToSilverRate);

			var currentBronzeBadge = getBronzeBadgeCount(_totalAmount, silverToBronzeRate);
			var currentSilverBadge = getSilverBadgeCount(_totalAmount, silverToBronzeRate, goldToSilverRate);
			var currentGoldBadge = getGoldBadgeCount(_totalAmount, silverToBronzeRate, goldToSilverRate);

			_bronzeBadgeAdded = currentBronzeBadge > previousBronzeBadge;
			_silverBadgeAdded = currentSilverBadge > previousSilverBadge;
			_goldBadgeAdded = currentGoldBadge > previousGoldBadge;
		}

		#region Calculate badge count
		private static int getGoldBadgeCount(int amount, int silverToBronzeRate, int goldToSilverRate)
		{
			return silverToBronzeRate != 0 && goldToSilverRate != 0
				? amount / (silverToBronzeRate * goldToSilverRate)
				: 0;
		}

		private static int getSilverBadgeCount(int amount, int silverToBronzeRate, int goldToSilverRate)
		{
			return silverToBronzeRate != 0 && goldToSilverRate != 0
				? (amount / silverToBronzeRate) % goldToSilverRate
				: 0;
		}

		private static int getBronzeBadgeCount(int amount, int silverToBronzeRate)
		{
			return silverToBronzeRate != 0 ? amount % silverToBronzeRate : amount;
		}

		#endregion
	}
}