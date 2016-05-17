using System;

namespace Teleopti.Ccc.Domain.Analytics
{
	public class AnalyticsAbsence
	{
		public int AbsenceId { get; set; }
		public Guid AbsenceCode { get; set; }
		public string AbsenceName { get; set; }
		public int DisplayColor { get; set; }
		public bool InContractTime { get; set; }
		public string InContractTimeName { get; set; }
		public bool InPaidTime { get; set; }
		public string InPaidTimeName { get; set; }
		public bool InWorkTime { get; set; }
		public string InWorkTimeName { get; set; }
		public int BusinessUnitId { get; set; }
		public int DatasourceId { get; set; }
		public DateTime InsertDate { get; set; }
		public DateTime UpdateDate { get; set; }
		public DateTime DatasourceUpdateDate { get; set; }
		public bool IsDeleted { get; set; }
		public string DisplayColorHtml { get; set; }
		public string AbsenceShortName { get; set; }
	}
}