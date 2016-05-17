using System;

namespace Teleopti.Ccc.Domain.Analytics
{
	public class AnalyticsActivity
	{
		public int ActivityId { get; set; }
		public Guid ActivityCode { get; set; }
		public string ActivityName { get; set; }
		public int DisplayColor { get; set; }
		public string DisplayColorHtml { get; set; }
		public bool InReadyTime { get; set; }
		public string InReadyTimeName { get; set; }
		public bool InContractTime { get; set; }
		public string InContractTimeName { get; set; }
		public bool InPaidTime { get; set; }
		public string InPaidTimeName { get; set; }
		public bool InWorkTime { get; set; }
		public string InWorkTimeName { get; set; }
		public int BusinessUnitId { get; set; }
		public int DatasourceId { get; set; }
		public DateTime DatasourceUpdateDate { get; set; }
		public bool IsDeleted { get; set; }
	}
}