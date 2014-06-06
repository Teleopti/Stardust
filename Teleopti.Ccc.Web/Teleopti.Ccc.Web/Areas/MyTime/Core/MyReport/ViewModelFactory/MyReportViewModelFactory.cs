using System.Collections.Generic;
using Teleopti.Ccc.Infrastructure.WebReports;
using Teleopti.Ccc.Web.Areas.MyTime.Core.MyReport.Mapping;
using Teleopti.Ccc.Web.Areas.MyTime.Models.MyReport;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.MyReport.ViewModelFactory
{
	public class MyReportViewModelFactory : IMyReportViewModelFactory
	{
		private readonly IDailyMetricsForDayQuery _dailyMetricsRepository;
		private readonly IDetailedAdherenceForDayQuery _detailedAdherenceRepository;
		private readonly IDailyMetricsMapper _dailyMetricsMapper;
		private readonly IDetailedAdherenceMapper _detailedAdherenceMapper;
	    private readonly IQueueMetricsForDayQuery _queueMetricsForDayQuery;
	    private readonly IQueueMetricsMapper _queueMetricsMapper;

	    public MyReportViewModelFactory(IDailyMetricsForDayQuery dailyMetricsRepository,
	        IDetailedAdherenceForDayQuery detailedAdherenceRepository, IDailyMetricsMapper dailyMetricsMapper,
	        IDetailedAdherenceMapper detailedAdherenceMapper,IQueueMetricsForDayQuery queueMetricsForDayQuery,
            IQueueMetricsMapper queueMetricsMapper)
	    {
	        _dailyMetricsRepository = dailyMetricsRepository;
	        _detailedAdherenceRepository = detailedAdherenceRepository;
	        _dailyMetricsMapper = dailyMetricsMapper;
	        _detailedAdherenceMapper = detailedAdherenceMapper;
	        _queueMetricsForDayQuery = queueMetricsForDayQuery;
	        _queueMetricsMapper = queueMetricsMapper;
	    }

	    public DailyMetricsViewModel CreateDailyMetricsViewModel(DateOnly dateOnly)
		{
			var data = _dailyMetricsRepository.Execute(dateOnly);

			return _dailyMetricsMapper.Map(data);
		}

		public DetailedAdherenceViewModel CreateDetailedAherenceViewModel(DateOnly dateOnly)
		{
			var data = _detailedAdherenceRepository.Execute(dateOnly);
			return _detailedAdherenceMapper.Map(data);
		}

        public ICollection<QueueMetricsViewModel> CreateQueueMetricsViewModel(DateOnly dateOnly)
        {
            var data = _queueMetricsForDayQuery.Execute(dateOnly);
            return _queueMetricsMapper.Map(data);
        }
	}
}