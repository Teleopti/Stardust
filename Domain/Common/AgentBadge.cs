using System;
using Teleopti.Ccc.Domain.Common.EntityBaseTypes;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Common
{
	public class AgentBadge : SimpleAggregateRoot, IAgentBadge
	{
		private bool _initialized;
		private int _lastAmount;
		private bool _bronzeBadgeAdded;
		private bool _silverBadgeAdded;
		private bool _goldBadgeAdded;

		private Guid _person;
		private BadgeType _badgeType;
		private int _totalAmount;
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

		public virtual int TotalAmount
		{
			get { return _totalAmount; }
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

		public virtual DateTime LastCalculatedDate
		{
			get { return _lastCalculatedDate; }
			set { _lastCalculatedDate = value; }
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
			if (silverToBronzeRate == 0 || goldToSilverRate == 0)
			{
				return 0;
			}
			return amount / (silverToBronzeRate * goldToSilverRate);
		}

		private static int getSilverBadgeCount(int amount, int silverToBronzeRate, int goldToSilverRate)
		{
			if (silverToBronzeRate == 0 || goldToSilverRate == 0)
			{
				return 0;
			}
			return (amount / silverToBronzeRate) % goldToSilverRate;
		}

		private static int getBronzeBadgeCount(int amount, int silverToBronzeRate)
		{
			if (silverToBronzeRate == 0)
			{
				return amount;
			}
			return amount % silverToBronzeRate;
		}
		#endregion
	}
}