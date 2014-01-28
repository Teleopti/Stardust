using Teleopti.Ccc.Web.Areas.MyTime.Core.MyReport.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Core.MyReport.Mapping;
using Teleopti.Ccc.Web.Areas.MyTime.Models.MyReport;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.MyReport.ViewModelFactory
{
	public class MyReportViewModelFactory:IMyReportViewModelFactory
	{
		private readonly IMyReportDataProvider _myReportDataProvider;
		private readonly IDailyMetricsMapper _mapper;

		public MyReportViewModelFactory(IMyReportDataProvider myReportDataProvider, IDailyMetricsMapper mapper)
		{
			_myReportDataProvider = myReportDataProvider;
			_mapper = mapper;
		}

		public DailyMetricsViewModel CreateDailyMetricsViewModel(DateOnly dateOnly)
		{
			var data = _myReportDataProvider.RetrieveDailyMetricsData(dateOnly);

			return _mapper.Map(data);
		}
	}
}