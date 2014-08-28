namespace Teleopti.Interfaces.Domain
{
	public interface IAgentBadge : IAggregateEntity
	{
		int BronzeBadge { get; set; }
		int SilverBadge { get; set; }
		int GoldBadge { get; set; }
		BadgeType BadgeType { get; set; }
		DateOnly LastCalculatedDate { get; set; }
		bool BronzeBadgeAdded { get; set; }
		bool SilverBadgeAdded { get; set; }
		bool GoldBadgeAdded { get; set; }
	}
}