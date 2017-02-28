using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.WinCode.Common;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCode.Intraday
{
    /// <summary>
    /// Interface for intraday view
    /// </summary>
    /// <remarks>
    /// Created by: robink
    /// Created date: 2008-10-21
    /// </remarks>
    public interface IIntradayView : IViewBase
    {
        void DrawSkillGrid();
        ISkill SelectedSkill { get; }
        void SelectSkillTab(ISkill skill);
        void SetupSkillTabs();
        bool InvokeRequired { get; }
        RightToLeft RightToLeft { get; }
        IAsyncResult BeginInvoke(Delegate method, params object[] args);
        void EnableSave();
        void UpdateFromEditor();
        void DisableSave();
        void RefreshPerson(IPerson person);
        void UpdateShiftEditor(IList<IScheduleDay> scheduleParts);
        void ToggleSchedulePartModified(bool enable);
        void SetChartButtons(bool enabled, AxisLocation location, ChartSeriesDisplayType type, Color color);
        void AddControlHelpContext(Control view);
        void StartProgress();
        void FinishProgress();
        void RefreshRealTimeScheduleControls();
        void ReloadScheduleDayInEditor(IPerson person);
        void ShowBackgroundDataSourceError();
        void HideBackgroundDataSourceError();
		IScenario Scenario { get; set; }
    }
}