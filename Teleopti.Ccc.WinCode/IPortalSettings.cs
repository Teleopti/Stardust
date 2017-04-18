namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common
{
    public interface IPortalSettings
    {
        int NumberOfVisibleGroupBars { get; set; }
        int SchedulerActionPaneHeight { get; set; }
        string LastModule { get; set; }
        int ModuleSelectorPanelHeight { get; set; }
        int IntradayActionPaneHeight { get; set; }
        int ForecasterActionPaneHeight { get; set; }
        int BudgetingActionPaneHeight { get; set; }
        int PayrollActionPaneHeight { get; set; }
        int PeopleActionPaneHeight { get; set; }
    }
}