using Teleopti.Ccc.Domain.Common.EntityBaseTypes;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Common
{
	public class AgentBadge : SimpleAggregateRoot, IAgentBadge
	{
		private int _totalAmount;
		private bool _initialized;
		private int _lastAmount;
		private bool _bronzeBadgeAdded;
		private bool _silverBadgeAdded;
		private bool _goldBadgeAdded;

		public IPerson Person { get; set; }
		public BadgeType BadgeType { get; set; }

		public int TotalAmount
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

		public DateOnly LastCalculatedDate { get; set; }

		public int GetBronzeBadge(int silverToBronzeRate, int goldToSilverRate)
		{
			updateBadgeAddedFlag(silverToBronzeRate, goldToSilverRate);
			return getBronzeBadgeCount(_totalAmount, silverToBronzeRate, goldToSilverRate);
		}

		public int GetSilverBadge(int silverToBronzeRate, int goldToSilverRate)
		{
			updateBadgeAddedFlag(silverToBronzeRate, goldToSilverRate);
			return getSilverBadgeCount(_totalAmount, silverToBronzeRate, goldToSilverRate);
		}

		public int GetGoldBadge(int silverToBronzeRate, int goldToSilverRate)
		{
			updateBadgeAddedFlag(silverToBronzeRate, goldToSilverRate);
			return getGoldBadgeCount(_totalAmount, silverToBronzeRate, goldToSilverRate);
		}

		public bool IsBronzeBadgeAdded(int silverToBronzeRate, int goldToSilverRate)
		{
			updateBadgeAddedFlag(silverToBronzeRate, goldToSilverRate);
			return _bronzeBadgeAdded;
		}

		public bool IsSilverBadgeAdded(int silverToBronzeRate, int goldToSilverRate)
		{
			updateBadgeAddedFlag(silverToBronzeRate, goldToSilverRate);
			return _silverBadgeAdded;
		}

		public bool IsGoldBadgeAdded(int silverToBronzeRate, int goldToSilverRate)
		{
			updateBadgeAddedFlag(silverToBronzeRate, goldToSilverRate);
			return _goldBadgeAdded;
		}

		private void updateBadgeAddedFlag(int silverToBronzeRate, int goldToSilverRate)
		{
			var previousBronzeBadge = getBronzeBadgeCount(_lastAmount, silverToBronzeRate, goldToSilverRate);
			var previousSilverBadge = getSilverBadgeCount(_lastAmount, silverToBronzeRate, goldToSilverRate);
			var previousGoldBadge = getGoldBadgeCount(_lastAmount, silverToBronzeRate, goldToSilverRate);

			var currentBronzeBadge = getBronzeBadgeCount(_totalAmount, silverToBronzeRate, goldToSilverRate);
			var currentSilverBadge = getSilverBadgeCount(_totalAmount, silverToBronzeRate, goldToSilverRate);
			var currentGoldBadge = getGoldBadgeCount(_totalAmount, silverToBronzeRate, goldToSilverRate);

			_bronzeBadgeAdded = currentBronzeBadge > previousBronzeBadge;
			_silverBadgeAdded = currentSilverBadge > previousSilverBadge;
			_goldBadgeAdded = currentGoldBadge > previousGoldBadge;
		}

		#region Calculate badge count
		private static int getGoldBadgeCount(int amount, int silverToBronzeRate, int goldToSilverRate)
		{
			return amount / (silverToBronzeRate * goldToSilverRate);
		}

		private static int getSilverBadgeCount(int amount, int silverToBronzeRate, int goldToSilverRate)
		{
			return (amount / silverToBronzeRate) % goldToSilverRate;
		}

		private static int getBronzeBadgeCount(int amount, int silverToBronzeRate, int goldToSilverRate)
		{
			return amount % (goldToSilverRate * silverToBronzeRate);
		}
		#endregion
	}
}