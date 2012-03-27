using System;
using System.ComponentModel;
using Teleopti.Ccc.Domain.Repositories;

namespace Teleopti.Ccc.WinCode.Forecasting
{
    public class JobHistoryPresenter
    {
        private readonly IJobHistoryView _view;
        private readonly IJobHistoryProvider _jobHistoryProvider;
    	private readonly PagingDetail _pagingDetail;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "2")]
		public JobHistoryPresenter(IJobHistoryView view, IJobHistoryProvider jobHistoryProvider, PagingDetail pagingDetail)
        {
            _view = view;
        	_jobHistoryProvider = jobHistoryProvider;
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

        private void loadHistory()
        {
        	var jobHistoryEntries = _jobHistoryProvider.GetHistory(_pagingDetail);
				_view.BindData(jobHistoryEntries);
        }

    	public void ReloadHistory()
    	{
    		loadHistory();
    	}
    }
}