using System;

namespace Teleopti.Ccc.Domain.Analytics
{
	public class AnalyticBusinessUnit
	{
		public int BusinessUnitId { get; set; }
		public Guid BusinessUnitCode { get; set; }
		public string BusinessUnitName { get; set; }
		public int DatasourceId { get; set; }
		public DateTime InsertDate { get; set; }
		public DateTime UpdateDate { get; set; }
		public DateTime DatasourceUpdateDate { get; set; }
		
	}
}