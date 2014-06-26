using System;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.WinCode.Common;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCode.Meetings.Interfaces
{
    public interface IMeetingOverviewView: IViewBase
    {
        DateTime SelectedDate { get; }
        IMeeting SelectedMeeting { get; }
        TimeZoneInfo UserTimeZone { get; set; }
        bool ConfirmDeletion(IMeeting theMeeting);
        void ReloadMeetings();
        void EditMeeting(IMeetingViewModel meetingViewModel, IToggleManager toggleManager);
        bool EditEnabled { get; set; }
        bool DeleteEnabled { get; set; }
        bool CopyEnabled { get; set; }
        bool PasteEnabled { get; set; }
        bool CutEnabled { get; set; }
        bool AddEnabled { get; set; }
        bool ExportEnabled { get; set; }
        void SetInfoText(string text);
        DateTimePeriod SelectedPeriod();
        void ShowErrorMessage(string message);
        DateOnlyPeriod SelectedWeek { get; }
        void ShowDataSourceException(DataSourceException dataSourceException);
    	bool FetchForCurrentUser { get; }
    }
}