using System;

namespace Teleopti.Interfaces.Domain
{
	/// <summary>
	/// Use your english skills and resharper to understand what I am
	/// </summary>
	public interface IPersonScheduleDayReadModel
	{
		/// <summary>
		/// Use your english skills and resharper to understand what I am
		/// </summary>
		Guid PersonId { get; set; }
		/// <summary>
		/// Use your english skills and resharper to understand what I am
		/// </summary>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1716:IdentifiersShouldNotMatchKeywords", MessageId = "Date")]
		DateTime Date { get; set; }
		/// <summary>
		/// Use your english skills and resharper to understand what I am
		/// </summary>
		DateOnly BelongsToDate { get; }
		/// <summary>
		/// Use your english skills and resharper to understand what I am
		/// </summary>
		DateTime? Start { get; set; }
		/// <summary>
		/// Use your english skills and resharper to understand what I am
		/// </summary>
		DateTime? End { get; set; }
		/// <summary>
		/// Use your english skills and resharper to understand what I am
		/// </summary>
		string Model { get; set; }
		/// <summary>
		/// when request come from a bulletin board, it reference the offer id
		/// </summary>
		Guid? ShiftExchangeOffer { get; set; }

		bool IsDayOff { get; set; }
		DateTime? MinStart { get; set; }
		bool IsLastPage { get; set; }
		int Total { get; set; }

		string FirstName { get; set; }
		string LastName { get; set; }
	}
}