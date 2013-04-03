using System;

namespace Teleopti.Interfaces.Domain
{
	public interface IPersonScheduleDayReadModel
	{
		Guid PersonId { get; set; }
		Guid TeamId { get; set; }
		Guid SiteId { get; set; }
		Guid BusinessUnitId { get; set; }
		DateTime Date { get; set; }
		DateOnly BelongsToDate { get; }
		DateTime? ShiftStart { get; set; }
		DateTime? ShiftEnd { get; set; }
		string Shift { get; set; }
	}
}