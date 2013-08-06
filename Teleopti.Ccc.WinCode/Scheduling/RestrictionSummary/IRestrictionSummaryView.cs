namespace Teleopti.Ccc.WinCode.Scheduling.RestrictionSummary
{
    public interface IRestrictionSummaryView:IScheduleViewBase
    {
        int RestrictionGridRowCount { get; }
        void CellDataLoaded();
        void UpdateRowCount();
        SchedulePresenterBase Presenter { get; set; }
        void UpdateEditor();

    }
}