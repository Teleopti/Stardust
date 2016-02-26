using System;
using Teleopti.Interfaces.Infrastructure.Analytics;

namespace Teleopti.Ccc.Infrastructure.Repositories.Analytics
{
	public class AnalyticsSkill : IAnalyticsSkill
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