using System;

namespace Teleopti.Ccc.Domain.Analytics
{
	public class AnalyticsRequest
	{
		public Guid RequestCode { get; set; }
		public int PersonId { get; set; }
		public int RequestStartDateId { get; set; }
		public DateTime ApplicationDatetime { get; set; }
		public DateTime RequestStartDate { get; set; }
		public DateTime RequestEndDate { get; set; }
		public int RequestTypeId { get; set; }
		public int RequestStatusId { get; set; }
		public int RequestDayCount { get; set; }
		public int RequestStartDateCount { get; set; }
		public int BusinessUnitId { get; set; }
		public int DatasourceId { get; set; }
		public DateTime InsertDate { get; set; }
		public DateTime UpdateDate { get; set; }
		public DateTime DatasourceUpdateDate { get; set; }
		public int AbsenceId { get; set; }
		public DateTime RequestStartTime { get; set; }
		public DateTime RequestEndTime { get; set; }
		public int RequestedTimeMinutes { get; set; }
	}
}