using System.Collections.Generic;
using System.Windows.Forms;
using Teleopti.Ccc.Domain.Infrastructure;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Infrastructure.Foundation;


namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Meetings.Interfaces
{
    public interface IExportMeetingView
    {
        DialogResult ShowDialog(IMeetingOverviewView owner);

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        IList<DateOnlyPeriod> SelectedDates { get; set; }
        void SetScenarioList(IList<IScenario> scenarios);
        bool ExportEnabled { get; set; }
        void SetTextOnDateSelectionFromTo(string errorText);
        IScenario SelectedScenario { get; }
        string ExportInfoText { get; set; }
        bool ProgressBarVisible { get; set; }
        void ShowDataSourceException(DataSourceException dataSourceException);
    }
}