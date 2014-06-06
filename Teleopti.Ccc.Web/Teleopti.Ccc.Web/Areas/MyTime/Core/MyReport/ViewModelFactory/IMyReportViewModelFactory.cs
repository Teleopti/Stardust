using System.Collections.Generic;
using Teleopti.Ccc.Web.Areas.MyTime.Models.MyReport;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.MyReport.ViewModelFactory
{
	public interface IMyReportViewModelFactory
	{
		DailyMetricsViewModel CreateDailyMetricsViewModel(DateOnly dateOnly);
		DetailedAdherenceViewModel CreateDetailedAherenceViewModel(DateOnly dateOnly);
        ICollection<QueueMetricsViewModel> CreateQueueMetricsViewModel(DateOnly dateOnly);
	}
}