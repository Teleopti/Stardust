namespace Teleopti.Interfaces.Domain
{
	public interface IAgentBadge : IAggregateRoot
	{
		int BronzeBadge { get; set; }
		int SilverBadge { get; set; }
		int GoldBadge { get; set; }
		BadgeType BadgeType { get; set; }
	}
}