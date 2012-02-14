using System;
using System.Collections.Generic;
using System.Drawing;
using Syncfusion.Windows.Forms.Grid;
using Teleopti.Interfaces.Domain;
using Teleopti.Ccc.WinCode.Common.Clipboard;

namespace Teleopti.Ccc.WinCode.Scheduling.RestrictionSummary
{
    public interface IRestrictionSummaryView:IScheduleViewBase
    {
        int RestrictionGridRowCount { get; }
        void InitializePreferenceView();
        void CellDataLoaded();
        void UpdateRowCount();
        Font CellFontBig { get; }
        Font CellFontSmall { get; }
        Font TimelineFont { get; }
        GridControl ViewGrid { get; }
        bool HasHelp { get; }
        string HelpId { get; }
        SchedulePresenterBase Presenter { get; set; }
        event EventHandler<EventArgs> ViewPasteCompleted;
        void LoadScheduleViewGrid();
        int CalculateColHeadersWidth();
        DateOnly SelectedDateLocal();
        IList<IScheduleDay> DeleteList<T>(ClipHandler<T> clipHandler);
        IEnumerable<IPerson> AllSelectedPersons();
        IList<IPerson> PersonsInDestination();
        int GetRowForAgent(IEntity person);
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1716:IdentifiersShouldNotMatchKeywords", MessageId = "Date")]
        int GetColumnForDate(DateTime date);
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1716:IdentifiersShouldNotMatchKeywords", MessageId = "Date")]
        Point GetCellPositionForAgentDay(IEntity person, DateTime date);
        void Dispose();
    	void UpdateEditor();

    }
}