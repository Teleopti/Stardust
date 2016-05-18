using System;

namespace Teleopti.Ccc.Domain.Analytics
{
	public class AnalyticsOvertime
	{
		public int OvertimeId { get; set; }
		public Guid OvertimeCode { get; set; }
		public string OvertimeName { get; set; }
		public int BusinessUnitId { get; set; }
		public DateTime DatasourceUpdateDate { get; set; }
		public bool IsDeleted { get; set; }
	}
}