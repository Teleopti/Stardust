using Teleopti.Ccc.Domain.Forecasting.Models;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;


namespace Teleopti.Ccc.Web.Areas.Forecasting.Models
{
	public class CampaignInput : ForecastViewModel
	{
		public DateOnly[] SelectedDays { get; set; }
		public double CampaignTasksPercent { get; set; }
	}
}