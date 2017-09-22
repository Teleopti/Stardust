using System;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.WpfControls.Common.Interop;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common;

namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.WpfControls.Common
{

    /// <summary>
    /// Responsible to create a dialog, set the content and result
    /// </summary>
    /// <remarks>
    /// Created by: henrika
    /// Created date: 2010-08-26
    /// </remarks>
    public class AddDialogComposer<T> where T : IDialogResult
    {
        private readonly T _viewModel;
    	private readonly TimeZoneInfo _timeZoneInfo;


        public AddDialogComposer(T content, TimeZoneInfo timeZoneInfo)
        {
            _viewModel = content;
        	_timeZoneInfo = timeZoneInfo;
        }
        

        public T Result()
        {
            OkCancelWindow w = new OkCancelWindow() { DataContext = _viewModel };
            _viewModel.Result =  w.ShowDialogFromWinForms(true, _timeZoneInfo) ?? false;
            return _viewModel;
        }

    }
}
