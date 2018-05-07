using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Forecasting.Models
{
	public class ForecastDayModel
	{
		public DateOnly Date { get; set; }
		public bool IsOpen { get; set; }
		public double Tasks { get; set; }
		public double TaskTime { get; set; }
		public double AfterTaskTime { get; set; }
		public double TotalTasks { get; set; }
		public double TotalAverageTaskTime { get; set; }
		public double TotalAverageAfterTaskTime { get; set; }
		public int Campaign { get; set; }
		public int Override { get; set; }
		public int Combo { get; set; }
	}
}