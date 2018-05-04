using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.Forecasting.Models
{
	public class CampaignInput : ForecastModel
	{
		public DateOnly[] SelectedDays { get; set; }
		public double CampaignTasksPercent { get; set; }
	}
}