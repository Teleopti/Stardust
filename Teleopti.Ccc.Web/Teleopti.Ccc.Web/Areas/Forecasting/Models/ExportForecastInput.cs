using System;

namespace Teleopti.Ccc.Web.Areas.Forecasting.Models
{
	public class ExportForecastInput
	{
		public DateTime ForecastStart { get; set; }
		public DateTime ForecastEnd { get; set; }
		public Guid ScenarioId { get; set; }
		public Guid SkillId { get; set; }
		public Guid WorkloadId { get; set; }
	}
}