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
		private readonly ILoggedOnUser _loggedOnUser;

		public MyReportViewModelFactory(IMyReportDataProvider myReportDataProvider, IDailyMetricsMapper mapper, ILoggedOnUser loggedOnUser)
		{
			_myReportDataProvider = myReportDataProvider;
			_mapper = mapper;
			_loggedOnUser = loggedOnUser;
		}

		public DailyMetricsViewModel CreateDailyMetricsViewModel(DateOnly dateOnly)
		{
			var data = _myReportDataProvider.RetrieveDailyMetricsData(dateOnly);

			return _mapper.Map(data, _loggedOnUser);
		}
	}
}