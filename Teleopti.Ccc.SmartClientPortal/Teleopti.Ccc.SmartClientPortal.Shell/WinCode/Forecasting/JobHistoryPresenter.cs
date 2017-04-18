using System;
using System.ComponentModel;
using Teleopti.Ccc.Domain.Repositories;

namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Forecasting
{
    public class JobHistoryPresenter
    {
        private readonly IJobHistoryView _view;
        private readonly IJobResultProvider _jobResultProvider;
    	private readonly PagingDetail _pagingDetail;
		
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "2")]
		public JobHistoryPresenter(IJobHistoryView view, IJobResultProvider jobResultProvider, PagingDetail pagingDetail)
        {
            _view = view;
        	_jobResultProvider = jobResultProvider;
        	_pagingDetail = pagingDetail;
        	_pagingDetail.PropertyChanged += pagingDetailPropertyChanged;
        }

    	private void pagingDetailPropertyChanged(object sender, PropertyChangedEventArgs e)
    	{
    		_view.TogglePrevious(_pagingDetail.Skip > 0);
    		_view.ToggleNext(_pagingDetail.Skip + _pagingDetail.Take < _pagingDetail.TotalNumberOfResults);

    		_view.SetResultDescription(string.Format(System.Globalization.CultureInfo.CurrentUICulture,
    		                                         UserTexts.Resources.PageOf, (_pagingDetail.Skip/_pagingDetail.Take)+1,
													 (int)Math.Ceiling(_pagingDetail.TotalNumberOfResults / (double)_pagingDetail.Take)));
    	}

    	public void Initialize()
        {
            loadHistory();
        }

        public void Next()
        {
        	_pagingDetail.Skip += _pagingDetail.Take;
            loadHistory();
        }

		public void Previous()
		{
			_pagingDetail.Skip -= _pagingDetail.Take;
			loadHistory();
		}

        public void LoadDetailedHistory(JobResultModel jobResultModel)
        {
			var jobHistoryEntries = _jobResultProvider.GetJobResultDetails(jobResultModel, _view.DetailLevel);
            _view.BindJobResultDetailData(jobHistoryEntries);
        }

        private void loadHistory()
        {
        	var jobHistoryEntries = _jobResultProvider.GetJobResults(_pagingDetail);
				_view.BindJobResultData(jobHistoryEntries);
        }

    	public void ReloadHistory()
    	{
    		loadHistory();
    	}
    }
}