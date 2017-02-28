using System;

namespace Teleopti.Interfaces.Domain
{
	public interface IAgentBadgeWithRank : IAggregateRoot
	{
		/// <summary>
		/// Person own the badges
		/// </summary>
		Guid Person { get; set; }

		/// <summary>
		/// Badge type
		/// </summary>
		BadgeType BadgeType { get; set; }

		/// <summary>
		/// Amount of bronze badges
		/// </summary>
		int BronzeBadgeAmount { get; set; }

		/// <summary>
		/// Amount of silver badges
		/// </summary>
		int SilverBadgeAmount { get; set; }

		/// <summary>
		/// Amount of gold badges
		/// </summary>
		int GoldBadgeAmount { get; set; }

		/// <summary>
		/// Last badge calculate date
		/// </summary>
		DateTime LastCalculatedDate { get; set; }

		/// <summary>
		/// Indicate if bronze badge added.
		/// </summary>
		/// <returns></returns>
		bool IsSilverBadgeAdded { get; }

		/// <summary>
		/// Indicate if silver badge added.
		/// </summary>
		/// <returns></returns>
		bool IsBronzeBadgeAdded { get; }

		/// <summary>
		/// Indicate if gold badge added
		/// </summary>
		/// <returns></returns>
		bool IsGoldBadgeAdded { get; }
	}
}