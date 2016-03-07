using System;

namespace Teleopti.Ccc.Domain.ApplicationLayer.PersonCollectionChangedHandlers.Analytics
{
	public class AnalyticsSkill
	{
		public int SkillId { get; set; }
		public Guid SkillCode { get; set; }
		public string SkillName { get; set; }
		public int TimeZoneId { get; set; }
		public Guid ForecastMethodCode { get; set; }
		public string ForecastMethodName { get; set; }
		public int BusinessUnitId { get; set; }
		public int DatasourceId { get; set; }
		public DateTime InsertDate { get; set; }
		public DateTime UpdateDate { get; set; }
		public DateTime DatasourceUpdateDate { get; set; }
		public bool IsDeleted { get; set; }
	}
}