using System;

namespace Teleopti.Ccc.Domain.Analytics
{
	public class AnalyticsScenario
	{ 
		public int ScenarioId { get; set; }
		public Guid? ScenarioCode { get; set; }
		public string ScenarioName { get; set; }
		public bool? DefaultScenario { get; set; }
		public int BusinessUnitId { get; set; }
		public Guid? BusinessUnitCode { get; set; }
		public string BusinessUnitName { get; set; }
		public int DatasourceId { get; set; }
		public DateTime InsertDate { get; set; }
		public DateTime UpdateDate { get; set; }
		public DateTime DatasourceUpdateDate { get; set; }
		public bool IsDeleted { get; set; }
	}
}