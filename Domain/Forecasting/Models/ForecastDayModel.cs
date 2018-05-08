using Teleopti.Interfaces.Domain;

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
		public double Campaign { get; set; }
		public int Override { get; set; }
		public int CampaignAndOverride { get; set; }
	}
}