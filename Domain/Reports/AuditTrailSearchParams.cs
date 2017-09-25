using System;

namespace Teleopti.Ccc.Web.Areas.Reports.Controllers
{
	public class AuditTrailSearchParams
	{
		public Guid ChangedByPersonId { get; set; }
		public DateTime ChangesOccurredStartDate { get; set; }
		public DateTime ChangesOccurredEndDate { get; set; }
		public DateTime AffectedPeriodStartDate { get; set; }
		public DateTime AffectedPeriodEndDate { get; set; }
	}
}