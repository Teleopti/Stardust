using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCode.Budgeting.Views
{
    public interface IBudgetGroupMainView
    {
    	void OnAddShrinkageRow(ICustomShrinkage customShrinkage);
    	void NotifyCustomShrinkageUpdatedByOthers();
    	void OnAddEfficiencyShrinkageRow(ICustomEfficiencyShrinkage customEfficiencyShrinkage);
    	void OnDeleteShrinkageRows(IEnumerable<ICustomShrinkage> customShrinkages);
    	void OnDeleteEfficiencyShrinkageRows(IEnumerable<ICustomEfficiencyShrinkage> customEfficiencyShrinkages);
    	void SetText(string windowTitle);
        void ShowMonthView();
        void ShowWeekView();
        void ShowDayView();
        bool MonthView { get; set; }
        bool WeekView { get; set; }
        bool DayView { get; set; }
        ViewType SelectedView { get; set; }
        void UpdateShrinkageProperty(ICustomShrinkage customShrinkage);
        void UpdateEfficiencyShrinkageProperty(ICustomEfficiencyShrinkage customEfficiencyShrinkage);
    }
}
