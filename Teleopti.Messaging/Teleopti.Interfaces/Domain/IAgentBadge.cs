namespace Teleopti.Interfaces.Domain
{
	public interface IAgentBadge
	{
		int BronzeBadge { get; set; }
		int SilverBadge { get; set; }
		int GoldenBadge { get; set; }
		BadgeType BadgeType { get; set; }
	}
}