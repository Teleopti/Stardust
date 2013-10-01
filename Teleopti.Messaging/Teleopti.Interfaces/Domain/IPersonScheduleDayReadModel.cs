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
		Guid TeamId { get; set; }
		/// <summary>
		/// Use your english skills and resharper to understand what I am
		/// </summary>
		Guid SiteId { get; set; }
		/// <summary>
		/// Use your english skills and resharper to understand what I am
		/// </summary>
		Guid BusinessUnitId { get; set; }
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
		DateTime? ShiftStart { get; set; }
		/// <summary>
		/// Use your english skills and resharper to understand what I am
		/// </summary>
		DateTime? ShiftEnd { get; set; }
		/// <summary>
		/// Use your english skills and resharper to understand what I am
		/// </summary>
		string Model { get; set; }
	}
}