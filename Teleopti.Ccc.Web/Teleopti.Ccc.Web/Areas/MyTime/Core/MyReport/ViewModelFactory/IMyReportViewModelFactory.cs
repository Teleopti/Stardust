using Teleopti.Ccc.Web.Areas.MyTime.Models.MyReport;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.MyReport.ViewModelFactory
{
	public interface IMyReportViewModelFactory
	{
		DailyMetricsViewModel CreateDailyMetricsViewModel(DateOnly dateOnly);
		DetailedAdherenceViewModel CreateDetailedAherenceViewModel(DateOnly dateOnly);
	}

	public class DetailedAdherenceViewModel
	{
	}
}