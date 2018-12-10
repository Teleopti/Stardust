using System;

namespace Teleopti.Ccc.Domain.InterfaceLegacy.Domain
{
	public interface IPersonScheduleDayReadModel
	{
	
		Guid PersonId { get; set; }
		
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1716:IdentifiersShouldNotMatchKeywords", MessageId = "Date")]
		DateTime Date { get; set; }
		DateOnly BelongsToDate { get; }
		DateTime? Start { get; set; }
		DateTime? End { get; set; }

		string Model { get; set; }
		Guid? ShiftExchangeOffer { get; set; }

		bool IsDayOff { get; set; }
		DateTime? MinStart { get; set; }
		bool IsLastPage { get; set; }
		int Total { get; set; }

		string FirstName { get; set; }
		string LastName { get; set; }
	}
}