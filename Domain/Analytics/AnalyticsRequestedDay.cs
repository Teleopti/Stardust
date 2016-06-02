using System;

namespace Teleopti.Ccc.Domain.Analytics
{
	public class AnalyticsRequestedDay
	{
		public Guid RequestCode { get; set; }
		public int PersonId { get; set; }
		public int RequestDateId { get; set; }
		public int RequestTypeId { get; set; }
		public int RequestStatusId { get; set; }
		public int RequestDayCount { get; set; }
		public int BusinessUnitId { get; set; }
		public int DatasourceId { get; set; }
		public DateTime InsertDate { get; set; }
		public DateTime UpdateDate { get; set; }
		public DateTime DatasourceUpdateDate { get; set; }
		public int AbsenceId { get; set; }
	}
}