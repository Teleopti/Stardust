using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Forecasting.Models
{
	public class ForecastDayModel
	{
		public DateOnly Date { get; set; }
		public bool IsOpen { get; set; }
		public double Tasks { get; set; }
		public double AverageTaskTime { get; set; }
		public double AverageAfterTaskTime { get; set; }
		public double TotalTasks { get; set; }
		public double TotalAverageTaskTime { get; set; }
		public double TotalAverageAfterTaskTime { get; set; }

		public double CampaignTasksPercentage { get; set; }
		public double? OverrideTasks { get; set; }
		public double? OverrideAverageTaskTime { get; set; }
		public double? OverrideAverageAfterTaskTime { get; set; }

		public bool HasCampaign { get; set; }
		public bool HasOverride { get; set; }
		public bool IsInModification { get; set; }
		public bool IsForecasted { get; set; }
	}
}