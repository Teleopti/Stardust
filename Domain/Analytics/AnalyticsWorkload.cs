using System;

namespace Teleopti.Ccc.Domain.Analytics
{
	public class AnalyticsWorkload
	{
		public int WorkloadId { get; set; }
		public Guid WorkloadCode { get; set; }
		public string WorkloadName { get; set; }
		public int SkillId { get; set; }
		public Guid SkillCode { get; set; }
		public string SkillName { get; set; }
		public int TimeZoneId { get; set; }
		public Guid ForecastMethodCode { get; set; }
		public string ForecastMethodName { get; set; }
		public double PercentageOffered { get; set; }
		public double PercentageOverflowIn { get; set; }
		public double PercentageOverflowOut { get; set; }
		public double PercentageAbandoned { get; set; }
		public double PercentageAbandonedShort { get; set; }
		public double PercentageAbandonedWithinServiceLevel { get; set; }
		public double PercentageAbandonedAfterServiceLevel { get; set; }
		public int BusinessUnitId { get; set; }
		public int DatasourceId { get; set; }
		public DateTime InsertDate { get; set; }
		public DateTime UpdateDate { get; set; }
		public DateTime DatasourceUpdateDate { get; set; }
		public bool IsDeleted { get; set; }
	}
}