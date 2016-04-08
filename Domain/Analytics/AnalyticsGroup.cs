using System;

namespace Teleopti.Ccc.Domain.Analytics
{
	public class AnalyticsGroup : AnalyticsGroupPage
	{
		public int GroupId { get; set; }
		public Guid GroupCode { get; set; }
		public string GroupName { get; set; }
		public bool GroupIsCustom { get; set; }
		public int BusinessUnitId { get; set; }
		public Guid BusinessUnitCode { get; set; }
		public string BusinessUnitName { get; set; }
		public int DatasourceId { get; set; }
		public DateTime InsertDate { get; set; }
		public DateTime DatasourceUpdateDate { get; set; }
	}
}