namespace Teleopti.Ccc.WinCode.Forecasting
{
    public class DetailedJobHistoryPresenter
    {
        private readonly IJobHistoryView _view;
        private readonly IDetailedJobHistoryProvider _jobDetailHistoryProvider;

        public DetailedJobHistoryPresenter(IJobHistoryView view, IDetailedJobHistoryProvider jobDetailHistoryProvider)
        {
            _view = view;
            _jobDetailHistoryProvider = jobDetailHistoryProvider;
        }

        public void LoadDetailedHistory(JobResultModel jobResultModel)
        {
            var jobHistoryEntries = _jobDetailHistoryProvider.GetHistory(jobResultModel);
            _view.BindJobDetailData(jobHistoryEntries);
        }
    }
}
