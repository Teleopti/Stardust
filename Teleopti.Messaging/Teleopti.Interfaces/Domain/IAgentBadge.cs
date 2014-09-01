namespace Teleopti.Interfaces.Domain
{
	public interface IAgentBadge : IAggregateRoot
	{
		IPerson Person { get; }
		BadgeType BadgeType { get; set; }

		int BronzeBadge { get; set; }
		int SilverBadge { get; set; }
		int GoldBadge { get; set; }

		bool BronzeBadgeAdded { get; set; }
		bool SilverBadgeAdded { get; set; }
		bool GoldBadgeAdded { get; set; }

		DateOnly LastCalculatedDate { get; set; }

		/// <summary>
		/// Add a new badge to current badge
		/// </summary>
		/// <param name="newBadge">New badge</param>
		/// <param name="silverToBronzeBadgeRate">The rate exchange bronze badge to silver badge.</param>
		/// <param name="goldToSilverBadgeRate">The rate exchange silver badge to gold badge.</param>
		void AddBadge(IAgentBadge newBadge, int silverToBronzeBadgeRate, int goldToSilverBadgeRate);
	}
}