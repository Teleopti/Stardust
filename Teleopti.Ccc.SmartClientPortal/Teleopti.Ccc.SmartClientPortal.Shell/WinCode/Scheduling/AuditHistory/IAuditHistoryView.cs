using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Infrastructure.Foundation;

namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Scheduling.AuditHistory
{
    public interface IAuditHistoryView
    {
        IScheduleDay SelectedScheduleDay { get; }
        void CloseView();
        bool EnableView { get; set; }
        void ShowView();
        void StartBackgroundWork(AuditHistoryDirection direction);
        bool LinkLabelLaterStatus { get; set; }
        bool LinkLabelEarlierStatus { get; set; }
	    bool LinkLabelLaterVisibility { get; set; }
	    bool LinkLabelEarlierVisibility { get; set; }
	    void SetRestoreButtonStatus();
        void ShowWaitCursor();
        void ShowDefaultCursor();
        void RefreshGrid();
        void ShowDataSourceException(DataSourceException dataSourceException);
        void UpdateHeaderText();
        void UpdatePageOfStatusText();
        void SelectFirstRowOnGrid();
    }
}
