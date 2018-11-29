using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Infrastructure.WebReports;
using Teleopti.Ccc.Web.Areas.MyTime.Core.MyReport.Mapping;
using Teleopti.Ccc.Web.Areas.MyTime.Models.MyReport;

using Teleopti.Ccc.Domain.Security.AuthorizationData;

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
		private readonly IPermissionProvider _permissionProvider;
		private readonly ILoggedOnUser _loggedOnUser;

	    public MyReportViewModelFactory(IDailyMetricsForDayQuery dailyMetricsRepository,
	        IDetailedAdherenceForDayQuery detailedAdherenceRepository, IDailyMetricsMapper dailyMetricsMapper,
	        IDetailedAdherenceMapper detailedAdherenceMapper,IQueueMetricsForDayQuery queueMetricsForDayQuery,
			IQueueMetricsMapper queueMetricsMapper, IPermissionProvider permissionProvider, ILoggedOnUser loggedOnUser)
	    {
	        _dailyMetricsRepository = dailyMetricsRepository;
	        _detailedAdherenceRepository = detailedAdherenceRepository;
	        _dailyMetricsMapper = dailyMetricsMapper;
	        _detailedAdherenceMapper = detailedAdherenceMapper;
	        _queueMetricsForDayQuery = queueMetricsForDayQuery;
	        _queueMetricsMapper = queueMetricsMapper;
		    _permissionProvider = permissionProvider;
		    _loggedOnUser = loggedOnUser;
	    }

	    public DailyMetricsViewModel CreateDailyMetricsViewModel(DateOnly dateOnly)
		{
			var data = _dailyMetricsRepository.Execute(dateOnly);

			return _dailyMetricsMapper.Map(data);
		}

		public DetailedAdherenceViewModel CreateDetailedAherenceViewModel(DateOnly dateOnly)
		{
			var detailData = new List<DetailedAdherenceForDayResult>();
			if (_permissionProvider.IsPersonSchedulePublished(dateOnly, _loggedOnUser.CurrentUser())
				|| _permissionProvider.HasApplicationFunctionPermission(DefinedRaptorApplicationFunctionPaths.ViewUnpublishedSchedules))
			{
				detailData = _detailedAdherenceRepository.Execute(dateOnly).ToList();
			}

			return _detailedAdherenceMapper.Map(detailData);
		}

        public ICollection<QueueMetricsViewModel> CreateQueueMetricsViewModel(DateOnly dateOnly)
        {
            var data = _queueMetricsForDayQuery.Execute(dateOnly);
            return _queueMetricsMapper.Map(data);
        }

		public bool HasMyReportPermission()
		{
			return _permissionProvider.HasApplicationFunctionPermission(
				DefinedRaptorApplicationFunctionPaths.MyReportWeb);
		}
	}
}