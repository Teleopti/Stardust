using System;

namespace Teleopti.Interfaces.Domain
{
	public interface IAgentBadge : IAggregateRoot
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
		/// Total amount of badges
		/// </summary>
		int TotalAmount { get; set; }

		/// <summary>
		/// Last badge calculate date
		/// </summary>
		DateOnly LastCalculatedDate { get; set; }

		/// <summary>
		/// Indicate if bronze badge added.
		/// </summary>
		/// <param name="silverToBronzeRate"></param>
		/// <param name="goldToSilverRate"></param>
		/// <returns></returns>
		bool IsSilverBadgeAdded(int silverToBronzeRate, int goldToSilverRate);

		/// <summary>
		/// Indicate if silver badge added.
		/// </summary>
		/// <param name="silverToBronzeRate"></param>
		/// <param name="goldToSilverRate"></param>
		/// <returns></returns>
		bool IsBronzeBadgeAdded(int silverToBronzeRate, int goldToSilverRate);

		/// <summary>
		/// Indicate if gold badge added
		/// </summary>
		/// <param name="silverToBronzeRate"></param>
		/// <param name="goldToSilverRate"></param>
		/// <returns></returns>
		bool IsGoldBadgeAdded(int silverToBronzeRate, int goldToSilverRate);
		
		/// <summary>
		/// Get bronze badge count
		/// </summary>
		/// <param name="silverToBronzeRate"></param>
		/// <param name="goldToSilverRate"></param>
		/// <returns></returns>
		int GetBronzeBadge(int silverToBronzeRate, int goldToSilverRate);

		/// <summary>
		/// Get silver badge count
		/// </summary>
		/// <param name="silverToBronzeRate"></param>
		/// <param name="goldToSilverRate"></param>
		/// <returns></returns>
		int GetSilverBadge(int silverToBronzeRate, int goldToSilverRate);

		/// <summary>
		/// Get gold badge count
		/// </summary>
		/// <param name="silverToBronzeRate"></param>
		/// <param name="goldToSilverRate"></param>
		/// <returns></returns>
		int GetGoldBadge(int silverToBronzeRate, int goldToSilverRate);
	}
}