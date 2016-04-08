using System;

namespace Teleopti.Ccc.Domain.Analytics
{
	public class AnalyticsPermission
	{
		public Guid PersonCode { get; set; }
		public int TeamId { get; set; }
		public bool MyOwn { get; set; }
		public int BusinessUnitId { get; set; }
		public int DatasourceId { get; set; }
		public DateTime DatasourceUpdateDate { get; set; }
		public Guid ReportId { get; set; }
	}
}