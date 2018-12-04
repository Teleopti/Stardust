using System;
using System.Collections.Generic;

namespace Teleopti.Ccc.Domain.Forecasting.Models
{
	public class ForecastViewModel
	{
		public Guid WorkloadId { get; set; }
		public Guid ScenarioId { get; set; }
		public IList<ForecastDayModel> ForecastDays { get; set; }
		public string WarningMessage { get; set; }
		public bool IsSkillNotInBusinessUnit { get; set; }
	}
}