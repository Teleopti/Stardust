using Teleopti.Ccc.Infrastructure.WebReports;
using Teleopti.Ccc.Web.Areas.MyTime.Core.MyReport.Mapping;
using Teleopti.Ccc.Web.Areas.MyTime.Models.MyReport;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.MyReport.ViewModelFactory
{
	public class MyReportViewModelFactory : IMyReportViewModelFactory
	{
		private readonly IDailyMetricsForDayQuery _dailyMetricsRepository;
		private readonly IDailyMetricsMapper _mapper;

		public MyReportViewModelFactory(IDailyMetricsForDayQuery dailyMetricsRepository, IDailyMetricsMapper mapper)
		{
			_dailyMetricsRepository = dailyMetricsRepository;
			_mapper = mapper;
		}

		public DailyMetricsViewModel CreateDailyMetricsViewModel(DateOnly dateOnly)
		{
			var data = _dailyMetricsRepository.Execute(dateOnly);

			return _mapper.Map(data);
		}
	}
}