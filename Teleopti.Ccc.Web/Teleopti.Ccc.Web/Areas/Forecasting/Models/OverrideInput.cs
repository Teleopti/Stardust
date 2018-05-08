using Teleopti.Ccc.Domain.Forecasting.Models;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.Forecasting.Models
{
	public class OverrideInput : ForecastModel
	{
		public DateOnly[] SelectedDays { get; set; }
		public double? OverrideTasks { get; set; }
		public double? OverrideAverageTaskTime { get; set; }
		public double? OverrideAverageAfterTaskTime { get; set; }
		public bool ShouldOverrideTasks { get; set; }
		public bool ShouldOverrideAverageTaskTime { get; set; }
		public bool ShouldOverrideAverageAfterTaskTime { get; set; }
	}
}