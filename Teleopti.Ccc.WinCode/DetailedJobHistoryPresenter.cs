using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Teleopti.Ccc.WinCode.Forecasting
{
    public class DetailedJobHistoryPresenter
    {
        private readonly IDetailedJobHistoryView _view;
        private readonly IDetailedJobHistoryProvider _jobHistoryProvider;

        public DetailedJobHistoryPresenter(IDetailedJobHistoryView view, IDetailedJobHistoryProvider jobHistoryProvider)
        {
            _view = view;
        	_jobHistoryProvider = jobHistoryProvider;

            //_pagingDetail = pagingDetail;
            //_pagingDetail.PropertyChanged += _pagingDetail_PropertyChanged;
        }

        public void LoadDetailedHistory(JobResultModel jobResultModel)
        {
            var jobHistoryEntries = _jobHistoryProvider.GetHistory(jobResultModel);
            _view.BindData(jobHistoryEntries);
        }
    }
}
