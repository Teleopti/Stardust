using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.Forecasting.Models
{
	public class ForecastDayModel
	{
		public DateOnly Date { get; set; }
		public double Tasks { get; set; }
		public double TaskTime { get; set; }
		public double AfterTaskTime { get; set; }
	}
}