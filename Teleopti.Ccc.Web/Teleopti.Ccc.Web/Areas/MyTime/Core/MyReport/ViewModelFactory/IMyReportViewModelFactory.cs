using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Web.Areas.MyTime.Models.MyReport;


namespace Teleopti.Ccc.Web.Areas.MyTime.Core.MyReport.ViewModelFactory
{
	public interface IMyReportViewModelFactory
	{
		DailyMetricsViewModel CreateDailyMetricsViewModel(DateOnly dateOnly);
		DetailedAdherenceViewModel CreateDetailedAherenceViewModel(DateOnly dateOnly);
        ICollection<QueueMetricsViewModel> CreateQueueMetricsViewModel(DateOnly dateOnly);
		bool HasMyReportPermission();
	}
}