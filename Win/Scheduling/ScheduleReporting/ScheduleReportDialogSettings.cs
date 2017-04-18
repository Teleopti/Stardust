using System;
using Teleopti.Ccc.Domain.SystemSetting;
using Teleopti.Ccc.WinCode.Scheduling.ScheduleReporting;

namespace Teleopti.Ccc.Win.Scheduling.ScheduleReporting
{
    [Serializable]
    public class ScheduleReportDialogSettings : SettingValue
    {
        private ScheduleReportDetail _detailLevel = ScheduleReportDetail.All;
        private bool _singleFile = true;
        private bool _individualReport = true;
        private bool _showPublicNote;

        public ScheduleReportDetail DetailLevel
        {
            get { return _detailLevel; }
            set { _detailLevel = value; }
        }

        public bool SingleFile
        {
            get { return _singleFile; }
            set { _singleFile = value; }
        }

        public bool IndividualReport
        {
            get { return _individualReport; }
            set { _individualReport = value; }
        }

        public bool ShowPublicNote
        {
            get { return _showPublicNote; }
            set { _showPublicNote = value; }
        }
    }
}