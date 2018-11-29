using System;
using System.Windows;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.WpfControls.Controls.Intraday.Models;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common.GuiHelpers;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Intraday;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Scheduling.Editor;


namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.WpfControls.Controls.Intraday
{
    /// <summary>
    /// Wrapper for the Intraday grid
    /// </summary>
    /// <remarks>
    /// All public accessors can be put in this control
    /// Replaces the Intraday DayLayerView
    /// Created by: henrika
    /// Created date: 2009-06-01
    /// </remarks>
    public partial class RealTimeScheduleControl : IIntradaySchedulesControl
    {
        private RealTimeScheduleViewModel _model;
        
        public RealTimeScheduleControl()
        {
            InitializeComponent();
            if (StateHolderReader.IsInitialized) VisualTreeTimeZoneInfo.SetTimeZoneInfo(this, TeleoptiPrincipal.CurrentPrincipal.Regional.TimeZone);
        }

        public RealTimeScheduleViewModel Model
        {
            get { return _model; }
        }

        #region  from DayLayerView
        public void SetDayLayerViewModel(IDayLayerViewModel dayLayerViewModel)
        {
            _model = new RealTimeScheduleViewModel(null, new CreateLayerViewModelService(), dayLayerViewModel);
            DataContext = Model;
            if (UpdateShiftEditor != null) 
				UpdateShiftEditor(this, new ShiftEditorEventArgs(null)); //remove just for fx.....
        }

        public void RefreshProjection(IPerson person)
        {
            Model.DayLayerViewModel.RefreshProjection(person);
        }

        //Handle selections for shifteditor
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        private void SelectShift(object sender, RoutedEventArgs e)
        {
            var viewModel = (e.OriginalSource as FrameworkElement).DataContext as LayerViewModel;
            if (viewModel == null) 
				return;
            if (UpdateShiftEditor != null) 
				UpdateShiftEditor(this, new ShiftEditorEventArgs(viewModel.SchedulePart));
        }

        public event EventHandler<ShiftEditorEventArgs> UpdateShiftEditor;
        #endregion

        public void UnregisterMessageBrokerEvents()
        {
	        if (Model != null)
		        Model.DayLayerViewModel.UnregisterMessageBrokerEvent();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            var helpContext = HelpProvider.GetHelpString(this);
            if (string.IsNullOrEmpty(helpContext) && Parent == null) 
				return;

            var elementParent = Parent as FrameworkElement;
            if (elementParent == null || elementParent.Parent == null) 
				return;

            HelpProvider.SetHelpString(elementParent.Parent, helpContext);
        }

        public void ReleaseManagedResources()
        {
	        if (Model != null)
		        Model.TimelineModel.ShowNowPeriod = false;
        }
    }
}
